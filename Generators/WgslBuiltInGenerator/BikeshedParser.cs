using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;

namespace WgslBuiltInGenerator;

public static class BikeshedParser
{
    public static string ParseBikeshedDocumentToCSharp(string bsDocument)
    {
        var csharpCode = new StringBuilder(@"
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
    public partial class array<T1,N>;
    public partial class array<T1>;
");
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
        /*### `normalize` ### {#normalize-builtin}
<table class='data builtin'>
  <tr algorithm="vector case, normalize">
    <td style="width:10%">Overload
    <td class="nowrap">
      <xmp highlight=wgsl>
        @const @must_use fn normalize(e: vecN<T> ) -> vecN<T>
      </xmp>
  <tr>
    <td style="width:10%">Parameterization
    <td>`T` is AbstractFloat, f32, or f16
  <tr>
    <td>Description
    <td>Returns a unit vector in the same direction as `e`.

    The [=domain=] is all vectors except the zero vector.
</table>*/
        /*
<table class='data'>
  <thead>
    <tr><td style="width:25%">Parameterization<th>Overload
  </thead>
  <tr algorithm="textureGather 2d">
    <td><var ignore>C</var> is [=i32=], or [=u32=]<br>
        <var ignore>ST</var> is [=i32=], [=u32=], or [=f32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(component: C,
                                   t: texture_2d<ST>,
                                   s: sampler,
                                   coords: vec2<f32>) -> vec4<ST>
      </xmp>

  <tr algorithm="textureGather 2d offset">
    <td><var ignore>C</var> is [=i32=], or [=u32=]<br>
        <var ignore>ST</var> is [=i32=], [=u32=], or [=f32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(component: C,
                                   t: texture_2d<ST>,
                                   s: sampler,
                                   coords: vec2<f32>,
                                   offset: vec2<i32>) -> vec4<ST>
      </xmp>

  <tr algorithm="textureGather 2d array">
    <td><var ignore>C</var> is [=i32=], or [=u32=]<br>
        <var ignore>A</var> is [=i32=], or [=u32=]<br>
        <var ignore>ST</var> is [=i32=], [=u32=], or [=f32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(component: C,
                                   t: texture_2d_array<ST>,
                                   s: sampler,
                                   coords: vec2<f32>,
                                   array_index: A) -> vec4<ST>
      </xmp>

  <tr algorithm="textureGather 2d array offset">
    <td><var ignore>C</var> is [=i32=], or [=u32=]<br>
        <var ignore>A</var> is [=i32=], or [=u32=]<br>
        <var ignore>ST</var> is [=i32=], [=u32=], or [=f32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(component: C,
                                   t: texture_2d_array<ST>,
                                   s: sampler,
                                   coords: vec2<f32>,
                                   array_index: A,
                                   offset: vec2<i32>) -> vec4<ST>
      </xmp>

  <tr algorithm="textureGather cube">
    <td><var ignore>C</var> is [=i32=], or [=u32=]<br>
        <var ignore>ST</var> is [=i32=], [=u32=], or [=f32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(component: C,
                                   t: texture_cube<ST>,
                                   s: sampler,
                                   coords: vec3<f32>) -> vec4<ST>
      </xmp>

  <tr algorithm="textureGather cube array">
    <td><var ignore>C</var> is [=i32=], or [=u32=]<br>
        <var ignore>A</var> is [=i32=], or [=u32=]<br>
        <var ignore>ST</var> is [=i32=], [=u32=], or [=f32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(component: C,
                                   t: texture_cube_array<ST>,
                                   s: sampler,
                                   coords: vec3<f32>,
                                   array_index: A) -> vec4<ST>
      </xmp>

  <tr algorithm="textureGather 2d depth">
    <td>
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(t: texture_depth_2d,
                                   s: sampler,
                                   coords: vec2<f32>) -> vec4<f32>
      </xmp>

  <tr algorithm="textureGather 2d depth offset">
    <td>
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(t: texture_depth_2d,
                                   s: sampler,
                                   coords: vec2<f32>,
                                   offset: vec2<i32>) -> vec4<f32>
      </xmp>

  <tr algorithm="textureGather cube depth">
    <td>
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(t: texture_depth_cube,
                                   s: sampler,
                                   coords: vec3<f32>) -> vec4<f32>
      </xmp>

  <tr algorithm="textureGather 2d depth array">
    <td><var ignore>A</var> is [=i32=], or [=u32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(t: texture_depth_2d_array,
                                   s: sampler,
                                   coords: vec2<f32>,
                                   array_index: A) -> vec4<f32>
      </xmp>

  <tr algorithm="textureGather 2d depth array offset">
    <td><var ignore>A</var> is [=i32=], or [=u32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(t: texture_depth_2d_array,
                                   s: sampler,
                                   coords: vec2<f32>,
                                   array_index: A,
                                   offset: vec2<i32>) -> vec4<f32>
      </xmp>

  <tr algorithm="textureGather cube depth array">
    <td><var ignore>A</var> is [=i32=], or [=u32=]
    <td>
      <xmp highlight=wgsl>
        @must_use fn textureGather(t: texture_depth_cube_array,
                                   s: sampler,
                                   coords: vec3<f32>,
                                   array_index: A) -> vec4<f32>
      </xmp>
</table>
         */


        var functionNames = new HashSet<string>();
        // 1. find all <xmp highlight=wgsl> blocks
        var blocks = FindBlocks(bsDocument);
        // 2. extract function signature
        var functions = ExtractFunctionNames(blocks);
        // 3. backtrack for generic types like A, ST, C, etc.
        BacktrackTypes(bsDocument, functions);
        // 4. write function signature to csharp code
        HashSet<string> seenFunctionNames = new HashSet<string>();
        foreach (var func in functions)
        {
            foreach (var se in ConvertWgslFunctionSignatureToCSharp(func))
            {
                if (seenFunctionNames.Contains(se))
                    writer.Write("// ");
                writer.WriteLine(se);
                seenFunctionNames.Add(se);
            }

            functionNames.Add(func.Name);
        }

        return functionNames;
    }


    struct Parameter
    {
        public string Name;
        public string Type;
        public bool IsParams;
    }

    class FunctionDef
    {
        public string Name;
        public Parameter[] parameters = [];
        public string returnType;
        public BlockInfo Block;
        public string OriginalSignature;

        public override string ToString()
        {
            return $"{returnType} {Name}({string.Join(", ", parameters.Select(p => $"{p.Type} {p.Name}"))})";
        }
    }

    struct BlockInfo
    {
        public ulong Start;
        public ulong Size;
        public string Content;
    }

    private static List<BlockInfo> FindBlocks(string bsDocument)
    {
        const string xmpHighlightWgsl = "<xmp highlight=wgsl>";
        const string readOnlySpan = "</xmp>";

        var blocks = new List<BlockInfo>();
        var slice = bsDocument.AsSpan();
        while (slice.IndexOf(xmpHighlightWgsl) is var start && start != -1)
        {
            slice = slice[(start + xmpHighlightWgsl.Length)..];
            var end = slice.IndexOf(readOnlySpan);
            if (end == -1)
                break;
            var block = new BlockInfo
            {
                Start = (ulong)start,
                Size = (ulong)end,
                Content = slice[..end].ToString()
            };
            blocks.Add(block);
            slice = slice[(end + readOnlySpan.Length)..];
        }

        return blocks;
    }

    private static List<FunctionDef> ExtractFunctionNames(List<BlockInfo> blocks)
    {
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
                var func = new FunctionDef
                {
                    Name = match.Groups[1].Value,
                    returnType = match.Groups[3].Value,
                    Block = block
                };
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

                        func.parameters = [param];
                    }
                    else
                    {
                        // Split parameters correctly handling nested angle brackets
                        var parametersList = SplitParametersHandlingNestedTypes(parameters);
                        func.parameters = parametersList.Select(ParseParam).ToArray<Parameter>();
                    }
                }

                func.OriginalSignature = match.Value.ReplaceLineEndings(" ").Replace('\t', ' ');
                while (func.OriginalSignature.Contains("  "))
                {
                    func.OriginalSignature = func.OriginalSignature.Replace("  ", " ");
                }

                functions.Add(func);
            }
        }


        return functions;
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

    private static IEnumerable<string> ConvertWgslFunctionSignatureToCSharp(FunctionDef func)
    {
        yield return "// " + func.OriginalSignature;
        var result = "";
        if (func.Name == "textureDimensions")
        {
        }

        bool hasST = false;
        bool hasCR = false;
        bool hasN = false;
        HashSet<string> generics = [];
        Dictionary<string, string> genericMap = new();
        for (var index = 0; index < func.parameters.Length; index++)
        {
            ref var funcParameter = ref func.parameters[index];
            funcParameter.Type = funcParameter.Type.Replace("<F, AM>", "<ST>");

            if (funcParameter.Type.Contains("<ST>"))
            {
                hasST = true;
            }

            if (funcParameter.Type.Contains("<CF>"))
            {
                funcParameter.Type = funcParameter.Type.Replace("<CF>", "<ST>");
            }

            if (genericMap.TryGetValue(funcParameter.Type, out var mapped))
            {
                funcParameter.Type = mapped;
                continue;
            }

            if (funcParameter.Type is "L" or "A")
            {
                funcParameter.Type = "u32";
                funcParameter.Name += " = default!";
            }

            if (funcParameter.Type.Length < 3)
            {
                generics.Add(funcParameter.Type);
                continue;
            }

            if (funcParameter.Type.StartsWith("vec") || funcParameter.Type.StartsWith("mat"))
            {
                if (funcParameter.Type == func.returnType || funcParameter.Type == "matRxC<T>") continue;
                var maped = "VM" + generics.Count;
                genericMap.Add(funcParameter.Type, maped);
                funcParameter.Type = maped;
                generics.Add(maped);
                continue;
            }

            if (funcParameter.Type.StartsWith("ptr"))
            {
                // we replace all AS, AM, E, with dynamic
                funcParameter.Type = funcParameter.Type.Replace("AS", "dynamic").Replace("AM", "dynamic")
                    .Replace("E", "dynamic");
            }
        }

        if (func.Name.Contains('<'))
        {
            var gn2 = func.Name[(func.Name.IndexOf('<') + 1)..];
            foreach (var se in gn2.Split(','))
            {
                generics.Add(se.Trim().TrimEnd('>'));
            }

            func.Name = func.Name[..func.Name.IndexOf('<')];
        }

        if (func.returnType.Contains('<'))
        {
            var gn2 = func.returnType[(func.returnType.IndexOf('<') + 1)..];
            foreach (var se in gn2.Split(','))
            {
                generics.Add(se.Trim().TrimEnd('>'));
            }
        }

        if (func.returnType == "matCxR<T>")
        {
            hasCR = true;
        }

        if (func.returnType.StartsWith("vecN"))
        {
            hasN = true;
        }

        if (func.returnType.Contains("<CF>"))
        {
            func.returnType = func.returnType.Replace("<CF>", "<ST>");
            generics.Remove("CF");
        }

        if (func.returnType.StartsWith("vec"))
        {
        }

        if (func.returnType.Length < 3)
        {
            generics.Add(func.returnType);
        }

        // map return type
        if (genericMap.TryGetValue(func.returnType, out var mapped2))
        {
            func.returnType = mapped2;
        }

        if (func.returnType.StartsWith('_'))
        {
            // write struct here
            yield return $"public partial class {func.returnType};";
        }

        // S -> T if no T
        if (generics.Contains("S") && !generics.Contains("T") && !generics.Contains("T1") && !generics.Contains("T2") &&
            !generics.Contains("ST"))
        {
            hasST = true;
            func.returnType = func.returnType.Replace("S", "T");
            for (var index = 0; index < func.parameters.Length; index++)
            {
                ref var parameter = ref func.parameters[index];
                parameter.Type = parameter.Type.Replace("S", "T");
            }

            generics.Remove("S");
            generics.Add("T");
        }


        // T Expansion
        if (generics.Contains("T") && (func.returnType.StartsWith("vec") || func.returnType.StartsWith("mat")))
        {
            hasST = true;
            func.returnType = func.returnType.Replace("T", "ST");
            for (var index = 0; index < func.parameters.Length; index++)
            {
                ref var parameter = ref func.parameters[index];
                parameter.Type = parameter.Type.Replace("T", "ST");
            }

            generics.Remove("T");
        }

        generics.Remove("ST");
        generics.Remove("CR");
        generics.Remove("N");
        generics.Remove("u32");
        generics.Remove("i32");
        generics.Remove("f32");
        generics.Remove("f16");

        var genericPart = "";
        if (generics.Count > 0)
        {
            genericPart = "<" + string.Join(", ", generics) + ">";
        }

        if (hasST)
        {
            foreach (var type in new List<string> { "f32", "i32", "u32" })
            {
                if (hasN)
                {
                    if (hasCR)
                    {
                        Console.WriteLine("Warning: Unsupported function signature: " + func);
                    }

                    for (int i = 2; i < 5; i++)
                    {
                        var returnType = TypeMapper(func.returnType.Replace("N", i.ToString()).Replace("ST", type));

                        yield return
                            $"public static {returnType} {func.Name}{genericPart}({string.Join(", ", func.parameters.Select(p => $"{TypeMapper(p.Type.Replace("N", i.ToString()).Replace("ST", type))} {p.Name}"))}) => default!;";
                    }
                }
                else if (hasCR)
                {
                    for (int i = 2; i < 5; i++)
                    {
                        for (int j = 2; j < 5; j++)
                        {
                            var returnType = TypeMapper(func.returnType.Replace("C", i.ToString())
                                .Replace("R", j.ToString()).Replace("ST", type));

                            yield return
                                $"public static {returnType} {func.Name}{genericPart}({string.Join(", ", func.parameters.Select(p => $"{TypeMapper(p.Type.Replace("C", i.ToString())
                                    .Replace("R", j.ToString()).Replace("ST", type))} {p.Name}"))}) => default!;";
                        }
                    }
                }
                else
                {
                    var returnType = TypeMapper(func.returnType.Replace("ST", type));

                    yield return
                        $"public static {returnType} {func.Name}{genericPart}({string.Join(", ", func.parameters.Select(p => $"{TypeMapper(p.Type.Replace("ST", type))} {p.Name}"))}) => default!;";
                }
            }


            yield break;
        }

        if (hasCR)
        {
            if (hasN || hasST)
            {
                Console.WriteLine("Warning: Unsupported function signature: " + func);
            }

            for (int i = 2; i < 5; i++)
            {
                for (int j = 2; j < 5; j++)
                {
                    var returnType = TypeMapper(func.returnType.Replace("C", i.ToString()).Replace("R", j.ToString()));

                    yield return
                        $"public static {returnType} {func.Name}{genericPart}({string.Join(", ", func.parameters.Select(p => $"{TypeMapper(p.Type.Replace("C", i.ToString())
                            .Replace("R", j.ToString()))} {p.Name}"))}) => default!;";
                }
            }


            yield break;
        }

        if (hasN)
        {
            if (hasCR || hasST)
            {
                Console.WriteLine("Warning: Unsupported function signature: " + func);
            }

            for (int i = 2; i < 5; i++)
            {
                var returnType = TypeMapper(func.returnType.Replace("N", i.ToString()));

                yield return
                    $"public static {returnType} {func.Name}{genericPart}({string.Join(", ", func.parameters.Select(p => $"{TypeMapper(p.Type.Replace("N", i.ToString()))} {p.Name}"))}) => default!;";
            }


            yield break;
        }


        yield return
            $"public static {TypeMapper(func.returnType)} {func.Name}{genericPart}({string.Join(", ", func.parameters.Select(p => $"{TypeMapper(p.Type)} {p.Name}"))}) => default!;";
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
                _ => typeIn
            };
        }

        if (typeIn.StartsWith("mat"))
        {
            var dimX = uint.Parse(typeIn[3..4]);
            var dimY = uint.Parse(typeIn[5..6]);
            var generic = typeIn[7..].TrimStart('<').TrimEnd('>');
            return generic switch
            {
                "S" or "T" or "f32" or "i32" or "u32" => $"mat{dimX}x{dimY}f",
                _ => typeIn
            };
        }

        return typeIn;
    }

    private static void BacktrackTypes(string bsDocument, List<FunctionDef> functions)
    {
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