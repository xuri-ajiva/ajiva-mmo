using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WgslGenerator;

[Flags]
public enum ShaderStage : int
{
    None = 0x0,
    Vertex = 0x1,
    Fragment = 0x2,
    Compute = 0x4,
    Force32 = 0x7FFFFFFF,
}

public enum VertexFormat : int
{
    Undefined = 0x0,
    Uint8x2 = 0x1,
    Uint8x4 = 0x2,
    Sint8x2 = 0x3,
    Sint8x4 = 0x4,
    Unorm8x2 = 0x5,
    Unorm8x4 = 0x6,
    Snorm8x2 = 0x7,
    Snorm8x4 = 0x8,
    Uint16x2 = 0x9,
    Uint16x4 = 0xA,
    Sint16x2 = 0xB,
    Sint16x4 = 0xC,
    Unorm16x2 = 0xD,
    Unorm16x4 = 0xE,
    Snorm16x2 = 0xF,
    Snorm16x4 = 0x10,
    Float16x2 = 0x11,
    Float16x4 = 0x12,
    Float32 = 0x13,
    Float32x2 = 0x14,
    Float32x3 = 0x15,
    Float32x4 = 0x16,
    Uint32 = 0x17,
    Uint32x2 = 0x18,
    Uint32x3 = 0x19,
    Uint32x4 = 0x1A,
    Sint32 = 0x1B,
    Sint32x2 = 0x1C,
    Sint32x3 = 0x1D,
    Sint32x4 = 0x1E,
    Force32 = 0x7FFFFFFF,
}

/// <summary>
/// Represents a binding group in the shader
/// </summary>
public class BindGroupInfo
{
    public uint GroupId { get; set; }
    public List<BindingEntryInfo> Entries { get; } = new();
}

/// <summary>
/// Represents a binding entry in a binding group
/// </summary>
public class BindingEntryInfo
{
    public uint Binding { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string? TextureFormat { get; set; }
    public BindingType BindingType { get; set; }
    public ShaderStage Visibility { get; set; } = ShaderStage.Fragment | ShaderStage.Vertex;
}

/// <summary>
/// Represents a vertex attribute in the shader
/// </summary>
public class VertexAttributeInfo
{
    public string Name { get; set; } = string.Empty;
    public uint Location { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public VertexFormat VertexFormat { get; set; }
    public string StructName { get; set; } = string.Empty;
}

/// <summary>
/// Represents shader entry points
/// </summary>
public class ShaderEntryInfo
{
    public TypeInOut? Vertex { get; set; }
    public TypeInOut? Fragment { get; set; }
    public TypeInOut? Compute { get; set; }
}

/// <summary>
/// Types of bindings
/// </summary>
public enum BindingType
{
    Undefined,

    // BufferBindingType
    Uniform,
    Storage,
    ReadOnlyStorage,

    // SamplerBindingType
    Filtering,
    NonFiltering,
    Comparison,

    // for texture
    Texture,
}

public enum AttributeType
{
    Builtin,
    Location,
    VertexShader,
    FragmentShader,
    ComputeShader,
    WorkgroupSize,
    Group,
    Binding,
    BindingMode,
}

public class TypeInOut
{
    public string EntryPoint { get; set; }
    public string ReturnType { get; set; }
    public string[] Parameters { get; set; }
}

internal class WgslVisitor : CSharpSyntaxVisitor
{
    private readonly SourceProductionContext _context;
    private readonly Compilation _compilation;
    private readonly ClassDeclarationSyntax _classDeclarationSyntax;
    private readonly StringBuilder _builder;
    private readonly IndentedTextWriter _writer;
    private readonly TypeMapper _typemapper = new();

    // Collection of binding groups
    private readonly Dictionary<uint, BindGroupInfo> _bindGroups = new();

    // Fast lockup of Uniforms
    private readonly Dictionary<string, BindingEntryInfo> _uniforms = new();

    // Collection of input attributes
    private readonly List<List<VertexAttributeInfo>> _inAttributes = new();

    // Collection of output attributes
    private List<VertexAttributeInfo>? _outAttribute;

    // Collection of shader structs
    private readonly Dictionary<string, List<VertexAttributeInfo>> _structAttributes = new();

    private readonly List<MemberDeclarationSyntax> _generatedStructs = new();

    private readonly List<string> _warnings = [];

    private ShaderEntryInfo _entryPoints = new();

    private uint _vertexInCounter = 0;
    private uint _varyngsCounter = 0;
    private uint _targetCounter = 0;

    public WgslVisitor(SourceProductionContext context,
        Compilation compilation,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        _context = context;
        _compilation = compilation;
        _classDeclarationSyntax = classDeclarationSyntax;
        _builder = new StringBuilder();
        _writer = new IndentedTextWriter(new StringWriter(_builder));
    }

    public IReadOnlyDictionary<uint, BindGroupInfo> GetBindingGroups() => _bindGroups;

    public IReadOnlyList<IReadOnlyList<VertexAttributeInfo>> GetInputAttributes() => _inAttributes;

    public IReadOnlyList<VertexAttributeInfo>? GetOutputAttribute() => _outAttribute;

    public ShaderEntryInfo GetEntryPoints() => _entryPoints;

    public IReadOnlyDictionary<string, List<VertexAttributeInfo>> GetStructAttributes() => _structAttributes;

    public IReadOnlyList<MemberDeclarationSyntax> GetGeneratedStructs() => _generatedStructs;
    public string[] GetWarnings() => _warnings.ToArray();

    public string GetResult()
    {
        _writer.Flush();
        return _builder.ToString();
    }


    public override void DefaultVisit(SyntaxNode node)
    {
        throw new NotSupportedException($"Visit{node.GetType().Name} is not implemented." +
                                        node.GetLocation().GetLineSpan());
    }

    private TypeInOut ParseInOut(MethodDeclarationSyntax syntax) => new()
    {
        EntryPoint = syntax.Identifier.Text,
        ReturnType = _typemapper.Map(syntax.ReturnType),
        Parameters = syntax.ParameterList.Parameters
            .Select(p => _typemapper.Map(p.Type!))
            .ToArray()
    };

    #region Visitor

    // this is only allowed for the origional class declle;
    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node != _classDeclarationSyntax)
        {
            throw new NotImplementedException("no nested calsses allowed, use structs instead");
        }

        foreach (var member in node.Members.OfType<TypeDeclarationSyntax>())
        {
            _typemapper.UserTypes.Add(member.Identifier.Text);
        }

        foreach (var syntax in node.Members.OfType<MethodDeclarationSyntax>())
        {
            foreach (var attribute in syntax.AttributeLists.SelectMany(x => x.Attributes))
            {
                var type = GetAttributeType(attribute);
                switch (type)
                {
                    case AttributeType.VertexShader:
                        _entryPoints.Vertex = ParseInOut(syntax);
                        break;

                    case AttributeType.FragmentShader:
                        _entryPoints.Fragment = ParseInOut(syntax);
                        break;

                    case AttributeType.ComputeShader:
                        _entryPoints.Compute = ParseInOut(syntax);
                        break;
                }
            }
        }

        if (_entryPoints is { Vertex: { } vertex, Fragment: { } fragment })
        {
            if (!fragment.Parameters.Contains(vertex.ReturnType))
            {
                _warnings.Add("Fragment shader does not use vertex shader output");
            }
        }

        _writer.WriteLine("//<auto-generated>");

        _writer.WriteLine("\n// <begin-declarations/>\n");
        foreach (var member in node.Members.OfType<TypeDeclarationSyntax>())
        {
            Visit(member);
        }

        _writer.WriteLine("\n// <begin-fields/>\n");
        foreach (var member in node.Members.OfType<FieldDeclarationSyntax>())
        {
            Visit(member);
        }

        _writer.WriteLine("\n// <begin-methods/>\n");
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            Visit(method);
        }

        _writer.WriteLine("\n// <end-type/>\n");
    }


    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var structName = node.Identifier.Text;
        _typemapper.UserTypes.Add(structName);
        _writer.WriteLine($"struct {structName} {{");

        var attributeList = new List<VertexAttributeInfo>();
        _structAttributes[structName] = attributeList;

        var fieldsImplementation = StructDeclaration(Identifier(structName))
            .WithModifiers(node.Modifiers);

        uint dummy = 0;
        ref var counter = ref dummy;
        if (_entryPoints.Vertex?.Parameters.Contains(structName) == true)
        {
            _inAttributes.Add(attributeList);
            counter = ref _vertexInCounter;
        }
        else if (_entryPoints.Vertex?.ReturnType == structName)
        {
            counter = ref _varyngsCounter;
        }
        else if (_entryPoints.Fragment?.ReturnType == structName)
        {
            if (_outAttribute is not null)
                _warnings.Add($"Fragment shader has multiple output structs, using {structName} as output");
            _outAttribute ??= attributeList;
            counter = ref _targetCounter;
        }


        if (node.ParameterList is not null)
        {
            _writer.Indent++;
            foreach (var parameter in node.ParameterList.Parameters)
            {
                // add fields to struct
                var fieldName = parameter.Identifier.Text.ToPascalCase();
                fieldsImplementation = fieldsImplementation.AddMembers(FieldDeclaration(
                        VariableDeclaration(parameter.Type!)
                            .WithVariables(SingletonSeparatedList(
                                VariableDeclarator(Identifier(fieldName))
                                    .WithInitializer(EqualsValueClause(IdentifierName(parameter.Identifier.Text)
                                    ))))
                    )
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                );
                // for vectors generate reinterpretcast using:
                // public unsafe ref Vector4 NormalVector => ref Unsafe.AsRef<Vector4>(Unsafe.AsPointer(ref Normal));
                if (parameter.Type is IdentifierNameSyntax { Identifier.Text: { Length: 4 } s } &&
                    s.StartsWith("vec") && s[3] is var vDim)
                {
                    fieldsImplementation = fieldsImplementation.AddMembers(PropertyDeclaration(
                            ParseTypeName($"Vector{vDim}"),
                            Identifier(fieldName + "Ref"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.UnsafeKeyword),
                            Token(SyntaxKind.RefKeyword)))
                        .WithExpressionBody(ArrowExpressionClause(
                            ParseExpression($"ref Unsafe.AsRef<Vector{vDim}>(Unsafe.AsPointer(ref {fieldName}))")))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    );
                }
                else if (parameter.Type is IdentifierNameSyntax { Identifier.Text: { Length: 4 } m } &&
                         m.StartsWith("mat") && m[3] is var mDim)
                {
                    fieldsImplementation = fieldsImplementation.AddMembers(PropertyDeclaration(
                            ParseTypeName($"Matrix{mDim}x{mDim}"),
                            Identifier(fieldName + "Ref"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.UnsafeKeyword),
                            Token(SyntaxKind.RefKeyword)))
                        .WithExpressionBody(ArrowExpressionClause(
                            ParseExpression(
                                $"ref Unsafe.AsRef<Matrix{mDim}x{mDim}>(Unsafe.AsPointer(ref {fieldName}))")))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    );
                }

                uint? location = null;
                bool buildIn = false;
                foreach (var attribute in parameter.AttributeLists.SelectMany(a => a.Attributes).ToList())
                {
                    var attributeType = GetAttributeType(attribute);
                    if (attributeType == AttributeType.Builtin)
                        buildIn = true;
                    if (attributeType != AttributeType.Location)
                    {
                        VisitAttributeTyped(attribute, attributeType);
                    }
                    else if (attribute.ArgumentList?.Arguments.First().Expression.ToString() is { } str
                             && uint.TryParse(str, out var loc))
                    {
                        location = loc;
                    }
                }

                uint finLocation;
                if (location is null)
                {
                    finLocation = counter;
                }
                else
                {
                    finLocation = (uint)location;
                    if (counter < finLocation)
                        counter = finLocation;
                }

                // builtin location is not allowed
                if (!buildIn)
                {
                    _writer.WriteLine($"@location({finLocation})");
                    counter++;
                }
                else
                {
                    _writer.WriteLine();
                }

                _writer.WriteLine($"{fieldName}: {_typemapper.Map(parameter.Type!)},");

                // Record vertex attribute information
                var attrInfo = new VertexAttributeInfo
                {
                    Name = fieldName,
                    Location = finLocation,
                    TypeName = parameter.Type!.ToString(),
                    StructName = structName,
                    VertexFormat = _typemapper.MapTypeToVertexFormat(parameter.Type!.ToString())
                };

                attributeList.Add(attrInfo);
            }

            _writer.Indent--;
        }

        _generatedStructs.Add(fieldsImplementation);
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var fieldName = node.Declaration.Variables.First().Identifier.Text;
        var typeName = node.Declaration.Type.ToString();

        uint groupId = 0;
        var bindingType = BindingType.Undefined;

        string? textureFormat = null;
        if (typeName.Contains("texture_"))
        {
            bindingType = BindingType.Texture;

            // Float, UnfilterableFloat, Depth, Sint, Uint
            if (typeName.Contains("depth"))
            {
                textureFormat = "Depth";
            }
            else if (node.Declaration.Type is GenericNameSyntax { TypeArgumentList: { Arguments: var args } })
            {
                var typeSyntax = args.First();
                var semanticModel = _compilation.GetSemanticModel(typeSyntax.SyntaxTree);
                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                var typeSymbol = typeInfo.Type;

                if (typeSymbol != null)
                {
                    var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    textureFormat = fullTypeName switch
                    {
                        "System.Single" or "f32" or "float" => "Float",
                        "System.UInt32" or "u32" or "uint" => "Uint",
                        "System.Int32" or "i32" or "int" => "Sint",
                        _ => textureFormat
                    };
                }
                else
                {
                    // Fallback to string-based approach if semantic analysis fails
                    var typeString = typeSyntax.ToString();
                    textureFormat = typeString switch
                    {
                        "f32" or "float" => "Float",
                        "u32" or "uint" => "Uint",
                        "i32" or "int" => "Sint",
                        _ => textureFormat
                    };
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported type {typeName}, Can't determine texture format");
            }
        }

        bool seenBindingId = false;
        foreach (var attribute in node.AttributeLists.SelectMany(a => a.Attributes).ToList())
        {
            var attributeType = GetAttributeType(attribute);
            VisitAttributeTyped(attribute, attributeType);

            var expressionSyntax = attribute.ArgumentList?.Arguments.First()?.Expression;
            if (expressionSyntax is null) throw new InvalidOperationException("Access attribute must have an argument");

            switch (attributeType)
            {
                case AttributeType.BindingMode:
                    switch (expressionSyntax)
                    {
                        case null:
                            throw new InvalidOperationException("Access attribute must have an argument");
                        case MemberAccessExpressionSyntax { Name.Identifier.Text: var bindType }:
                            if (!Enum.TryParse<BindingType>(bindType, out bindingType))
                            {
                                throw new InvalidOperationException($"Unknown binding type {bindType}");
                            }

                            if (typeName.Contains("sampler") &&
                                bindingType is not (BindingType.Filtering or BindingType.NonFiltering
                                    or BindingType.Comparison))
                            {
                                throw new InvalidOperationException(
                                    "Sampler must have filtering, non-filtering or comparison access");
                            }

                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Did not know what to do with {expressionSyntax.ToString()}");
                    }

                    break;
                case AttributeType.Group:
                    groupId = uint.Parse(expressionSyntax.ToString());
                    break;
                case AttributeType.Binding:
                    seenBindingId = true;
                    break;
            }
        }

        if (bindingType == BindingType.Undefined)
            bindingType = typeName.Contains("sampler") ? BindingType.Filtering : BindingType.Uniform;

        // Add to binding groups
        if (!_bindGroups.TryGetValue(groupId, out var bindGroup))
        {
            bindGroup = new BindGroupInfo { GroupId = groupId };
            _bindGroups[groupId] = bindGroup;
        }

        var bindingEntryInfo = new BindingEntryInfo
        {
            Binding = (uint)bindGroup.Entries.Count,
            Name = fieldName,
            TypeName = typeName,
            BindingType = bindingType,
            TextureFormat = textureFormat
        };
        if (!seenBindingId)
        {
            _writer.WriteLine($"@binding({bindingEntryInfo.Binding})");
        }
        else
        {
            _writer.WriteLine();
        }

        bindGroup.Entries.Add(bindingEntryInfo);


        switch (bindingType)
        {
            case BindingType.Undefined:
                throw new NotImplementedException("Binding type must be defined");
            case BindingType.Uniform:
                _writer.Write("var<uniform> ");
                break;
            case BindingType.Storage:
                _writer.Write("var<storage, read_write> ");
                break;
            case BindingType.ReadOnlyStorage:
                _writer.Write("var<storage, read> ");
                break;
            case BindingType.Filtering:
            case BindingType.NonFiltering:
            case BindingType.Comparison:
            case BindingType.Texture:
                _writer.Write("var ");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _writer.WriteLine(
            $"{node.Declaration.Variables.First().Identifier.Text}: {_typemapper.Map(node.Declaration.Type)};");
    }

    public override void VisitAttribute(AttributeSyntax node) => VisitAttributeTyped(node, GetAttributeType(node));

    private AttributeType GetAttributeType(AttributeSyntax node) =>
        GetAttributeType(node.Name.ToString());

    private AttributeType GetAttributeType(string name)
    {
        return Enum.TryParse(name, true, out AttributeType attributeType)
            ? attributeType
            : throw new NotImplementedException($"Attribute {name} is not implemented.");
    }

    private void VisitAttributeTyped(AttributeSyntax node, AttributeType type)
    {
        switch (type)
        {
            case AttributeType.Builtin:
                var builtinName = node.ArgumentList?.Arguments.First().Expression.ToString().Trim('\"');
                _writer.Write($"@builtin({builtinName}) ");
                break;

            case AttributeType.Location:
                var location = node.ArgumentList?.Arguments.First().Expression.ToString();
                _writer.Write($"@location({location}) ");

                // Store the location for later vertex attribute generation
                // (will be associated with the parameter or field in the parent method)
                break;

            case AttributeType.VertexShader:
                _writer.Write("@vertex ");
                break;

            case AttributeType.FragmentShader:
                _writer.Write("@fragment ");
                break;

            case AttributeType.ComputeShader:
                _writer.Write("@compute ");
                break;

            case AttributeType.WorkgroupSize:
                _writer.Write($"@workgroup_size{node.ArgumentList} ");
                break;

            case AttributeType.Group:
                _writer.Write($"@group{node.ArgumentList} ");
                break;

            case AttributeType.Binding:
                _writer.Write($"@binding{node.ArgumentList} ");
                break;

            case AttributeType.BindingMode:
                // Special case for var<access flags>
                break;

            default:
                throw new NotImplementedException($"Attribute {type} is not implemented.");
        }
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        _typemapper.KnownMethods.Add(name);
        var attributes = node.AttributeLists.SelectMany(a => a.Attributes).ToList();
        foreach (var attribute in attributes)
        {
            VisitAttribute(attribute);
        }

        if (attributes.Any())
            _writer.WriteLine();

        var returnType = _typemapper.Map(node.ReturnType);
        _writer.Write($"fn {name}(");
        _writer.Indent += 1;
        foreach (var parameter in node.ParameterList.Parameters)
        {
            Visit(parameter);
            if (parameter != node.ParameterList.Parameters.Last())
                _writer.WriteLine(", ");
            else if (node.ParameterList.Parameters.Count > 1) _writer.WriteLine();
        }

        _writer.Indent -= 1;
        var location = "";

        // patching to add location to known types, user types are not our responsibility
        if ((name == _entryPoints.Vertex?.EntryPoint || name == _entryPoints.Fragment?.EntryPoint ||
             name == _entryPoints.Compute?.EntryPoint)
            && _typemapper.KnownTypes.Contains(returnType))
            location = "@location(0) ";

        _writer.Write($") -> {location}{returnType} ");
        Visit(node.Body);
        _writer.WriteLine();
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        var attributes = node.AttributeLists.SelectMany(a => a.Attributes);
        foreach (var attribute in attributes)
        {
            Visit(attribute);
        }

        _writer.Write($"{node.Identifier.Text}: {_typemapper.Map(node.Type!)}");
    }

    public override void VisitBlock(BlockSyntax node)
    {
        _writer.WriteLine("{");
        _writer.Indent++;
        foreach (var statement in node.Statements)
        {
            Visit(statement);
        }

        _writer.Indent--;
        _writer.WriteLine("}");
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        // find if the variable is updated later
        var variable = node.Declaration.Variables.First();
        var variableName = variable.Identifier.Text;

        var updated = node.Parent!.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Any(a => a.Left.ToString() == variableName);

        // ((IdentifierNameSyntax)((InvocationExpressionSyntax)variable.Initializer.Value).Expression).Identifier.Text
        updated |= variable.Initializer is
            { Value: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "array" } } };

        _writer.Write(updated ? "var " : "let ");
        _writer.Write($"{variable.Identifier.Text} = ");
        Visit(variable.Initializer!.Value);
        _writer.WriteLine(";");
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var type = _typemapper.Map(node.Type);
        _writer.Write($"{type}(");

        if (node.ArgumentList is not null)
        {
            for (var index = 0; index < node.ArgumentList.Arguments.Count; index++)
            {
                var argument = node.ArgumentList.Arguments[index];
                Visit(argument.Expression);
                if (index < node.ArgumentList.Arguments.Count - 1)
                    _writer.Write(", ");
            }
        }

        _writer.Write(")");
    }

    public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        _writer.Write(' ');
        _writer.Write(node.OperatorToken.Text);
        Visit(node.Operand);
    }

    public override void VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        _writer.Write(node.Token.Text);
    }

    public override void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
    {
        // becomes array(...)
        _writer.Write("array(");
        for (var index = 0; index < node.Initializer.Expressions.Count; index++)
        {
            var expression = node.Initializer.Expressions[index];
            Visit(expression);
            if (index < node.Initializer.Expressions.Count - 1)
                _writer.Write(", ");
        }

        _writer.Write(")");
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        _writer.Write("return ");
        Visit(node.Expression);
        _writer.WriteLine(";");
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        _writer.Write(node.Identifier.Text);
    }

    public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
    {
        _writer.Write($"{node.Expression}[");
        Visit(node.ArgumentList.Arguments.First().Expression);
        _writer.Write("]");
    }

    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        Visit(node.Left);
        _writer.Write(' ');
        _writer.Write(node.OperatorToken.Text);
        _writer.Write(' ');
        Visit(node.Right);
    }


    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        Visit(node.Expression);
        _writer.Write($".{node.Name.Identifier.Text}");
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is MemberAccessExpressionSyntax access)
        {
            if (access.Name.Identifier.Text == "_raw")
            {
                _writer.Write(access.Expression.ToString().Trim('\"'));
            }

            return;
        }

        var identifierIdentifier = node.Expression switch
        {
            GenericNameSyntax genericNameSyntax => genericNameSyntax.Identifier,
            IdentifierNameSyntax identifier => identifier.Identifier,
            _ => throw new NotImplementedException("For now only generic and identifier names are supported, got: " +
                                                   node.Expression.GetType().Name)
        };

        var methodName = identifierIdentifier.Text;
        if (methodName == "discard")
        {
            _writer.Write("discard");
            return;
        }

        if (!_typemapper.KnownMethods.Contains(methodName))
        {
            if (char.IsUpper(methodName[0]))
            {
                // _diagnosticsHelper.ReportWarning("Method was made lowercase", "Method was made lowercase", node.GetLocation());
                methodName = string.Concat(char.ToLower(methodName[0]), methodName.Substring(1));
            }

            if (!_typemapper.KnownMethods.Contains(methodName))
            {
                // _diagnosticsHelper.ReportWarning($"Unknown method {identifierIdentifier.Text}", "Unknown method", node.GetLocation());
                methodName = methodName switch
                {
                    "normalized" => "normalize",
                    _ => methodName,
                };
            }
        }

        _writer.Write(methodName);
        _writer.Write("(");
        for (var index = 0; index < node.ArgumentList.Arguments.Count; index++)
        {
            var argument = node.ArgumentList.Arguments[index];
            Visit(argument.Expression);
            if (index < node.ArgumentList.Arguments.Count - 1)
                _writer.Write(", ");
        }

        _writer.Write(")");
    }

    public override void VisitCastExpression(CastExpressionSyntax node)
    {
        _writer.Write($"({_typemapper.Map(node.Type)})(");
        Visit(node.Expression);
        _writer.Write(")");
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        _writer.Write($"for ({node.Declaration}; {node.Condition}; {node.Incrementors})");
        Visit(node.Statement);
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        //todo imrove this?
        _writer.Write(node.ToString());
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        _writer.Write("if (");
        Visit(node.Condition);
        _writer.Write(")");
        Visit(node.Statement);
        if (node.Else is not null)
        {
            _writer.Write("else ");
            Visit(node.Else.Statement);
        }
    }

    public override void VisitContinueStatement(ContinueStatementSyntax node)
    {
        _writer.WriteLine("continue;");
    }

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        Visit(node.Expression);
        _writer.WriteLine(";");
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        Visit(node.Left);
        _writer.Write(' ');
        _writer.Write(node.OperatorToken.Text);
        _writer.Write(' ');
        Visit(node.Right);
    }

    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        // make if else
        _writer.Write("if (");
        Visit(node.Condition);
        _writer.Write(") {");
        Visit(node.WhenTrue);
        _writer.Write("} else { ");
        Visit(node.WhenFalse);
        _writer.Write("}");
    }

    public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
    {
        _writer.Write("(");
        Visit(node.Expression);
        _writer.Write(")");
    }

    public override void VisitEmptyStatement(EmptyStatementSyntax node)
    {
        // Empty statement, do nothing
    }

    #endregion
}
