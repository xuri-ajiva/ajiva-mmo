using System.CodeDom.Compiler;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace WgslBuiltInGenerator;

internal interface IFunctionTransformer
{
    IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument);
}

internal struct Parameter
{
    public string Name;
    public string Type;
    public bool IsParams;
}

public class Constraint
{
    public string Generic;
    public string ConstraintType;
}

internal record FunctionDef(
    Parameter[] Parameters,
    List<string> VarIgnore,
    List<Constraint> Constrains,
    string[] Generics,
    string Name,
    string ReturnType,
    BlockInfo Block,
    string OriginalSignature,
    string? Parameterization = null,
    string? Preconditions = null,
    string? Description = null
)
{
    public override string ToString()
    {
        return $"{ReturnType} {Name}({string.Join(", ", Parameters.Select(p => $"{p.Type} {p.Name}"))})";
    }
}

internal struct BlockInfo
{
    public ulong Start;
    public ulong Size;
    public string Content;
}

public static class BikeshedParser
{
    public static string ParseBikeshedDocumentToCSharp(string bsDocument)
    {
        var csharpCode = new StringBuilder(
            """
            using System;
            using System.Collections.Generic;
            using GlmSharp;

            namespace WgslGenerator;

            using AbstractInt = System.Int32;

            using u32 = System.UInt32;
            using i32 = System.Int32;
            using f32 = System.Single;
            using f16 = System.Single; // Wrong but who cares
            using vec2i = GlmSharp.ivec2;
            using vec3i = GlmSharp.ivec3;
            using vec4i = GlmSharp.ivec4;
            using vec2u = GlmSharp.uvec2;
            using vec3u = GlmSharp.uvec3;
            using vec4u = GlmSharp.uvec4;
            using vec2f = GlmSharp.vec2;
            using vec3f = GlmSharp.vec3;
            using vec4f = GlmSharp.vec4;
            using vec2b = GlmSharp.bvec2;
            using vec3b = GlmSharp.bvec3;
            using vec4b = GlmSharp.bvec4;
            // matrix types only have f32 support
            using mat2x2f = GlmSharp.mat2;
            using mat2x3f = GlmSharp.mat2x3; 
            using mat2x4f = GlmSharp.mat2x4;
            using mat3x2f = GlmSharp.mat3x2; 
            using mat3x3f = GlmSharp.mat3;
            using mat3x4f = GlmSharp.mat3x4; 
            using mat4x2f = GlmSharp.mat4x2; 
            using mat4x3f = GlmSharp.mat4x3; 
            using mat4x4f = GlmSharp.mat4; 

            public partial class BuiltIn
            {

                public partial class texture_1d<T>;
                public partial class texture_2d<T>;
                public partial class texture_2d_array<T>;
                public partial class texture_3d<T>;
                public partial class texture_cube<T>;
                public partial class texture_cube_array<T>;
                public partial class texture_multisampled_2d<T>;
                public partial class texture_storage_1d<T>;
                public partial class texture_storage_2d<T>;
                public partial class texture_storage_2d_array<T>;
                public partial class texture_storage_3d<T>;

                public partial class texture_depth_2d;
                public partial class texture_depth_2d_array;
                public partial class texture_depth_cube;
                public partial class texture_depth_cube_array;

                public partial class texture_depth_multisampled_2d;
                public partial class texture_external;

                public partial class sampler;
                public partial class sampler_comparison;

                public partial class ptr<T1>;
                public partial class ptr<T1,T2>;
                public partial class ptr<T1,T2,T3>;
                public partial class atomic<T1>;
                public partial class read_write;
                public partial class storage;
                public partial class workgroup;
            """);
        var writer = new IndentedTextWriter(new StringWriter(csharpCode), "    ");
        writer.Indent++;


        var functionNames = AppendFunctions(bsDocument, writer);

        ApendVallidFunctionNames(writer, functionNames);

        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine();

        writer.Flush();
        return csharpCode.ToString();
    }

    private static HashSet<string> AppendFunctions(string bsDocument, IndentedTextWriter writer)
    {
        var functionNames = new HashSet<string>();
        // 1. find all <xmp highlight=wgsl> blocks
        var blocks = FindBlocks(bsDocument);
        // 2. extract function signature
        var functions = ExtractFunctionDefs(blocks, bsDocument);
        // 3. backtrack for generic types like A, ST, C, etc.

        IEnumerable<FunctionDef> current = functions;

        ReadOnlySpan<IFunctionTransformer> transformers =
        [
            new TextureHandler(),
            new PrtHandler(),
            new ParameterizationHandler(),
            new ParameterizationHandler(),
            new MatrixHandler('C'),
            new MatrixHandler('R'),
            new VectorNHandler(),
            new GenericStripHandler("mat", "T", "f32"),
            new GenericStripHandler("mat", "S", "f32"),
            new TypeHandler("ST"),
            new TypeHandler("CF"),
            new TypeHandler("T1"),
            new TypeHandler("T2"),
            new TypeHandler("S1"),
            new TypeHandler("S2"),
            new TypeHandler("T"),
            new TypeHandler("S"),
            new TypeHandler("C"),
            new TypeMapper(),
            new StripGenerics(),
            new TypeAnonymizer(),
            new ArrayReduce(),
            new AddGenerics(2),
        ];

        foreach (var transformer in transformers)
        {
            var next = transformer.TransformFunction(current, bsDocument).ToList();
            var names = next.Select(x => x.Name).Distinct().Count();
            var textureSample = next.FirstOrDefault(x => x.Name == "textureSample");
            Console.WriteLine($"{transformer.GetType()} produced {next.Count} functions with {names} unique names");
            Console.WriteLine($"textureSample: {textureSample?.OriginalSignature}");
            current = next;
        }

        // 4. write function signature to csharp code
        HashSet<string> seenFunctionNames = [];

        HashSet<string> skipFunctions =
        [
            "bitcast", "frexp", "modf"
        ];

        HashSet<string> defCache = [];
        foreach (var function in current.Skip(19))
        {
            seenFunctionNames.Add(function.Name);

            if (skipFunctions.Contains(function.Name)) continue;

            if (function.ReturnType.StartsWith('_'))
            {
                writer.WriteLine($"public partial struct {function.ReturnType};");
            }

            var functionSignature =
                $"public static {TypeMapper(function.ReturnType)} {(function.Name == "bool" ? "@bool" : function.Name)}{(function.Generics.Length > 0 ? $"<{string.Join(", ", function.Generics)}>" : "")}" +
                $"({string.Join(", ", function.Parameters.Select(p => $"{TypeMapper(p.Type)} {p.Name}"))}) => default!;";


            //writer.WriteLine(xmlComment);
            if (defCache.Contains(functionSignature)) continue;

            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// {Format(function.Description)}");
            writer.WriteLine("/// </summary>");
            writer.WriteLine($"/// <OriginalSignature>{Format(function.OriginalSignature)}</OriginalSignature>");
            if (!string.IsNullOrEmpty(function.Parameterization))
            {
                writer.WriteLine($"/// <Parameterization>{Format(function.Parameterization)}</Parameterization>");
            }

            if (!string.IsNullOrEmpty(function.Preconditions))
            {
                writer.WriteLine($"/// <Preconditions>{Format(function.Preconditions)}</Preconditions>");
            }

            writer.WriteLine(functionSignature);
            defCache.Add(functionSignature);
        }


        return functionNames;
    }

    private static string? Format(string? value)
    {
        if (value is null) return null;
        var rp = value.Replace("&lt;", "<").Replace("&gt;", ">");
        var res = rp.StartsWith("<td>") ? rp["<td>".Length..] : rp;
        return $"<![CDATA[{res}]]>";
    }

    private static List<BlockInfo> FindBlocks(string bsDocument)
    {
        const string xmpHighlightWgsl = "<xmp highlight=wgsl>";
        const string readOnlySpan = "</xmp>";

        var blocks = new List<BlockInfo>();
        var rSlice = bsDocument.AsSpan();
        var offset = 0;
        while (FindNext(rSlice[offset..], xmpHighlightWgsl) is { } start)
        {
            offset += start;
            if (FindNext(rSlice[offset..], readOnlySpan) is { } end)
            {
                var block = new BlockInfo
                {
                    Start = (ulong)offset + (ulong)xmpHighlightWgsl.Length,
                    Size = (ulong)(end - xmpHighlightWgsl.Length),
                };
                block.Content = bsDocument.Substring((int)block.Start, (int)block.Size);
                blocks.Add(block);
                offset += end + readOnlySpan.Length;
            }
        }


        return blocks;
    }

    private static List<FunctionDef> ExtractFunctionDefs(List<BlockInfo> blocks, string bsDocument)
    {
        Span<Range> dest = stackalloc Range[1000];
        ReadOnlySpan<string> paramEnd = ["</table>", "<tr>", "</thead>"];

        var functions = new List<FunctionDef>();
        foreach (var block in blocks)
        {
            var content = block.Content;

            /*
             @must_use fn textureGather(t: texture_depth_cube_array,
                                   s: sampler,
                                   coords: vec3<f32>,
                                   array_index: A) -> vec4<f32>
             */

            // 1. parse fn ... -> (return type)
            var matches = Regex.Matches(content, @"fn\s+(\w+(?:<[^>]*>)?)\(([^)]*)\)\s*->\s*(\w+(?:<[^>]*>)?)");
            foreach (Match match in matches)
            {
                Parameter[] paramArray = [];
                // 2. parse parameters
                var parameters = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(parameters))
                {
                    if (parameters.Contains("..."))
                    {
                        var param = ParseParam(parameters[..parameters.IndexOf(',')]);
                        param.IsParams = true;
                        if (char.IsDigit(param.Name[^1]))
                        {
                            param.Name = param.Name[..^1];
                        }

                        paramArray = [param];
                    }
                    else
                    {
                        // Split parameters correctly handling nested angle brackets
                        var parametersList = SplitParametersHandlingNestedTypes(parameters);
                        paramArray = parametersList.Select(ParseParam).ToArray<Parameter>();
                    }
                }

                var span = bsDocument.AsSpan((int)(block.Start + block.Size));
                if (span.Length > 10000)
                    span = span[..10000];

                string? Preconditions = null, Parameterization = null, Description = null;
                List<string> VarIgnore = [];
                if (TryFindBetween(span, ">Preconditions", out var preconditions, FindNext, paramEnd))
                {
                    Preconditions = CleanStr(span[preconditions]);
                    span = span[preconditions.Start..];
                }

                if (TryFindBetween(span, ">Parameterization", out var parameterization, FindNext, paramEnd))
                {
                    Parameterization = CleanStr(span[parameterization]);
                    span = span[parameterization.Start..];
                }

                if (TryFindBetween(span, ">Description", out var description, FindNext, paramEnd))
                {
                    Description = CleanStr(span[description]);
                }

                span = bsDocument.AsSpan((int)(block.Start - 10000)..(int)(block.Start));
                if (FindLast(span, "<tr algorithm") is { } start)
                {
                    span = span[start..];
                    if (TryFindBetween(span, "<td>", out var fCosn, FindNext, "<td>"))
                    {
                        span = span[fCosn];
                        var amount = span.SplitAny(dest, ["<var ignore>", "<br>"],
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var range in dest[..amount])
                        {
                            VarIgnore.Add(span[range].StartsWith("<var ignore>")
                                ? CleanStr(span[range]["<var ignore>".Length..])
                                : CleanStr(span[range]));
                        }
                    }
                }

                functions.Add(new FunctionDef(
                    paramArray, VarIgnore, [], [], match.Groups[1].Value, match.Groups[3].Value, block,
                    CleanStr(match.Value), Parameterization, Preconditions, Description));
            }
        }


        return functions;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string CleanStr(ReadOnlySpan<char> span)
    {
        // removes newlines and padding

        /*span.ReplaceLineEndings(" ").Replace('\t', ' '));
        while (span.Contains("  "))
        {
            span = span.Replace("  ", " ");
        }*/

        Span<char> chars = stackalloc char[span.Length];
        var i = 0;
        foreach (var c in span)
        {
            if (char.IsWhiteSpace(c) || char.IsControl(c))
            {
                if (i == 0 || char.IsWhiteSpace(chars[i - 1]))
                    continue;
                chars[i++] = ' ';
            }
            else
            {
                chars[i++] = c;
            }
        }

        return new string(chars[..i]);
    }

    private static int? FindNext(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
    {
        var index = span.IndexOf(value);
        return index == -1 ? null : index;
    }

    private static int? FindLast(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
    {
        var index = span.LastIndexOf(value);
        return index == -1 ? null : index;
    }

    private static bool TryFindBetween(ReadOnlySpan<char> span, ReadOnlySpan<char> start, out Range match,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, int?> findFunc,
        params ReadOnlySpan<string> end)
    {
        match = default;
        if (findFunc(span, start) is not { } startIndex) return false;

        var best = -1;
        foreach (var endValue in end)
        {
            if (findFunc(span[(startIndex + 1)..], endValue) is not { } endIndex) continue;
            if (best == -1 || endIndex < best)
            {
                best = endIndex;
            }
        }

        match = (startIndex + start.Length)..(startIndex + 1 + best);
        return best != -1;
    }


    private static Parameter ParseParam(string parameterName)
    {
        var parts = parameterName.Split([':'], 2);
        if (parts.Length < 2)
            return new Parameter { Name = parts[0].Trim(), Type = "unknown" };
        return new Parameter
        {
            Name = parts[0].Trim(),
            Type = parts[1].Trim()
        };
    }

    private static List<string> SplitParametersHandlingNestedTypes(string parameters)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(parameters))
            return result;

        int depth = 0;
        int startIndex = 0;

        for (int i = 0; i < parameters.Length; i++)
        {
            char c = parameters[i];
            if (c == '<') depth++;
            else if (c == '>') depth--;
            else if (c == ',' && depth == 0)
            {
                result.Add(parameters.Substring(startIndex, i - startIndex).Trim());
                startIndex = i + 1;
            }
        }

        // Add the last parameter
        result.Add(parameters.Substring(startIndex).Trim());
        return result;
    }

    public static string TypeMapper(string typeIn)
    {
        if (typeIn.StartsWith("vec"))
        {
            var generic = typeIn[4..].TrimStart('<').TrimEnd('>');
            var dim = uint.Parse(typeIn[3..4]);
            return generic switch
            {
                "f32" => $"vec{dim}f",
                "i32" => $"vec{dim}i",
                "u32" => $"vec{dim}u",
                "f16" => $"vec{dim}f", // wrong but who cares
                "bool" => $"vec{dim}b",
                _ => typeIn
            };
        }

        if (typeIn.StartsWith("mat"))
        {
            var dimX = uint.Parse(typeIn[3..4]);
            var dimY = uint.Parse(typeIn[5..6]);
            return $"mat{dimX}x{dimY}f";
        }

        return typeIn;
    }

    private static void ResolveConstrains(string bsDocument, List<FunctionDef> functions)
    {
        HashSet<string> uniqueConstraints = new HashSet<string>();
        foreach (var function in functions)
        {
            foreach (var se in function.VarIgnore)
            {
                // todo
            }

            if (!string.IsNullOrEmpty(function.Parameterization))
            {
                uniqueConstraints.Add(function.Parameterization);
                //<td>`T` is a [=type/concrete=] [=constructible=] type.<br> 
                var matches = Regex.Matches(function.Parameterization, @"`(\w+)`([^<]+)<br>");
                foreach (Match match in matches)
                {
                    uniqueConstraints.Add(match.Value);
                }
            }
        }
    }

    private static IEnumerable<string> ResolveGenericType(string bsDocument, FunctionDef function, string generic)
    {
        var genericType = generic.Trim();
        var genericTypeRegex = new Regex($@"<var ignore>{genericType}</var>\s+is\s+([^<]+)<br>");
        var matches = genericTypeRegex.Matches(bsDocument);

        foreach (Match match in matches)
        {
            yield return match.Groups[1].Value.Trim();
        }
    }

    private static void ApendVallidFunctionNames(IndentedTextWriter csharpCode, HashSet<string> functionNames)
    {
        csharpCode.WriteLine();
        csharpCode.WriteLine("// Valid Function Names:");
        csharpCode.WriteLine();

        foreach (var funcName in functionNames)
        {
            csharpCode.WriteLine("// " + funcName);
        }
    }
}

// for prt<AS,...> replace with prt<AX,...>
internal class PrtHandler : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            if (functionDef.Parameters.Any(x => x.Type.Contains("ptr<AS")))
            {
                yield return functionDef with
                {
                    Parameters = functionDef.Parameters.Select(p =>
                        p.Type.Contains("ptr<AS") ? p with { Type = p.Type.Replace("ptr<AS", "ptr<AX") } : p).ToArray(),
                    Generics = ["AX"]
                };
            }
            else
            {
                yield return functionDef;
            }
        }
    }
}

// reduces array<T (,N?)> to T[]
internal class ArrayReduce : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            yield return functionDef with
            {
                ReturnType = Reduce(functionDef.ReturnType),
                Parameters = functionDef.Parameters.Select(p => p with { Type = Reduce(p.Type) }).ToArray()
            };
        }
    }

    private static string Reduce(string value)
    {
        if (!value.StartsWith("array")) return value;
        var index = value.IndexOf('<');
        if (index == -1) return value;
        var generic = value[index..^1];
        return $"{generic[..^1]}[]";
    }
}

// still left vec... or mat... types get generict into V[{i}]
internal class TypeAnonymizer : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        Dictionary<string, string> mappings = [];
        foreach (var functionDef in functions)
        {
            if (functionDef.ReturnType.Contains('<') &&
                (functionDef.ReturnType.StartsWith("vec") || functionDef.ReturnType.StartsWith("mat")))
            {
                if (!mappings.TryGetValue(functionDef.ReturnType, out var generic))
                {
                    generic = $"V{mappings.Count + 1}";
                    mappings[functionDef.ReturnType] = generic;
                }
            }

            foreach (var paramDef in functionDef.Parameters)
            {
                if (paramDef.Type.Contains('<') && (paramDef.Type.StartsWith("vec") || paramDef.Type.StartsWith("mat")))
                {
                    if (!mappings.TryGetValue(paramDef.Type, out var generic3))
                    {
                        generic3 = $"V{mappings.Count + 1}";
                        mappings[paramDef.Type] = generic3;
                    }
                }
            }

            yield return functionDef with
            {
                ReturnType = mappings.TryGetValue(functionDef.ReturnType, out var generic4)
                    ? generic4
                    : functionDef.ReturnType,
                Parameters = functionDef.Parameters.Select(p =>
                    p with { Type = mappings.TryGetValue(p.Type, out var generic5) ? generic5 : p.Type }).ToArray()
            };
        }
    }
}

// tryes to resolve T from Parameterization
internal class ParameterizationHandler : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var function in functions)
        {
            if (function.Parameterization is null)
            {
                yield return function;
                continue;
            }

            // check if func has S and T as generics
            var hasT = function.ReturnType == "T"
                       || function.ReturnType.Contains($"<T>")
                       || function.Parameters.Any(x => x.Type == "T" || x.Type.Contains($"<T>"));

            if (hasT)
            {
                var i32Target = MakeTyped(function, "S", "i32");
                var u32Target = MakeTyped(function, "S", "u32");
                var f32Target = MakeTyped(function, "S", "f32");

                var allintegraldecl = function.Parameterization.Contains("ALLINTEGRALDECL");
                var allfloatingdecl = function.Parameterization.Contains("ALLFLOATINGDECL");
                var allnumericdecl = function.Parameterization.Contains("ALLNUMERICDECL");
                var allsignednumericdecl = function.Parameterization.Contains("ALLSIGNEDNUMERICDECL");
                if (allintegraldecl || allsignednumericdecl || allnumericdecl)
                {
                    yield return MakeTyped(i32Target, "T", "i32");
                    yield return MakeTyped(i32Target, "T", "vecN<i32>");
                }

                if (allsignednumericdecl || allnumericdecl || allfloatingdecl)
                {
                    yield return MakeTyped(f32Target, "T", "f32");
                    yield return MakeTyped(f32Target, "T", "vecN<f32>");
                }

                if (allintegraldecl || allfloatingdecl || allnumericdecl)
                {
                    yield return MakeTyped(u32Target, "T", "u32");
                    yield return MakeTyped(u32Target, "T", "vecN<u32>");
                }
            }

            // try detecting vecN
            // <td>`T` is vecN&lt;AbstractFloat&gt;, vecN&lt;f32&gt;, or vecN&lt;f16&gt;
            var matches = Regex.Matches(function.Parameterization, @"`(\w+)` is ([^\n<]*)(?:<br>)?");
            if (matches.Count == 0)
            {
                yield return function;
                continue;
            }

            foreach (Match match in matches)
            {
                var generic = match.Groups[1].Value;
                var values = match.Groups[2].Value;

                foreach (var type in values.Split("or").SelectMany(x => x.Split(", ")))
                {
                    if (type.Contains('['))
                    {
                        // extract type from [type]
                        var tmatches = Regex.Matches(type, @"\[([^\]]+)\]");
                        foreach (Match tmatch in tmatches)
                        {
                            var valueType = tmatch.Groups[1].ValueSpan;

                            switch (valueType)
                            {
                                case "=scalar=":
                                case "=numeric scalar=":
                                case "=runtime-sized=":
                                    yield return function;
                                    break;
                                case "FLOATSCALAR":
                                case "=f16=":
                                case "=f32=":
                                    yield return MakeTyped(function, generic, "f32");
                                    break;
                                case "FLOATING":
                                    yield return MakeTyped(function, generic, "f32");
                                    yield return MakeTyped(function, generic, "vecN<f32>");
                                    break;
                                case "INTEGRAL":
                                    yield return MakeTyped(function, generic, "i32");
                                    yield return MakeTyped(function, generic, "u32");
                                    yield return MakeTyped(function, generic, "vecN<i32>");
                                    yield return MakeTyped(function, generic, "vecN<u32>");
                                    break;
                                case "SIGNEDINTEGRAL":
                                    yield return MakeTyped(function, generic, "i32");
                                    yield return MakeTyped(function, generic, "vecN<i32>");
                                    break;
                                case "UNSIGNEDINTEGRAL":
                                    yield return MakeTyped(function, generic, "u32");
                                    yield return MakeTyped(function, generic, "vecN<u32>");
                                    break;
                                case "ALLSIGNEDINTEGRAL":
                                    yield return MakeTyped(function, generic, "i32");
                                    yield return MakeTyped(function, generic, "vecN<i32>");
                                    break;
                                default:
                                    yield return function;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var target = type.Trim();
                        switch (target)
                        {
                            case "AbstractInt":
                                yield return MakeTyped(function, generic, "i32");
                                yield return MakeTyped(function, generic, "u32");
                                break;
                            case "AbstractFloat":
                                yield return MakeTyped(function, generic, "f32");
                                //yield return MakeTyped(function, generic, "f16");
                                break;
                            case "i32" or "u32" or "f32":
                                yield return MakeTyped(function, generic, target);
                                break;
                            case "f16":
                                yield return MakeTyped(function, generic, "f32");
                                break;
                            case "a vect":
                                yield return MakeTyped(function, generic, "vecN<T>");
                                break;
                            default:
                                // extract vecN&lt;T&gt;
                                var vecNMatch = Regex.Match(target, "vecN&lt;([^&]+)&gt;");
                                if (vecNMatch.Success && vecNMatch.Groups is { Count: > 0 } &&
                                    vecNMatch.Groups[1] is { Value: var imple })
                                {
                                    if (imple is "AbstractFloat" or "f16")
                                    {
                                        yield return MakeTyped(function, generic, $"vecN<f32>");
                                    }
                                    else
                                    {
                                        yield return MakeTyped(function, generic, $"vecN<{imple}>");
                                    }
                                }
                                else
                                {
                                    yield return function;
                                }

                                break;
                        }
                    }
                }
            }
        }
    }

    private FunctionDef MakeTyped(FunctionDef function, string generic, string target)
    {
        return function with
        {
            ReturnType = Transform(generic, target, function.ReturnType),
            Parameters = function.Parameters.Select(p => p with { Type = Transform(generic, target, p.Type) }).ToArray()
        };
    }

    private static string Transform(string generic, string target, string p)
    {
        if (p == generic)
            return p = target;
        if (p.Contains($"<{generic}>"))
            return p.Replace($"<{generic}>", $"<{target}>");
        return p;
    }
}

internal class TypeMapper : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            yield return functionDef with
            {
                Name = BikeshedParser.TypeMapper(functionDef.Name),
                ReturnType = BikeshedParser.TypeMapper(functionDef.ReturnType),
                Parameters = functionDef.Parameters.Select(p => p with { Type = BikeshedParser.TypeMapper(p.Type) })
                    .ToArray()
            };
        }
    }
}

// tryes to resolve T from Variings
internal class TextureHandler : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            var function = functionDef;
            if (function.Parameters.Any(x => x.Type.Contains("<F, AM>")))
            {
                for (var index = 0; index < function.Parameters.Length; index++)
                {
                    ref var parameter = ref function.Parameters[index];
                    parameter.Type = parameter.Type.Replace("<F, AM>", "<C>");
                }

                function = function with { ReturnType = function.ReturnType.Replace("<CF>", "<C>") };
            }


            if (function.Parameters.Any(x => x.Type == "T") &&
                function.VarIgnore.FirstOrDefault(x => x.Contains("|T|")) is { } input)
            {
                var matches = Regex.Matches(input, @"`(\w+)`");
                foreach (Match match in matches)
                {
                    var valueType = match.Groups[1].Value;
                    yield return function with
                    {
                        ReturnType = function.ReturnType == "T" ? valueType : function.ReturnType,
                        Parameters = function.Parameters
                            .Select(p => p.Type == "T" ? p with { Type = valueType } : p).ToArray()
                    };
                }
            }
            else
            {
                yield return function;
            }
        }
    }
}

// Adds generics to functions, for each type with <= maxLenType chars
internal class AddGenerics(int maxLenType) : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            var types = new HashSet<string>(functionDef.Generics);
            foreach (var parameter in functionDef.Parameters)
            {
                if (parameter.Type.Length <= maxLenType)
                {
                    types.Add(parameter.Type);
                }

                var index = parameter.Type.IndexOf('<');
                if (index != -1)
                {
                    var genericPart = parameter.Type.AsSpan(index..^1);
                    foreach (var range in genericPart.Split(','))
                    {
                        var readOnlySpan = genericPart[range];
                        if (readOnlySpan.StartsWith(' '))
                        {
                            if (readOnlySpan.Length - 1 <= maxLenType)
                                types.Add(readOnlySpan[1..].ToString());
                        }
                        else
                        {
                            if (readOnlySpan.Length <= maxLenType)
                                types.Add(readOnlySpan.ToString());
                        }
                    }
                }
            }

            if (functionDef.ReturnType.Length <= maxLenType)
            {
                types.Add(functionDef.ReturnType);
            }


            yield return functionDef with
            {
                Generics = types.ToArray()
            };
        }
    }
}

// strips <> from func names
internal class StripGenerics : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            if (functionDef.Name.Contains('<'))
            {
                var name = functionDef.Name[..functionDef.Name.IndexOf('<')];
                yield return functionDef with { Name = name };
            }
            else
            {
                yield return functionDef;
            }
        }
    }
}

// Replaces {type} with i32, u32, and f32
internal class TypeHandler(string target) : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            var isApplicable = IsApplicable(functionDef.ReturnType) ||
                               functionDef.Parameters.Any(parameter => IsApplicable(parameter.Type));

            if (!isApplicable)
            {
                yield return functionDef;
                continue;
            }

            foreach (var type in new[] { "i32", "u32", "f32" })
            {
                yield return functionDef with
                {
                    Name = Apply(functionDef.Name, type),
                    ReturnType = Apply(functionDef.ReturnType, type),
                    Parameters = functionDef.Parameters.Select(p => p with { Type = Apply(p.Type, type) }).ToArray(),
                };
            }
        }
    }

    private string Apply(string value, string type)
    {
        return value.Replace(target, type);
    }

    private bool IsApplicable(string value)
    {
        return value.Contains(target);
    }
}

// strips generic from types
internal class GenericStripHandler(string prefix, string generic, string replace) : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            var isApplicable = IsApplicable(functionDef.ReturnType) ||
                               functionDef.Parameters.Any(parameter => IsApplicable(parameter.Type));

            if (!isApplicable)
            {
                yield return functionDef;
                continue;
            }

            yield return functionDef with
            {
                ReturnType = Apply(functionDef.ReturnType),
                Parameters = functionDef.Parameters.Select(p => p with { Type = Apply(p.Type) }).ToArray(),
            };
        }
    }

    private static string? ExtractGenerics(string value)
    {
        var indx = value.IndexOf('<');
        return indx == -1 ? null : value[indx..];
    }


    private string Apply(string value)
    {
        if (value == generic)
        {
            return replace;
        }

        if (value.StartsWith(prefix))
        {
            return value.Contains(generic)
                ? value[..value.IndexOf('<')]
                : value.Replace(generic, "");
        }

        return value;
    }

    private bool IsApplicable(string value)
    {
        return value.StartsWith(prefix) && value.Contains($"<{generic}>");
    }
}

// Replaces vecN... with vec2... vec3... vec4...
internal class VectorNHandler : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            var isApplicable = IsApplicable(functionDef.ReturnType) ||
                               functionDef.Parameters.Any(parameter => IsApplicable(parameter.Type));

            if (!isApplicable)
            {
                yield return functionDef;
                continue;
            }

            for (var i = 2; i < 5; i++)
            {
                yield return functionDef with
                {
                    ReturnType = Apply(functionDef.ReturnType, i),
                    Parameters = functionDef.Parameters.Select(p => p with { Type = Apply(p.Type, i) }).ToArray(),
                };
            }
        }
    }

    private static string Apply(string functionDefReturnType, int n)
    {
        return functionDefReturnType.Replace('N', (char)('0' + n));
    }

    private static bool IsApplicable(string value)
    {
        return value.StartsWith("vecN");
    }
}

// Replace mat{A}x{B} with mat{i}x{j} where i = 2..4 and j = 2..4 iff A or B == searchValue
internal class MatrixHandler(char searchValue) : IFunctionTransformer
{
    public IEnumerable<FunctionDef> TransformFunction(IEnumerable<FunctionDef> functions, string bsDocument)
    {
        foreach (var functionDef in functions)
        {
            var isApplicable = IsApplicable(functionDef.ReturnType) ||
                               functionDef.Parameters.Any(parameter => IsApplicable(parameter.Type));

            if (!isApplicable)
            {
                yield return functionDef;
                continue;
            }

            for (int i = 2; i < 5; i++)
            {
                yield return functionDef with
                {
                    ReturnType = Apply(functionDef.ReturnType, i),
                    Parameters = functionDef.Parameters.Select(p => p with { Type = Apply(p.Type, i) }).ToArray(),
                };
            }
        }
    }

    private string Apply(string value, int number)
    {
        return value.Replace(searchValue, (char)('0' + number));
    }

    private bool IsApplicable(string type)
    {
        return type.StartsWith("mat") && type.Contains(searchValue);
    }
}
