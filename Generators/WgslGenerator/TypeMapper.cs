using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WgslGenerator;

public class TypeMapper
{
    public string Map(TypeSyntax declarationType)
    {
        return declarationType switch
        {
            PredefinedTypeSyntax predefinedTypeSyntax => MapPredefinedType(predefinedTypeSyntax),
            QualifiedNameSyntax qualifiedNameSyntax => MapString(qualifiedNameSyntax.Right.Identifier.Text),
            IdentifierNameSyntax identifierNameSyntax => MapString(identifierNameSyntax.Identifier.Text),
            ArrayTypeSyntax arrayTypeSyntax => $"array<{Map(arrayTypeSyntax.ElementType)}>",
            GenericNameSyntax genericNameSyntax => MapGenericName(genericNameSyntax),
            _ => throw new NotImplementedException($"Type {declarationType.GetType().Name} is not implemented.")
        };
    }

    private string MapGenericName(GenericNameSyntax genericNameSyntax)
    {
        return
            $"{MapString(genericNameSyntax.Identifier.Text)}<{string.Join(", ", genericNameSyntax.TypeArgumentList.Arguments.Select(Map))}>";
    }


    private string MapPredefinedType(PredefinedTypeSyntax predefinedTypeSyntax)
    {
        return predefinedTypeSyntax.Keyword.Text switch
        {
            "float" => "f32",
            "int" => "i32",
            "uint" => "u32",
            "void" => "void",
            "bool" => "bool",
            _ => "undefined_type"
        };
    }

    private string MapString(string a)
    {
        if (KnownTypes.Contains(a) || UserTypes.Contains(a)) return a;
        return a switch
        {
            "vec2" => "vec2<f32>",
            "vec3" => "vec3<f32>",
            "vec4" => "vec4<f32>",
            "mat2" => "mat2x2<f32>",
            "mat3" => "mat3x3<f32>",
            "mat4" => "mat4x4<f32>",
            "ivec2" => "vec2<i32>",
            "ivec3" => "vec3<i32>",
            "ivec4" => "vec4<i32>",
            "uvec2" => "vec2<u32>",
            "uvec3" => "vec3<u32>",
            "uvec4" => "vec4<u32>",
            "var" => "var", // att let promotion pass
            _ => throw new NotImplementedException($"Type {a} is not implemented.")
        };
    }

    public HashSet<string> UserTypes { get; } = [];

    public HashSet<string> KnownTypes { get; } =
    [
        "f32", "i32", "u32", "f16",

        "AbstractInt", "AbstractFloat",

        "vec2i", "vec3i", "vec4i", "vec2u", "vec3u", "vec4u", "vec2f", "vec3f", "vec4f",
        "vec2<f32>", "vec3<f32>", "vec4<f32>",
        "vec2<i32>", "vec3<i32>", "vec4<i32>",
        "vec2<u32>", "vec3<u32>", "vec4<u32>",

        "mat2x2f", "mat2x3f", "mat2x4f", "mat3x2f", "mat3x3f", "mat3x4f", "mat4x2f", "mat4x3f", "mat4x4f",
        "mat2x2<f32>", "mat2x3<f32>", "mat2x4<f32>", "mat3x2<f32>", "mat3x3<f32>", "mat3x4<f32>", "mat4x2<f32>",
        "mat4x3<f32>", "mat4x4<f32>",

        "texture_1d", "texture_2d", "texture_2d_array", "texture_3d", "texture_cube", "texture_cube_array",
        "texture_multisampled_2d", "texture_storage_1d", "texture_storage_2d", "texture_storage_2d_array",
        "texture_storage_3d", "texture_depth_2d", "texture_depth_2d_array", "texture_depth_cube",
        "texture_depth_cube_array", "texture_depth_multisampled_2d", "texture_external", "sampler",
        "sampler_comparison", "ptr", "atomic", "read_write", "storage", "workgroup",

        "__frexp_result_f32", "__frexp_result_f16", "__frexp_result_abstract", "__frexp_result_vecN_f32",
        "__frexp_result_vecN_f16", "__frexp_result_vecN_abstract", "__modf_result_f32", "__modf_result_f16",
        "__modf_result_abstract", "__modf_result_vecN_f32", "__modf_result_vecN_f16", "__modf_result_vecN_abstract",
        "_atomic_compare_exchange_result",
    ];

    public HashSet<string> KnownMethods { get; } =
    [
        "textureLoad", "textureSample", "floor", "ceil", "max", "min", "sqrt", "pow", "array", "length", "dot",
        "normalize", "cross", "reflect", "refract", "mix", "step", "smoothstep", "distance", "fma"
    ];

    // Helper method to map C# types to WebGPU vertex formats
    public VertexFormat MapTypeToVertexFormat(string typeName)
    {
        return typeName switch
        {
            "vec2" or "Vector2" => VertexFormat.Float32x2,
            "vec3" or "Vector3" => VertexFormat.Float32x3,
            "vec4" or "Vector4" => VertexFormat.Float32x4,
            "float" or "f32" => VertexFormat.Float32,
            "uint" or "u32" => VertexFormat.Uint32,
            "int" or "i32" => VertexFormat.Sint32,
            _ => VertexFormat.Float32x4 // Default format for unknown types
        };
    }
}

public static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
            return str;
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsLower(str[0]))
            return str;
        return char.ToUpperInvariant(str[0]) + str.Substring(1);
    }
}