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
            _ => throw new NotImplementedException(
                $"Predefined type {predefinedTypeSyntax.Keyword.Text} is not implemented.")
        };
    }

    private string MapString(string a)
    {
        if (KnownTypes.Contains(a)) return a;
        return a switch
        {
            "vec2" => $"vec2<f32>",
            "vec3" => $"vec3<f32>",
            "vec4" => $"vec4<f32>",
            "mat4" => $"mat4x4<f32>",
            "mat3" => $"mat3x3<f32>",
            "ivec2" => $"vec2<i32>",
            "ivec3" => $"vec3<i32>",
            "ivec4" => $"vec4<i32>",
            "var" => "var", // att let promotion pass
            _ => throw new NotImplementedException($"Type {a} is not implemented.")
        };
    }

    public HashSet<string> KnownTypes { get; } =
    [
        "texture_2d", "texture_depth_2d", "sampler"
    ];

    public HashSet<string> KnownMethods { get; } =
    [
        "textureLoad", "textureSample", "floor", "ceil", "max", "min", "sqrt", "pow", "array", "length", "dot",
        "normalize", "cross"
    ];

    public string MapMethod(string name)
    {
        if (char.IsUpper(name[0]))
            name = string.Concat(char.ToLower(name[0]), name.Substring(1));
        if (KnownMethods.Contains(name)) return name;
        return name switch
        {
            "normalized" => "normalize",
            _ => throw new NotImplementedException($"Method {name} is not implemented.")
        };
    }
}
