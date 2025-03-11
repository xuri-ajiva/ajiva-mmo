using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace WgslGenerator;

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
    public BindingType BindingType { get; set; }
    public ShaderStage Visibility { get; set; } = ShaderStage.Fragment | ShaderStage.Vertex;
    public ShaderAccess? Access { get; set; }
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
    public bool IsInstance { get; set; }
}

/// <summary>
/// Represents shader entry points
/// </summary>
public class ShaderEntryInfo
{
    public string? VertexEntryPoint { get; set; }
    public string? FragmentEntryPoint { get; set; }
}

/// <summary>
/// Types of bindings
/// </summary>
public enum BindingType
{
    UniformBuffer,
    StorageBuffer,
    Sampler,
    Texture,
    StorageTexture
}

[Flags]
public enum ShaderAccess
{
    Uniform,
    Storage,
    Read,
    Write,
}

internal class WgslVisitor : CSharpSyntaxVisitor
{
    private readonly SourceProductionContext _context;
    private readonly Compilation _compilation;
    private readonly ClassDeclarationSyntax _classDeclarationSyntax;
    private readonly StringBuilder _builder;
    private readonly IndentedTextWriter _writer;
    private TypeMapper _typemapper = new();

    // Collection of binding groups
    private readonly Dictionary<uint, BindGroupInfo> _bindGroups = new();

    // Collection of vertex attributes
    private readonly List<VertexAttributeInfo> _vertexAttributes = new();

    // Collection of instance attributes
    private readonly List<VertexAttributeInfo> _instanceAttributes = new();

    // Shader entry points
    private readonly ShaderEntryInfo _entryPoints = new();

    // Collection of shader structs
    private readonly Dictionary<string, List<VertexAttributeInfo>> _structAttributes = new();

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

    public IReadOnlyList<VertexAttributeInfo> GetVertexAttributes() => _vertexAttributes;

    public IReadOnlyList<VertexAttributeInfo> GetInstanceAttributes() => _instanceAttributes;

    public ShaderEntryInfo GetEntryPoints() => _entryPoints;

    public IReadOnlyDictionary<string, List<VertexAttributeInfo>> GetStructAttributes() => _structAttributes;

    public string GetResult()
    {
        _writer.Flush();
        return _builder.ToString();
    }


    public override void DefaultVisit(SyntaxNode node)
    {
        throw new NotSupportedException($"Visit{node.GetType().Name} is not implemented.");
    }

    #region Visitor

    // this is only allowed for the origional class declle;
    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node != _classDeclarationSyntax)
        {
            throw new NotImplementedException("no nested calsses allowed, use structs instead");
        }

        _writer.WriteLine("//<auto-generated>\n");
        foreach (var member in node.Members)
        {
            Visit(member);
        }
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        _typemapper.KnownTypes.Add(node.Identifier.Text);
        _writer.WriteLine($"struct {node.Identifier.Text} {{");
        _writer.Indent++;
        foreach (var member in node.Members)
        {
            Visit(member);
        }

        _writer.Indent--;
        _writer.WriteLine("}");
    }

    // Override for struct declarations to collect vertex attribute information
    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        if (node.ClassOrStructKeyword.Text == "class")
            throw new NotImplementedException("Only structs are allowed");
        if (node.Members.Any())
            throw new NotImplementedException("Record should use primary constructor");

        var structName = node.Identifier.Text;
        _typemapper.KnownTypes.Add(structName);
        _writer.WriteLine($"struct {structName} {{");

        var attributeList = new List<VertexAttributeInfo>();
        _structAttributes[structName] = attributeList;

        // Check if this struct is for vertex attributes or instance attributes
        bool isInstance = structName.Equals("Instance", StringComparison.OrdinalIgnoreCase);
        bool isVertexAttributes = structName.Equals("Attributes", StringComparison.OrdinalIgnoreCase);

        if (node.ParameterList is not null)
        {
            _writer.Indent++;
            foreach (var member in node.ParameterList.Parameters)
            {
                uint location = 0;
                var attributes = member.AttributeLists.SelectMany(a => a.Attributes).ToList();

                foreach (var attribute in attributes)
                {
                    Visit(attribute);

                    // Extract location for vertex attributes
                    if (attribute.Name.ToString() == "Location" && attribute.ArgumentList != null)
                    {
                        location = uint.Parse(attribute.ArgumentList.Arguments.First().Expression.ToString());
                    }
                }

                if (attributes.Any())
                    _writer.WriteLine();

                _writer.WriteLine($"{member.Identifier.Text}: {_typemapper.Map(member.Type!)},");

                // Record vertex attribute information
                var attrInfo = new VertexAttributeInfo
                {
                    Name = member.Identifier.Text,
                    Location = location,
                    TypeName = member.Type!.ToString(),
                    StructName = structName,
                    IsInstance = isInstance,
                    VertexFormat = _typemapper.MapTypeToVertexFormat(member.Type!.ToString())
                };

                attributeList.Add(attrInfo);

                // Add to vertex or instance attributes list
                if (isVertexAttributes)
                {
                    _vertexAttributes.Add(attrInfo);
                }
                else if (isInstance)
                {
                    _instanceAttributes.Add(attrInfo);
                }
            }

            _writer.Indent--;
        }

        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        // Current implementation...
        var attributes = node.AttributeLists.SelectMany(a => a.Attributes).ToList();

        _writer.Write("");
        AttributeSyntax? accessAttribute = null;
        AttributeSyntax? groupAttribute = null;
        AttributeSyntax? bindingAttribute = null;

        foreach (var attribute in attributes)
        {
            Visit(attribute);

            var name = attribute.Name.ToString();
            switch (name)
            {
                case "Access":
                    accessAttribute = attribute;
                    break;
                case "Group":
                    groupAttribute = attribute;
                    break;
                case "Binding":
                    bindingAttribute = attribute;
                    break;
            }
        }

        _writer.WriteLine();

        // Process for binding information collection
        if (groupAttribute != null && bindingAttribute != null)
        {
            uint groupId = uint.Parse(groupAttribute.ArgumentList!.Arguments.First().Expression.ToString());
            uint bindingId = uint.Parse(bindingAttribute.ArgumentList!.Arguments.First().Expression.ToString());

            string fieldName = node.Declaration.Variables.First().Identifier.Text;
            string typeName = node.Declaration.Type.ToString();

            BindingType bindingType;
            ShaderAccess? access = null;

            // Determine binding type from field type and attributes
            if (typeName.Contains("texture_"))
            {
                bindingType = BindingType.Texture;
            }
            else if (typeName.Contains("sampler"))
            {
                bindingType = BindingType.Sampler;
            }
            else
            {
                // Default to uniform buffer if access attribute specifies it, otherwise storage buffer
                bindingType = BindingType.UniformBuffer;

                if (accessAttribute != null)
                {
                    var arg = accessAttribute.ArgumentList!.Arguments.First();
                    if (arg.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var accessType = memberAccess.Name.Identifier.Text;
                        if (accessType.Contains("Storage"))
                        {
                            bindingType = BindingType.StorageBuffer;
                        }

                        // Parse access type
                        access = (ShaderAccess)Enum.Parse(typeof(ShaderAccess), accessType);
                    }
                }
            }

            // Add to binding groups
            if (!_bindGroups.TryGetValue(groupId, out var bindGroup))
            {
                bindGroup = new BindGroupInfo { GroupId = groupId };
                _bindGroups[groupId] = bindGroup;
            }

            bindGroup.Entries.Add(new BindingEntryInfo
            {
                Binding = bindingId,
                Name = fieldName,
                TypeName = typeName,
                BindingType = bindingType,
                Access = access
            });
        }

        if (accessAttribute is not null)
        {
            _writer.Write("var<");
            var arg = accessAttribute.ArgumentList!.Arguments.First();
            if (arg.Expression is MemberAccessExpressionSyntax a)
            {
                _writer.Write(a.Name.Identifier.Text.ToLower());
            }
            else
            {
                var accesses = arg.Expression.ChildNodes().ToList();
                foreach (var syntax in accesses)
                {
                    switch (syntax)
                    {
                        case MemberAccessExpressionSyntax memberAccess:
                            _writer.Write(memberAccess.Name.Identifier.Text.ToLower());
                            break;
                        default:
                            throw new NotImplementedException("Only identifiers are allowed in Access attribute");
                    }

                    if (syntax != accesses.Last())
                        _writer.Write(", ");
                }
            }

            _writer.Write("> ");
        }
        else
        {
            _writer.Write("var ");
        }

        _writer.WriteLine(
            $"{node.Declaration.Variables.First().Identifier.Text}: {_typemapper.Map(node.Declaration.Type)};");
    }

    public override void VisitAttribute(AttributeSyntax node)
    {
        var name = node.Name.ToString();

        switch (name)
        {
            case "Builtin":
                var builtinName = node.ArgumentList?.Arguments.First().Expression.ToString().Trim('\"');
                _writer.Write($"@builtin({builtinName}) ");
                break;

            case "Location":
                var location = node.ArgumentList?.Arguments.First().Expression.ToString();
                _writer.Write($"@location({location}) ");

                // Store the location for later vertex attribute generation
                // (will be associated with the parameter or field in the parent method)
                break;

            case "VertexShader":
                _writer.Write("@vertex ");

                // Store the method name as vertex entry point
                var methodDecl = node.Parent?.Parent as MethodDeclarationSyntax;
                if (methodDecl != null)
                {
                    _entryPoints.VertexEntryPoint = methodDecl.Identifier.Text;
                }

                break;

            case "FragmentShader":
                _writer.Write("@fragment ");

                // Store the method name as fragment entry point
                var fragMethodDecl = node.Parent?.Parent as MethodDeclarationSyntax;
                if (fragMethodDecl != null)
                {
                    _entryPoints.FragmentEntryPoint = fragMethodDecl.Identifier.Text;
                }

                break;

            case "Group":
                var group = node.ArgumentList?.Arguments.First().Expression.ToString();
                _writer.Write($"@group({group}) ");
                break;

            case "Binding":
                var binding = node.ArgumentList?.Arguments.First().Expression.ToString();
                _writer.Write($"@binding({binding}) ");
                break;

            case "Access":
                // Special case for var<access flags>
                break;

            default:
                throw new NotImplementedException($"Attribute {name} is not implemented.");
        }
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        _writer.WriteLine();
        var attributes = node.AttributeLists.SelectMany(a => a.Attributes);
        foreach (var attribute in attributes)
        {
            Visit(attribute);
        }

        _writer.WriteLine();

        var returnType = _typemapper.Map(node.ReturnType);
        var name = node.Identifier.Text;
        _writer.Write($"fn {name}(");
        _writer.Indent += 2;
        foreach (var parameter in node.ParameterList.Parameters)
        {
            Visit(parameter);
            if (parameter != node.ParameterList.Parameters.Last())
                _writer.WriteLine(", ");
        }

        _writer.Indent -= 2;
        _writer.WriteLine($") -> {returnType}");
        Visit(node.Body);
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
        _writer.Write("let ");
        var variable = node.Declaration.Variables.First();
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
                _writer.WriteLine(access.Expression.ToString().Trim('\"'));
            }

            return;
        }

        if (node.Expression is not IdentifierNameSyntax identifier)
            throw new NotImplementedException("Expression should be an identifier");

        _writer.Write(_typemapper.MapMethod(identifier.Identifier.Text));
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
        // we drop casts
        Visit(node.Expression);
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

    #endregion
}
