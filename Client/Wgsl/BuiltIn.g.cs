using System;
using System.Collections.Generic;
using GlmSharp;

namespace Client.Wgsl;

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
    /// <summary>
    /// <![CDATA[Construct [=zero value=] of the type `T`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn T() -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=constructible=] type.<br> ]]></Parameterization>
    public static i32 i32() => default!;
    
    /// <summary>
    /// <![CDATA[Construct [=zero value=] of the type `T`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn T() -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=constructible=] type.<br> ]]></Parameterization>
    public static u32 u32() => default!;
    
    /// <summary>
    /// <![CDATA[Construct [=zero value=] of the type `T`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn T() -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=constructible=] type.<br> ]]></Parameterization>
    public static f32 f32() => default!;
    
    /// <summary>
    /// <![CDATA[Construct a [=bool=] value. If `T` is [=bool=], this is an identity operation.<br> Otherwise this is a boolean coercion. The result is `false` if `e` is a [=zero value=] (or -0.0 for floating point types) and `true` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn bool(e : T) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type. ]]></Parameterization>
    public static bool @bool(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a [=bool=] value. If `T` is [=bool=], this is an identity operation.<br> Otherwise this is a boolean coercion. The result is `false` if `e` is a [=zero value=] (or -0.0 for floating point types) and `true` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn bool(e : T) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type. ]]></Parameterization>
    public static bool @bool(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a [=bool=] value. If `T` is [=bool=], this is an identity operation.<br> Otherwise this is a boolean coercion. The result is `false` if `e` is a [=zero value=] (or -0.0 for floating point types) and `true` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn bool(e : T) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type. ]]></Parameterization>
    public static bool @bool(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=f16=] value. If `T` is [=f16=], this is an identity operation.<br> If `T` is a [=numeric scalar=] (other than [=f16=]), `e` is converted to [=f16=] (including invalid conversions).<br> If `T` is [=bool=], the result is `1.0h` if `e` is `true` and `0.0h` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn f16(e : T) -> f16]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type ]]></Parameterization>
    public static f16 f16(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=f16=] value. If `T` is [=f16=], this is an identity operation.<br> If `T` is a [=numeric scalar=] (other than [=f16=]), `e` is converted to [=f16=] (including invalid conversions).<br> If `T` is [=bool=], the result is `1.0h` if `e` is `true` and `0.0h` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn f16(e : T) -> f16]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type ]]></Parameterization>
    public static f16 f16(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=f16=] value. If `T` is [=f16=], this is an identity operation.<br> If `T` is a [=numeric scalar=] (other than [=f16=]), `e` is converted to [=f16=] (including invalid conversions).<br> If `T` is [=bool=], the result is `1.0h` if `e` is `true` and `0.0h` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn f16(e : T) -> f16]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type ]]></Parameterization>
    public static f16 f16(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=f32=] value. If `T` is [=f32=], this is an identity operation.<br> If `T` is a [=numeric scalar=] (other than [=f32=]), `e` is converted to [=f32=] (including invalid conversions).<br> If `T` is [=bool=], the result is `1.0f` if `e` is `true` and `0.0f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn f32(e : T) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=] type ]]></Parameterization>
    public static f32 f32(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=f32=] value. If `T` is [=f32=], this is an identity operation.<br> If `T` is a [=numeric scalar=] (other than [=f32=]), `e` is converted to [=f32=] (including invalid conversions).<br> If `T` is [=bool=], the result is `1.0f` if `e` is `true` and `0.0f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn f32(e : T) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=] type ]]></Parameterization>
    public static f32 f32(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=f32=] value. If `T` is [=f32=], this is an identity operation.<br> If `T` is a [=numeric scalar=] (other than [=f32=]), `e` is converted to [=f32=] (including invalid conversions).<br> If `T` is [=bool=], the result is `1.0f` if `e` is `true` and `0.0f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn f32(e : T) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=] type ]]></Parameterization>
    public static f32 f32(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=i32=] value. If `T` is [=i32=], this is an identity operation.<br> If `T` is [=u32=], this is a reinterpretation of bits (i.e. the result is the unique value in [=i32=] that has the same bit pattern as `e`).<br> If `T` is a [[#floating-point-types|floating point type]], `e` is [=scalar floating point to integral conversion|converted=] to [=i32=], [=rounding=] towards zero.<br> If `T` is [=bool=], the result is `1i` if `e` is `true` and `0i` otherwise.<br> If `T` is an [=AbstractInt=], this is an identity operation if `e` can be represented in [=i32=], otherwise it produces a [=shader-creation error=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn i32(e : T) -> i32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type ]]></Parameterization>
    public static i32 i32(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=i32=] value. If `T` is [=i32=], this is an identity operation.<br> If `T` is [=u32=], this is a reinterpretation of bits (i.e. the result is the unique value in [=i32=] that has the same bit pattern as `e`).<br> If `T` is a [[#floating-point-types|floating point type]], `e` is [=scalar floating point to integral conversion|converted=] to [=i32=], [=rounding=] towards zero.<br> If `T` is [=bool=], the result is `1i` if `e` is `true` and `0i` otherwise.<br> If `T` is an [=AbstractInt=], this is an identity operation if `e` can be represented in [=i32=], otherwise it produces a [=shader-creation error=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn i32(e : T) -> i32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type ]]></Parameterization>
    public static i32 i32(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct an [=i32=] value. If `T` is [=i32=], this is an identity operation.<br> If `T` is [=u32=], this is a reinterpretation of bits (i.e. the result is the unique value in [=i32=] that has the same bit pattern as `e`).<br> If `T` is a [[#floating-point-types|floating point type]], `e` is [=scalar floating point to integral conversion|converted=] to [=i32=], [=rounding=] towards zero.<br> If `T` is [=bool=], the result is `1i` if `e` is `true` and `0i` otherwise.<br> If `T` is an [=AbstractInt=], this is an identity operation if `e` can be represented in [=i32=], otherwise it produces a [=shader-creation error=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn i32(e : T) -> i32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=scalar=] type ]]></Parameterization>
    public static i32 i32(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 2x2 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x2<T>(e : mat2x2<S>) -> mat2x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x2f mat2x2f(mat2x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 2x2 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x2<T>(v1 : vec2<T>, v2 : vec2<T>) -> mat2x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x2f mat2x2f(vec2f v1, vec2f v2) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 2x2 column-major [=matrix=] from elements. Same as mat2x2(vec2(e1,e2), vec2(e3,e4)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x2<T>(e1 : T, e2 : T, e3 : T, e4 : T) -> mat2x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x2f mat2x2f(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 2x3 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x3<T>(e : mat2x3<S>) -> mat2x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x3f mat2x3f(mat2x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 2x3 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x3<T>(v1 : vec3<T>, v2 : vec3<T>) -> mat2x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x3f mat2x3f(vec3f v1, vec3f v2) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 2x3 column-major [=matrix=] from elements. Same as mat2x3(vec3(e1,e2,e3), vec3(e4,e5,e6)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x3<T>(e1 : T, ..., e6 : T) -> mat2x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x3f mat2x3f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 2x4 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x4<T>(e : mat2x4<S>) -> mat2x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x4f mat2x4f(mat2x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 2x4 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x4<T>(v1 : vec4<T>, v2 : vec4<T>) -> mat2x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x4f mat2x4f(vec4f v1, vec4f v2) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 2x4 column-major [=matrix=] from elements. Same as mat2x4(vec4(e1,e2,e3,e4), vec4(e5,e6,e7,e8)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat2x4<T>(e1 : T, ..., e8 : T) -> mat2x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat2x4f mat2x4f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 3x2 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x2<T>(e : mat3x2<S>) -> mat3x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x2f mat3x2f(mat3x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 3x2 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x2<T>(v1 : vec2<T>, v2 : vec2<T>, v3 : vec2<T>) -> mat3x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x2f mat3x2f(vec2f v1, vec2f v2, vec2f v3) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 3x2 column-major [=matrix=] from elements. Same as mat3x2(vec2(e1,e2), vec2(e3,e4), vec2(e5,e6)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x2<T>(e1 : T, ..., e6 : T) -> mat3x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x2f mat3x2f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 3x3 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x3<T>(e : mat3x3<S>) -> mat3x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x3f mat3x3f(mat3x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 3x3 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x3<T>(v1 : vec3<T>, v2 : vec3<T>, v3 : vec3<T>) -> mat3x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x3f mat3x3f(vec3f v1, vec3f v2, vec3f v3) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 3x3 column-major [=matrix=] from elements. Same as mat3x3(vec3(e1,e2,e3), vec3(e4,e5,e6), vec3(e7,e8,e9)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x3<T>(e1 : T, ..., e9 : T) -> mat3x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x3f mat3x3f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 3x4 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x4<T>(e : mat3x4<S>) -> mat3x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x4f mat3x4f(mat3x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 3x4 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x4<T>(v1 : vec4<T>, v2 : vec4<T>, v3 : vec4<T>) -> mat3x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x4f mat3x4f(vec4f v1, vec4f v2, vec4f v3) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 3x4 column-major [=matrix=] from elements. Same as mat3x4(vec4(e1,e2,e3,e4), vec4(e5,e6,e7,e8), vec4(e9,e10,e11,e12)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat3x4<T>(e1 : T, ..., e12 : T) -> mat3x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat3x4f mat3x4f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 4x2 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x2<T>(e : mat4x2<S>) -> mat4x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x2f mat4x2f(mat4x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 4x2 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x2<T>(v1 : vec2<T>, v2 : vec2<T>, v3 : vec2<T>, v4: vec2<T>) -> mat4x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x2f mat4x2f(vec2f v1, vec2f v2, vec2f v3, vec2f v4) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 4x2 column-major [=matrix=] from elements. Same as mat4x2(vec2(e1,e2), vec2(e3,e4), vec2(e5,e6), vec2(e7,e8)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x2<T>(e1 : T, ..., e8 : T) -> mat4x2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x2f mat4x2f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 4x3 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x3<T>(e : mat4x3<S>) -> mat4x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x3f mat4x3f(mat4x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 4x3 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x3<T>(v1 : vec3<T>, v2 : vec3<T>, v3 : vec3<T>, v4 : vec3<T>) -> mat4x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x3f mat4x3f(vec3f v1, vec3f v2, vec3f v3, vec3f v4) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 4x3 column-major [=matrix=] from elements. Same as mat4x3(vec3(e1,e2,e3), vec3(e4,e5,e6), vec3(e7,e8,e9), vec3(e10,e11,e12)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x3<T>(e1 : T, ..., e12 : T) -> mat4x3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x3f mat4x3f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Constructor for a 4x4 column-major [=matrix=]. If `T` does not match `S`, a [[#floating-point-conversion|conversion]] occurs. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x4<T>(e : mat4x4<S>) -> mat4x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=f16=] or [=f32=]<br> `S` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x4f mat4x4f(mat4x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 4x4 column-major [=matrix=] from column vectors. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x4<T>(v1 : vec4<T>, v2 : vec4<T>, v3 : vec4<T>, v4 : vec4<T>) -> mat4x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x4f mat4x4f(vec4f v1, vec4f v2, vec4f v3, vec4f v4) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a 4x4 column-major [=matrix=] from elements. Same as mat4x4(vec4(e1,e2,e3,e4), vec4(e5,e6,e7,e8), vec4(e9,e10,e11,e12), vec4(e13,e14,e15,e16)). ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mat4x4<T>(e1 : T, ..., e16 : T) -> mat4x4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [FLOATSCALAR] ]]></Parameterization>
    public static mat4x4f mat4x4f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a [=structure=] of type `S` from members. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn S(e1 : T1, ..., eN : TN) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[`S` is a [=constructible=] structure type with members having types `T1` ... `TN`. ]]></Parameterization>
    public static u32 u32(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a [=structure=] of type `S` from members. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn S(e1 : T1, ..., eN : TN) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[`S` is a [=constructible=] structure type with members having types `T1` ... `TN`. ]]></Parameterization>
    public static u32 u32(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construct a [=structure=] of type `S` from members. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn S(e1 : T1, ..., eN : TN) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[`S` is a [=constructible=] structure type with members having types `T1` ... `TN`. ]]></Parameterization>
    public static u32 u32(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a two-component [=vector=] with `e` as both components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2i(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a two-component [=vector=] with `e` as both components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2u(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a two-component [=vector=] with `e` as both components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a two-component [=vector=] with `e` as both components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e : S) -> vec2<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a two-component [=vector=] with `e` as both components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e : S) -> vec2<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a two-component [=vector=] with `e` as both components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e : S) -> vec2<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2i(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2i(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2i(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2u(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2u(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2u(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2f(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2f(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e : vec2<S>) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2f(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e : vec2<S>) -> vec2<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e : vec2<S>) -> vec2<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e.x` and `e.y` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)` and `T(e.y)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e : vec2<S>) -> vec2<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e1` and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e1 : T, e2 : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2i(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e1` and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e1 : T, e2 : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2u(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e1` and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2<T>(e1 : T, e2 : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2f(f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e1` and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e1 : T, e2 : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2i vec2(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e1` and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e1 : T, e2 : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2u vec2(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a two-component [=vector=] with `e1` and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec2(e1 : T, e2 : T) -> vec2<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2f vec2(f32 e1, f32 e2) => default!;

    /// <summary>
    /// <![CDATA[Construction of a three-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a three-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a three-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a three-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e : S) -> vec3<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a three-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e : S) -> vec3<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a three-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e : S) -> vec3<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e : vec3<S>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e : vec3<S>) -> vec3<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e : vec3<S>) -> vec3<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e.x`, `e.y`, and `e.z` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, and `T(e.z)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e : vec3<S>) -> vec3<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `e2`, and `e3` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e1 : T, e2 : T, e3 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(i32 e1, i32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `e2`, and `e3` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e1 : T, e2 : T, e3 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(u32 e1, u32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `e2`, and `e3` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e1 : T, e2 : T, e3 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(f32 e1, f32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `e2`, and `e3` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e1 : T, e2 : T, e3 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3(i32 e1, i32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `e2`, and `e3` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e1 : T, e2 : T, e3 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3(u32 e1, u32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `e2`, and `e3` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e1 : T, e2 : T, e3 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3(f32 e1, f32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `v1.x`, `v1.y`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(v1 : vec2<T>, e1 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(vec2i v1, i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `v1.x`, `v1.y`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(v1 : vec2<T>, e1 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(vec2u v1, u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `v1.x`, `v1.y`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(v1 : vec2<T>, e1 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(vec2f v1, f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `v1.x`, `v1.y`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(v1 : vec2<T>, e1 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3(vec2i v1, i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `v1.x`, `v1.y`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(v1 : vec2<T>, e1 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3(vec2u v1, u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `v1.x`, `v1.y`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(v1 : vec2<T>, e1 : T) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3(vec2f v1, f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e1 : T, v1 : vec2<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3i(i32 e1, vec2i v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e1 : T, v1 : vec2<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3u(u32 e1, vec2u v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3<T>(e1 : T, v1 : vec2<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3f(f32 e1, vec2f v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e1 : T, v1 : vec2<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i vec3(i32 e1, vec2i v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e1 : T, v1 : vec2<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u vec3(u32 e1, vec2u v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a three-component [=vector=] with `e1`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec3(e1 : T, v1 : vec2<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f vec3(f32 e1, vec2f v1) => default!;

    /// <summary>
    /// <![CDATA[Construction of a four-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a four-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a four-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a four-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e : S) -> vec4<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a four-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e : S) -> vec4<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Construction of a four-component [=vector=] with `e` as all components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e : S) -> vec4<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e : vec4<S>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e : vec4<S>) -> vec4<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e : vec4<S>) -> vec4<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e.x`, `e.y`, `e.z`, and `e.w` as components. If `T` does not match `S` a conversion is used and the components are `T(e.x)`, `T(e.y)`, `T(e.z)` and `T(e.w)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e : vec4<S>) -> vec4<S>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=scalar=]<br> `S` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `e3`, and `e4` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(i32 e1, i32 e2, i32 e3, i32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `e3`, and `e4` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(u32 e1, u32 e2, u32 e3, u32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `e3`, and `e4` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `e3`, and `e4` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(i32 e1, i32 e2, i32 e3, i32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `e3`, and `e4` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(u32 e1, u32 e2, u32 e3, u32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `e3`, and `e4` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(i32 e1, vec2i v1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(u32 e1, vec2u v1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(f32 e1, vec2f v1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(i32 e1, vec2i v1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(u32 e1, vec2u v1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(f32 e1, vec2f v1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(i32 e1, i32 e2, vec2i v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(u32 e1, u32 e2, vec2u v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(f32 e1, f32 e2, vec2f v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(i32 e1, i32 e2, vec2i v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(u32 e1, u32 e2, vec2u v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `e2`, `v1.x`, and `v1.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(f32 e1, f32 e2, vec2f v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v2.x`, and `v2.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(vec2i v1, vec2i v2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v2.x`, and `v2.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(vec2u v1, vec2u v2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v2.x`, and `v2.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(vec2f v1, vec2f v2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v2.x`, and `v2.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(vec2i v1, vec2i v2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v2.x`, and `v2.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(vec2u v1, vec2u v2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v2.x`, and `v2.y` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(vec2f v1, vec2f v2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `e1`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(vec2i v1, i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `e1`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(vec2u v1, u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `e1`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(vec2f v1, f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `e1`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(vec2i v1, i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `e1`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(vec2u v1, u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `e1`, and `e2` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(vec2f v1, f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v1.z`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec3<T>, e1 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(vec3i v1, i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v1.z`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec3<T>, e1 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(vec3u v1, u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v1.z`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(v1 : vec3<T>, e1 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(vec3f v1, f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v1.z`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec3<T>, e1 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(vec3i v1, i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v1.z`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec3<T>, e1 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(vec3u v1, u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `v1.x`, `v1.y`, `v1.z`, and `e1` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(v1 : vec3<T>, e1 : T) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(vec3f v1, f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `v1.z` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, v1 : vec3<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4i(i32 e1, vec3i v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `v1.z` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, v1 : vec3<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4u(u32 e1, vec3u v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `v1.z` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4<T>(e1 : T, v1 : vec3<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4f(f32 e1, vec3f v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `v1.z` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, v1 : vec3<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i vec4(i32 e1, vec3i v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `v1.z` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, v1 : vec3<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u vec4(u32 e1, vec3u v1) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] construction of a four-component [=vector=] with `e1`, `v1.x`, `v1.y`, and `v1.z` as components. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn vec4(e1 : T, v1 : vec3<T>) -> vec4<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f vec4(f32 e1, vec3f v1) => default!;

    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn all(e: vecN<bool>) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool all(vec2b e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn all(e: vecN<bool>) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool all(vec3b e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn all(e: vecN<bool>) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool all(vec4b e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn all(e: bool) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool all(bool e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn any(e: vecN<bool>) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool any(vec2b e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn any(e: vecN<bool>) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool any(vec3b e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn any(e: vecN<bool>) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool any(vec4b e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn any(e: bool) -> bool]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static bool any(bool e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: T, t: T, cond: bool) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static i32 select(i32 f, i32 t, bool cond) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: T, t: T, cond: bool) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static u32 select(u32 f, u32 t, bool cond) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `t` when `cond` is true, and `f` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: T, t: T, cond: bool) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] or [=vector=] ]]></Parameterization>
    public static f32 select(f32 f, f32 t, bool cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2i select(vec2i f, vec2i t, vec2b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2u select(vec2u f, vec2u t, vec2b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec2f select(vec2f f, vec2f t, vec2b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3i select(vec3i f, vec3i t, vec3b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3u select(vec3u f, vec3u t, vec3b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec3f select(vec3f f, vec3f t, vec3b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4i select(vec4i f, vec4i t, vec4b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4u select(vec4u f, vec4u t, vec4b cond) => default!;
    
    /// <summary>
    /// <![CDATA[[=Component-wise=] selection. Result component `i` is evaluated as `select(f[i], t[i], cond[i])`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [=scalar=] ]]></Parameterization>
    public static vec4f select(vec4f f, vec4f t, vec4b cond) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=NRuntime=], the number of elements in the [=runtime-sized=] array. See [[#buffer-binding-determines-runtime-sized-array-element-count]] ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn arrayLength(p: ptr<storage, array<E>, AM>) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`E` is an element type for a [=runtime-sized=] array,<br> [=access mode=] `AM` is [=access/read=] or [=access/read_write=] ]]></Parameterization>
    public static u32 arrayLength<AM, E>(ptr<storage, E[], AM> p) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn num_point_lights() -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static u32 num_point_lights() => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static i32 abs(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2i abs(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3i abs(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4i abs(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static f32 abs(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2f abs(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3f abs(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4f abs(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static u32 abs(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2u abs(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3u abs(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[The absolute value of `e`. [=Component-wise=] when `T` is a vector. If `e` is a floating-point type, then the result is `e` with a positive sign bit. If `e` is an unsigned [=integer scalar=] type, then the result is `e`. If `e` is a signed [=integer scalar=] type and evaluates to the largest negative value, then the result is `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn abs(e: T ) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4u abs(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 acos(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f acos(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f acos(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f acos(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 acos(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u acos(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u acos(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u acos(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse cosine (cos<sup>-1</sup>) of `e`.<br> That is, approximates `x` with 0 &le; `x` &le; &pi;, such that `cos`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 acos(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 acosh(f32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f acosh(vec2f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f acosh(vec3f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f acosh(vec4f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 acosh(u32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u acosh(vec2u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u acosh(vec3u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u acosh(vec4u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic cosine (cosh<sup>-1</sup>) of `x`, as a [=hyperbolic angle=].<br> That is, approximates `a` with 0 &le; a &le; [PINF], such that `cosh`(`a`) = `x`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn acosh(x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 acosh(i32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 asin(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f asin(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f asin(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f asin(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 asin(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u asin(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u asin(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u asin(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse sine (sin<sup>-1</sup>) of `e`.<br> That is, approximates `x` with -&pi;/2 &le; `x` &le; &pi;/2, such that `sin`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 asin(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 asinh(f32 y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f asinh(vec2f y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f asinh(vec3f y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f asinh(vec4f y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 asinh(u32 y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u asinh(vec2u y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u asinh(vec3u y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u asinh(vec4u y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic sine (sinh<sup>-1</sup>) of `y`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `sinh`(`y`) = `a`. [=Component-wise=] when `T` is a vector. <!-- No restriction on scalar domain. log(x + sqrt( x * x + 1)) requires x + sqrt( x * x + 1) >= 0 But the sqrt term is always at least as large as abs(x). QED --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn asinh(y: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 asinh(i32 y) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 atan(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f atan(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f atan(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f atan(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 atan(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u atan(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u atan(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u atan(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the principal value, in radians, of the inverse tangent (tan<sup>-1</sup>) of `e`.<br> That is, approximates `x` with &minus; &pi;/2 &le; `x` &le; &pi;/2, such that `tan`(`x`) = `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 atan(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 atanh(f32 t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f atanh(vec2f t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f atanh(vec3f t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f atanh(vec4f t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 atanh(u32 t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u atanh(vec2u t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u atanh(vec3u t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u atanh(vec4u t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the inverse hyperbolic tangent (tanh<sup>-1</sup>) of `t`, as a [=hyperbolic angle=].<br> That is, approximates `a` such that `tanh`(`a`) = `t`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atanh(t: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 atanh(i32 t) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 atan2(f32 y, f32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f atan2(vec2f y, vec2f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f atan2(vec3f y, vec3f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f atan2(vec4f y, vec4f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 atan2(u32 y, u32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u atan2(vec2u y, vec2u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u atan2(vec3u y, vec3u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u atan2(vec4u y, vec4u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns an [=angle=], in radians, in the interval [-&pi;, &pi;] whose tangent is `y`&divide;`x`. The quadrant selected by the result depends on the signs of `y` and `x`. For example, the function may be implemented as: * `atan(y/x)` when `x` > 0 * `atan(y/x)` + &pi; when (`x` < 0) and (`y` > 0) * `atan(y/x)` - &pi; when (`x` < 0) and (`y` < 0) <div class=note> <span class=marker>Note:</span> The error in the result is unbounded: * When `abs(x)` is very small, e.g. [=ieee754/subnormal=] for its type, * At the origin (`x`,`y`) = (0,0), or * When `y` is subnormal or infinite. </div> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atan2(y: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 atan2(i32 y, i32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 ceil(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f ceil(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f ceil(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f ceil(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 ceil(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u ceil(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u ceil(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u ceil(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=ceiling expression|ceiling=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ceil(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 ceil(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static i32 clamp(i32 e, i32 low, i32 high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2i clamp(vec2i e, vec2i low, vec2i high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3i clamp(vec3i e, vec3i low, vec3i high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4i clamp(vec4i e, vec4i low, vec4i high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static f32 clamp(f32 e, f32 low, f32 high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2f clamp(vec2f e, vec2f low, vec2f high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3f clamp(vec3f e, vec3f low, vec3f high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4f clamp(vec4f e, vec4f low, vec4f high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static u32 clamp(u32 e, u32 low, u32 high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2u clamp(vec2u e, vec2u low, vec2u high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3u clamp(vec3u e, vec3u low, vec3u high) => default!;
    
    /// <summary>
    /// <![CDATA[Restricts the value of `e` within a range. If `T` is an integer type, then the result is `min(max(e, low), high)`. If `T` is a floating-point type, then the result is either `min(max(e, low), high)`, or the median of the three values `e`, `low`, `high`. [=Component-wise=] when `T` is a vector. If `low` is greater than `high`, then: * It is a [=shader-creation error=] if `low` and `high` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `low` and `high` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn clamp(e: T, low: T, high: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4u clamp(vec4u e, vec4u low, vec4u high) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 cos(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f cos(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f cos(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f cos(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 cos(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u cos(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u cos(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u cos(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cosine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cos(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 cos(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 cosh(f32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f cosh(vec2f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f cosh(vec3f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f cosh(vec4f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 cosh(u32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u cosh(vec2u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u cosh(vec3u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u cosh(vec4u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic cosine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector <!-- No restriction on domain from the inehrited formula: (exp(x) + (exp(-x))) * 0.5 No danger from subtracting infinites of the same sign. --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cosh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 cosh(i32 a) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static i32 countLeadingZeros(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static u32 countLeadingZeros(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2i countLeadingZeros(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3i countLeadingZeros(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4i countLeadingZeros(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2u countLeadingZeros(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3u countLeadingZeros(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the most significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "clz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countLeadingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4u countLeadingZeros(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static i32 countOneBits(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static u32 countOneBits(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2i countOneBits(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3i countOneBits(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4i countOneBits(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2u countOneBits(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3u countOneBits(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of 1 bits in the representation of `e`.<br> Also known as "population count".<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countOneBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4u countOneBits(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static i32 countTrailingZeros(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static u32 countTrailingZeros(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2i countTrailingZeros(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3i countTrailingZeros(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4i countTrailingZeros(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2u countTrailingZeros(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3u countTrailingZeros(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[The number of consecutive 0 bits starting from the least significant bit of `e`, when `T` is a scalar type.<br> [=Component-wise=] when `T` is a vector.<br> Also known as "ctz" in some languages. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn countTrailingZeros(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4u countTrailingZeros(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cross product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cross(a: vec3<T>, b: vec3<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3f cross(vec3f a, vec3f b) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cross product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cross(a: vec3<T>, b: vec3<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3i cross(vec3i a, vec3i b) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the cross product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn cross(a: vec3<T>, b: vec3<T>) -> vec3<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3u cross(vec3u a, vec3u b) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 degrees(f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f degrees(vec2f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f degrees(vec3f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f degrees(vec4f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 degrees(u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u degrees(vec2u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u degrees(vec3u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u degrees(vec4u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts radians to degrees, approximating `e1`&nbsp;&times;&nbsp;180&nbsp;&div;&nbsp;&pi;. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn degrees(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 degrees(i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the determinant of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn determinant(e: matCxC<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static f32 determinant(mat2x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the determinant of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn determinant(e: matCxC<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static f32 determinant(mat3x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the determinant of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn determinant(e: matCxC<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static f32 determinant(mat4x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 distance(f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 distance(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 distance(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 distance(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 distance(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 distance(vec2u e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 distance(vec3u e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 distance(vec4u e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the distance between `e1` and `e2` (e.g. `length(e1 - e2)`). The [=domain=] is all vectors (|e1|,|e2|) where the subtraction |e1|&minus;|e2| is valid. That is, the set of all vectors except where `e1[i]` and `e2[i]` are the same infinite value, for some component `i`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn distance(e1: T, e2: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 distance(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static i32 dot(vec2i e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static i32 dot(vec3i e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static i32 dot(vec4i e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static u32 dot(vec2u e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static u32 dot(vec3u e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static u32 dot(vec4u e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static f32 dot(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static f32 dot(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the dot product of `e1` and `e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot(e1: vecN<T>, e2: vecN<T>) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractInt, AbstractFloat, i32, u32, f32, or f16 ]]></Parameterization>
    public static f32 dot(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot4U8Packed(e1: u32, e2: u32) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 dot4U8Packed(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dot4I8Packed(e1: u32, e2: u32) -> i32]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 dot4I8Packed(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 exp(f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f exp(vec2f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f exp(vec3f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f exp(vec4f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 exp(u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u exp(vec2u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u exp(vec3u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u exp(vec4u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural exponentiation of `e1` (e.g. `e`<sup>`e1`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 exp(i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 exp2(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f exp2(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f exp2(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f exp2(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 exp2(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u exp2(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u exp2(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u exp2(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 2 raised to the power `e` (e.g. `2`<sup>`e`</sup>). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn exp2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 exp2(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, with sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are the same as bit `c - 1` of the result. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static i32 extractBits(i32 e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, with sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are the same as bit `c - 1` of the result. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static vec2i extractBits(vec2i e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, with sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are the same as bit `c - 1` of the result. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static vec3i extractBits(vec3i e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, with sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are the same as bit `c - 1` of the result. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static vec4i extractBits(vec4i e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, without sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are 0. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static u32 extractBits(u32 e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, without sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are 0. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static vec2u extractBits(vec2u e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, without sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are 0. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static vec3u extractBits(vec3u e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Reads bits from an integer, without sign extension. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is 0 if `c` is 0. <li>Otherwise, bits `0..c - 1` of the result are copied from bits `o..o + c - 1` of `e`. Other bits of the result are 0. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn extractBits(e: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static vec4u extractBits(vec4u e, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` if `dot(e2, e3)` is negative, and `-e1` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn faceForward(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static vec2f faceForward(vec2f e1, vec2f e2, vec2f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` if `dot(e2, e3)` is negative, and `-e1` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn faceForward(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static vec3f faceForward(vec3f e1, vec3f e2, vec3f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` if `dot(e2, e3)` is negative, and `-e1` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn faceForward(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static vec4f faceForward(vec4f e1, vec4f e2, vec4f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` if `dot(e2, e3)` is negative, and `-e1` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn faceForward(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static i32 faceForward(i32 e1, i32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` if `dot(e2, e3)` is negative, and `-e1` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn faceForward(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static u32 faceForward(u32 e1, u32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` if `dot(e2, e3)` is negative, and `-e1` otherwise. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn faceForward(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static f32 faceForward(f32 e1, f32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>-1 if `e` is 0 or -1. <li>Otherwise the position of the most significant bit in `e` that is different from `e`'s sign bit. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static i32 firstLeadingBit(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>-1 if `e` is 0 or -1. <li>Otherwise the position of the most significant bit in `e` that is different from `e`'s sign bit. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static vec2i firstLeadingBit(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>-1 if `e` is 0 or -1. <li>Otherwise the position of the most significant bit in `e` that is different from `e`'s sign bit. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static vec3i firstLeadingBit(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>-1 if `e` is 0 or -1. <li>Otherwise the position of the most significant bit in `e` that is different from `e`'s sign bit. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [SIGNEDINTEGRAL] ]]></Parameterization>
    public static vec4i firstLeadingBit(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the most significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static u32 firstLeadingBit(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the most significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static vec2u firstLeadingBit(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the most significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static vec3u firstLeadingBit(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the most significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstLeadingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [UNSIGNEDINTEGRAL] ]]></Parameterization>
    public static vec4u firstLeadingBit(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static i32 firstTrailingBit(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static u32 firstTrailingBit(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2i firstTrailingBit(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3i firstTrailingBit(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4i firstTrailingBit(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2u firstTrailingBit(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3u firstTrailingBit(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[For scalar `T`, the result is: <ul> <li>`T(-1)` if `e` is zero. <li>Otherwise the position of the least significant 1 bit in `e`. </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn firstTrailingBit(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4u firstTrailingBit(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 floor(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f floor(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f floor(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f floor(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 floor(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u floor(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u floor(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u floor(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the [=floor expression|floor=] of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn floor(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 floor(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 fma(f32 e1, f32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f fma(vec2f e1, vec2f e2, vec2f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f fma(vec3f e1, vec3f e2, vec3f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f fma(vec4f e1, vec4f e2, vec4f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 fma(u32 e1, u32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u fma(vec2u e1, vec2u e2, vec2u e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u fma(vec3u e1, vec3u e2, vec3u e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u fma(vec4u e1, vec4u e2, vec4u e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * e2 + e3`. [=Component-wise=] when `T` is a vector. Note: The name `fma` is short for "fused multiply add". Note: The [[!IEEE-754|IEEE-754]] `fusedMultiplyAdd` operation computes the [=intermediate results=] as if with unbounded range and precision, and only the final result is [=rounding|rounded=] to a value in the destination type. However, the [[#floating-point-accuracy]] rule for `fma` allows an implementation which performs an ordinary multiply to the target type followed by an ordinary addition. In this case the [=intermediate result=] values may overflow or lose accuracy, and the overall operation is not "fused" at all. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fma(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 fma(i32 e1, i32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 fract(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f fract(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f fract(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f fract(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 fract(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u fract(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u fract(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u fract(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the fractional part of `e`, computed as `e - floor(e)`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fract(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 fract(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static i32 insertBits(i32 e, i32 newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static u32 insertBits(u32 e, u32 newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2i insertBits(vec2i e, vec2i newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3i insertBits(vec3i e, vec3i newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4i insertBits(vec4i e, vec4i newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2u insertBits(vec2u e, vec2u newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3u insertBits(vec3u e, vec3u newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Sets bits in an integer. When `T` is a scalar type, then: <ul> <li>`w` is the bit width of `T` <li>`o = min(offset, w)` <li>`c = min(count, w - o)` <li>The result is `e` if `c` is 0. <li>Otherwise, bits `o..o + c - 1` of the result are copied from bits `0..c - 1` of `newbits`. Other bits of the result are copied from `e`. </ul> [=Component-wise=] when `T` is a vector. If `count` + `offset` is greater than `w`, then: * It is a [=shader-creation error=] if `count` and `offset` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `count` and `offset` are [=override-expressions=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4u insertBits(vec4u e, vec4u newbits, u32 offset, u32 count) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 inverseSqrt(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f inverseSqrt(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f inverseSqrt(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f inverseSqrt(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 inverseSqrt(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u inverseSqrt(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u inverseSqrt(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u inverseSqrt(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the reciprocal of `sqrt(e)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn inverseSqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 inverseSqrt(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp(f32 e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static f32 ldexp<I>(f32 e1, I e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2f ldexp(vec2f e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3f ldexp(vec3f e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4f ldexp(vec4f e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2f ldexp(vec2f e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3f ldexp(vec3f e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4f ldexp(vec4f e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2f ldexp(vec2f e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2f ldexp(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3f ldexp(vec3f e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3f ldexp(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4f ldexp(vec4f e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4f ldexp(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp(u32 e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static u32 ldexp<I>(u32 e1, I e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2u ldexp(vec2u e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3u ldexp(vec3u e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4u ldexp(vec4u e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2u ldexp(vec2u e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3u ldexp(vec3u e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4u ldexp(vec4u e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2u ldexp(vec2u e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2u ldexp(vec2u e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3u ldexp(vec3u e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3u ldexp(vec3u e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4u ldexp(vec4u e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4u ldexp(vec4u e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static i32 ldexp(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static i32 ldexp(i32 e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static i32 ldexp(i32 e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static i32 ldexp(i32 e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2f ldexp<V1>(vec2f e1, V1 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3f ldexp<V2>(vec3f e1, V2 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4f ldexp<V3>(vec4f e1, V3 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec2u ldexp<V4>(vec2u e1, V4 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec3u ldexp<V5>(vec3u e1, V5 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static vec4u ldexp<V6>(vec4u e1, V6 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1 * 2`<sup>`e2`</sup>, except: * The result may be zero if `e2` + *bias* &le; 0. * If `e2` > *bias* + 1 * It is a [=shader-creation error=] if `e2` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e2` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. Here, *bias* is the exponent bias of the floating point format: * 15 for `f16` * 127 for `f32` * 1023 for AbstractFloat, when AbstractFloat is [[!IEEE-754|IEEE-754]] [=ieee754/binary64=] If `x` is zero or a finite normal value for its type, then: <blockquote> x = ldexp(frexp(x).fract, frexp(x).exp) </blockquote> [=Component-wise=] when `T` is a vector. Note: A mnemonic for the name `ldexp` is "load exponent". The name may have been taken from the corresponding instruction in the floating point unit of the PDP-11. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn ldexp(e1: T, e2: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL]<br> `I` is [ALLSIGNEDINTEGRAL]<br> `I` is a vector if and only if `T` is a vector<br> `T` can only be [=type/abstract=] if `I` is also [=type/abstract=] and vice versa Note: If either parameter is [=type/concrete=] then the other parameter will undergo [=feasible automatic conversion|automatic conversion=] to a [=type/concrete=] type (if applicable) and the result will be a [=type/concrete=] type. ]]></Parameterization>
    public static i32 ldexp<I>(i32 e1, I e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 length(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 length(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 length(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 length(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 length(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 length(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 length(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 length(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the length of `e`.<br> Evaluates to the absolute value of `e` if `T` is [=scalar=].<br> Evaluates to `sqrt(e[0]`<sup>`2`</sup> `+ e[1]`<sup>`2`</sup> `+ ...)` if `T` is a vector type. Note: The scalar case may be evaluated as `sqrt(e * e)`, which may unnecessarily overflow or lose accuracy. <!-- No restriction on domain from formula sqrt(x*x) --> ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn length(e: T) -> S]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 length(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 log(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f log(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f log(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f log(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 log(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u log(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u log(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u log(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the natural logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 log(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 log2(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f log2(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f log2(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f log2(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 log2(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u log2(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u log2(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u log2(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the base-2 logarithm of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn log2(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 log2(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static i32 max(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2i max(vec2i e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3i max(vec3i e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4i max(vec4i e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static f32 max(f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2f max(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3f max(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4f max(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static u32 max(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2u max(vec2u e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3u max(vec3u e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e1` is less than `e2`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn max(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4u max(vec4u e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static i32 min(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2i min(vec2i e1, vec2i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3i min(vec3i e1, vec3i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4i min(vec4i e1, vec4i e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static f32 min(f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2f min(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3f min(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4f min(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static u32 min(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec2u min(vec2u e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec3u min(vec3u e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e2` if `e2` is less than `e1`, and `e1` otherwise. [=Component-wise=] when `T` is a vector. If `e1` and `e2` are floating-point values, then: * If both `e1` and `e2` are [=ieee754/subnormal=], then the result may be *either* value. * If one operand is a NaN, the other is returned. * If both operands are NaNs, a NaN is returned. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn min(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLNUMERICDECL] ]]></Parameterization>
    public static vec4u min(vec4u e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 mix(f32 e1, f32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f mix(vec2f e1, vec2f e2, vec2f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f mix(vec3f e1, vec3f e2, vec3f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f mix(vec4f e1, vec4f e2, vec4f e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 mix(u32 e1, u32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u mix(vec2u e1, vec2u e2, vec2u e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u mix(vec3u e1, vec3u e2, vec3u e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u mix(vec4u e1, vec4u e2, vec4u e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the linear blend of `e1` and `e2` (e.g. `e1 * (T(1) - e3) + e2 * e3`). [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T, e2: T, e3: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 mix(i32 e1, i32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static i32 mix(i32 e1, i32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static u32 mix(u32 e1, u32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec2i mix(vec2i e1, vec2i e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec2u mix(vec2u e1, vec2u e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec2f mix(vec2f e1, vec2f e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec3i mix(vec3i e1, vec3i e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec3u mix(vec3u e1, vec3u e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec3f mix(vec3f e1, vec3f e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec4i mix(vec4i e1, vec4i e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec4u mix(vec4u e1, vec4u e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec4f mix(vec4f e1, vec4f e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static i32 mix(i32 e1, i32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static u32 mix(u32 e1, u32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static f32 mix(f32 e1, f32 e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static f32 mix(f32 e1, f32 e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec2i mix(vec2i e1, vec2i e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec2u mix(vec2u e1, vec2u e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec3i mix(vec3i e1, vec3i e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec3u mix(vec3u e1, vec3u e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec4i mix(vec4i e1, vec4i e2, i32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the component-wise linear blend of `e1` and `e2`, using scalar blending factor `e3` for each component.<br> Same as `mix(e1, e2, T2(e3))`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn mix(e1: T2, e2: T2, e3: T) -> T2]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16<br> `T2` is vecN<T> ]]></Parameterization>
    public static vec4u mix(vec4u e1, vec4u e2, u32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec2f normalize(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3f normalize(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec4f normalize(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec2i normalize(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec2u normalize(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3i normalize(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3u normalize(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec4i normalize(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns a unit vector in the same direction as `e`. The [=domain=] is all vectors except the zero vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn normalize(e: vecN<T> ) -> vecN<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec4u normalize(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 pow(f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f pow(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f pow(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f pow(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 pow(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u pow(vec2u e1, vec2u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u pow(vec3u e1, vec3u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u pow(vec4u e1, vec4u e2) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e1` raised to the power `e2`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pow(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 pow(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[Quantizes a 32-bit floating point value `e` as if `e` were converted to a [[!IEEE-754|IEEE-754]] [=ieee754/binary16=] value, and then converted back to a IEEE-754 [=ieee754/binary32=] value. If `e` is outside the [=finite range=] of binary16, then: * It is a [=shader-creation error=] if `e` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. The intermediate binary16 value may be [=flushed to zero=], i.e. the final result may be zero if the intermediate binary16 value is [=ieee754/subnormal=]. See [[#floating-point-conversion]]. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quantizeToF16(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 quantizeToF16(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Quantizes a 32-bit floating point value `e` as if `e` were converted to a [[!IEEE-754|IEEE-754]] [=ieee754/binary16=] value, and then converted back to a IEEE-754 [=ieee754/binary32=] value. If `e` is outside the [=finite range=] of binary16, then: * It is a [=shader-creation error=] if `e` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. The intermediate binary16 value may be [=flushed to zero=], i.e. the final result may be zero if the intermediate binary16 value is [=ieee754/subnormal=]. See [[#floating-point-conversion]]. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quantizeToF16(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f quantizeToF16(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Quantizes a 32-bit floating point value `e` as if `e` were converted to a [[!IEEE-754|IEEE-754]] [=ieee754/binary16=] value, and then converted back to a IEEE-754 [=ieee754/binary32=] value. If `e` is outside the [=finite range=] of binary16, then: * It is a [=shader-creation error=] if `e` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. The intermediate binary16 value may be [=flushed to zero=], i.e. the final result may be zero if the intermediate binary16 value is [=ieee754/subnormal=]. See [[#floating-point-conversion]]. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quantizeToF16(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f quantizeToF16(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Quantizes a 32-bit floating point value `e` as if `e` were converted to a [[!IEEE-754|IEEE-754]] [=ieee754/binary16=] value, and then converted back to a IEEE-754 [=ieee754/binary32=] value. If `e` is outside the [=finite range=] of binary16, then: * It is a [=shader-creation error=] if `e` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `e` is an [=override-expression=]. * Otherwise the result is an [=indeterminate value=] for `T`. The intermediate binary16 value may be [=flushed to zero=], i.e. the final result may be zero if the intermediate binary16 value is [=ieee754/subnormal=]. See [[#floating-point-conversion]]. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quantizeToF16(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f quantizeToF16(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 radians(f32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f radians(vec2f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f radians(vec3f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f radians(vec4f e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 radians(u32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u radians(vec2u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u radians(vec3u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u radians(vec4u e1) => default!;
    
    /// <summary>
    /// <![CDATA[Converts degrees to radians, approximating `e1`&nbsp;&times;&nbsp;&pi;&nbsp;&div;&nbsp;180. [=Component-wise=] when `T` is a vector ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn radians(e1: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 radians(i32 e1) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface orientation `e2`, returns the reflection direction `e1 - 2 * dot(e2, e1) * e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reflect(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static vec2f reflect(vec2f e1, vec2f e2) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface orientation `e2`, returns the reflection direction `e1 - 2 * dot(e2, e1) * e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reflect(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static vec3f reflect(vec3f e1, vec3f e2) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface orientation `e2`, returns the reflection direction `e1 - 2 * dot(e2, e1) * e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reflect(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static vec4f reflect(vec4f e1, vec4f e2) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface orientation `e2`, returns the reflection direction `e1 - 2 * dot(e2, e1) * e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reflect(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static i32 reflect(i32 e1, i32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface orientation `e2`, returns the reflection direction `e1 - 2 * dot(e2, e1) * e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reflect(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static u32 reflect(u32 e1, u32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface orientation `e2`, returns the reflection direction `e1 - 2 * dot(e2, e1) * e2`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reflect(e1: T, e2: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<AbstractFloat>, vecN<f32>, or vecN<f16> ]]></Parameterization>
    public static f32 reflect(f32 e1, f32 e2) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec2f refract(vec2f e1, vec2f e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec3f refract(vec3f e1, vec3f e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static vec4f refract(vec4f e1, vec4f e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static i32 refract(i32 e1, i32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static u32 refract(u32 e1, u32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static f32 refract(f32 e1, f32 e2, f32 e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static i32 refract<I>(i32 e1, i32 e2, I e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static u32 refract<I>(u32 e1, u32 e2, I e3) => default!;
    
    /// <summary>
    /// <![CDATA[For the incident vector `e1` and surface normal `e2`, and the ratio of indices of refraction `e3`, let `k = 1.0 - e3 * e3 * (1.0 - dot(e2, e1) * dot(e2, e1))`. If `k < 0.0`, returns the refraction vector 0.0, otherwise return the refraction vector `e3 * e1 - (e3 * dot(e2, e1) + sqrt(k)) * e2`. The incident vector `e1` and the normal `e2` should be normalized for desired results according to Snell's Law; otherwise, the results may not conform to expected physical behavior. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn refract(e1: T, e2: T, e3: I) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is vecN<I><br> `I` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static f32 refract<I>(f32 e1, f32 e2, I e3) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static i32 reverseBits(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static u32 reverseBits(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2i reverseBits(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3i reverseBits(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4i reverseBits(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec2u reverseBits(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec3u reverseBits(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Reverses the bits in `e`: The bit at position `k` of the result equals the bit at position `31 -k` of `e`.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn reverseBits(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is [INTEGRAL] ]]></Parameterization>
    public static vec4u reverseBits(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 round(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f round(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f round(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f round(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 round(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u round(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u round(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u round(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is the integer `k` nearest to `e`, as a floating point value.<br> When `e` lies halfway between integers `k` and `k + 1`, the result is `k` when `k` is even, and `k + 1` when `k` is odd.<br> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn round(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 round(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 saturate(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f saturate(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f saturate(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f saturate(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 saturate(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u saturate(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u saturate(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u saturate(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `clamp(e, 0.0, 1.0)`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn saturate(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 saturate(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static i32 sign(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static vec2i sign(vec2i e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static vec3i sign(vec3i e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static vec4i sign(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static f32 sign(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static vec2f sign(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static vec3f sign(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static vec4f sign(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Result is: <ul> <li> 1 when `e` > 0 <li> 0 when `e` = 0 <li> -1 when `e` < 0 </ul> [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sign(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLSIGNEDNUMERICDECL] ]]></Parameterization>
    public static u32 sign(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 sin(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f sin(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f sin(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f sin(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 sin(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u sin(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u sin(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u sin(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the sine of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sin(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 sin(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 sinh(f32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f sinh(vec2f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f sinh(vec3f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f sinh(vec4f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 sinh(u32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u sinh(vec2u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u sinh(vec3u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u sinh(vec4u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic sine of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>)&divide;2, but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sinh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 sinh(i32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 smoothstep(f32 edge0, f32 edge1, f32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f smoothstep(vec2f edge0, vec2f edge1, vec2f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f smoothstep(vec3f edge0, vec3f edge1, vec3f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f smoothstep(vec4f edge0, vec4f edge1, vec4f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 smoothstep(u32 edge0, u32 edge1, u32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u smoothstep(vec2u edge0, vec2u edge1, vec2u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u smoothstep(vec3u edge0, vec3u edge1, vec3u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u smoothstep(vec4u edge0, vec4u edge1, vec4u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the smooth Hermite interpolation between 0 and 1. [=Component-wise=] when `T` is a vector. For scalar `T`, the result is `t * t * (3.0 - 2.0 * t)`,<br> where `t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0)`. Qualitatively: * When `edge0` < `edge1`, the function is 0 for `x` below `edge0`, then smoothly rises until `x` reaches `edge1`, and remains at 1 afterward. * When `edge0` > `edge1`, the function is 1 for `x` below `edge1`, then smoothly descends until `x` reaches `edge0`, and remains at 0 afterward. If `edge0` &equals; `edge1`: * It is a [=shader-creation error=] if `edge0` and `edge1` are [=const-expressions=]. * It is a [=pipeline-creation error=] if `edge0` and `edge1` are [=override-expressions=]. * Otherwise, the result is an [=indeterminate value=] for `T`. In this case the computation performs a floating point [=ieee754/division by zero=], and the [=Finite Math Assumption=] applies. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn smoothstep(edge0: T, edge1: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 smoothstep(i32 edge0, i32 edge1, i32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 sqrt(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f sqrt(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f sqrt(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f sqrt(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 sqrt(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u sqrt(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u sqrt(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u sqrt(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the square root of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn sqrt(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 sqrt(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 step(f32 edge, f32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f step(vec2f edge, vec2f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f step(vec3f edge, vec3f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f step(vec4f edge, vec4f x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 step(u32 edge, u32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u step(vec2u edge, vec2u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u step(vec3u edge, vec3u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u step(vec4u edge, vec4u x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns 1.0 if `edge` &le; `x`, and 0.0 otherwise. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn step(edge: T, x: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 step(i32 edge, i32 x) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 tan(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f tan(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f tan(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f tan(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 tan(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u tan(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u tan(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u tan(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the tangent of `e`, where `e` is in radians. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tan(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 tan(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 tanh(f32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f tanh(vec2f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f tanh(vec3f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f tanh(vec4f a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 tanh(u32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u tanh(vec2u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u tanh(vec3u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u tanh(vec4u a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the hyperbolic tangent of `a`, where `a` is a [=hyperbolic angle=]. Approximates the pure mathematical function (*e*<sup>a</sup> &minus; *e*<sup>&minus;a</sup>) &divide; (*e*<sup>a</sup> + *e*<sup>&minus;a</sup>) but not necessarily computed that way. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn tanh(a: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 tanh(i32 a) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat2x2f transpose(mat2x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat2x3f transpose(mat3x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat2x4f transpose(mat4x2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat3x2f transpose(mat2x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat3x3f transpose(mat3x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat3x4f transpose(mat4x3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat4x2f transpose(mat2x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat4x3f transpose(mat3x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the transpose of `e`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn transpose(e: matRxC<T>) -> matCxR<T>]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is AbstractFloat, f32, or f16 ]]></Parameterization>
    public static mat4x4f transpose(mat4x4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static f32 trunc(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2f trunc(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3f trunc(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4f trunc(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static u32 trunc(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec2u trunc(vec2u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec3u trunc(vec3u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static vec4u trunc(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns [=truncate=](`e`), the nearest whole number whose absolute value is less than or equal to the absolute value of `e`. [=Component-wise=] when `T` is a vector. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn trunc(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[[ALLFLOATINGDECL] ]]></Parameterization>
    public static i32 trunc(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window x coordinates. The result is the same as either `dpdxFine(e)` or `dpdxCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdx(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 dpdx(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window x coordinates. The result is the same as either `dpdxFine(e)` or `dpdxCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdx(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f dpdx(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window x coordinates. The result is the same as either `dpdxFine(e)` or `dpdxCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdx(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f dpdx(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window x coordinates. The result is the same as either `dpdxFine(e)` or `dpdxCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdx(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f dpdx(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates using local differences. This may result in fewer unique positions than `dpdxFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 dpdxCoarse(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates using local differences. This may result in fewer unique positions than `dpdxFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f dpdxCoarse(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates using local differences. This may result in fewer unique positions than `dpdxFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f dpdxCoarse(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates using local differences. This may result in fewer unique positions than `dpdxFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f dpdxCoarse(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 dpdxFine(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f dpdxFine(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f dpdxFine(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window x coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdxFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f dpdxFine(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window y coordinates. The result is the same as either `dpdyFine(e)` or `dpdyCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdy(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 dpdy(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window y coordinates. The result is the same as either `dpdyFine(e)` or `dpdyCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdy(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f dpdy(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window y coordinates. The result is the same as either `dpdyFine(e)` or `dpdyCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdy(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f dpdy(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Partial derivative of `e` with respect to window y coordinates. The result is the same as either `dpdyFine(e)` or `dpdyCoarse(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdy(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f dpdy(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates using local differences. This may result in fewer unique positions than `dpdyFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 dpdyCoarse(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates using local differences. This may result in fewer unique positions than `dpdyFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f dpdyCoarse(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates using local differences. This may result in fewer unique positions than `dpdyFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f dpdyCoarse(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates using local differences. This may result in fewer unique positions than `dpdyFine(e)`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f dpdyCoarse(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 dpdyFine(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f dpdyFine(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f dpdyFine(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the partial derivative of `e` with respect to window y coordinates. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn dpdyFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f dpdyFine(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdx(e)) + abs(dpdy(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidth(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 fwidth(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdx(e)) + abs(dpdy(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidth(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f fwidth(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdx(e)) + abs(dpdy(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidth(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f fwidth(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdx(e)) + abs(dpdy(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidth(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f fwidth(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxCoarse(e)) + abs(dpdyCoarse(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 fwidthCoarse(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxCoarse(e)) + abs(dpdyCoarse(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f fwidthCoarse(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxCoarse(e)) + abs(dpdyCoarse(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f fwidthCoarse(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxCoarse(e)) + abs(dpdyCoarse(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthCoarse(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f fwidthCoarse(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxFine(e)) + abs(dpdyFine(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static f32 fwidthFine(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxFine(e)) + abs(dpdyFine(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec2f fwidthFine(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxFine(e)) + abs(dpdyFine(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec3f fwidthFine(vec3f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `abs(dpdxFine(e)) + abs(dpdyFine(e))`. Returns an [=indeterminate value=] if called in [=uniform control flow|non-uniform control flow=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn fwidthFine(e: T) -> T]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is f32 or vecN<f32> ]]></Parameterization>
    public static vec4f fwidthFine(vec4f e) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions(texture_depth_2d t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions(texture_depth_2d_array t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions(texture_depth_cube t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions(texture_depth_cube_array t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions(texture_depth_multisampled_2d t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions(texture_external t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T, level: L) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions<L>(texture_depth_2d t, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T, level: L) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions<L>(texture_depth_2d_array t, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T, level: L) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions<L>(texture_depth_cube t, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureDimensions(t: T, level: L) -> vec2<u32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec2u textureDimensions<L>(texture_depth_cube_array t, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(i32 component, texture_2d<i32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(u32 component, texture_2d<i32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(f32 component, texture_2d<i32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(i32 component, texture_2d<u32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(u32 component, texture_2d<u32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(f32 component, texture_2d<u32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(i32 component, texture_2d<f32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(u32 component, texture_2d<f32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(f32 component, texture_2d<f32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(i32 component, texture_2d<i32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(u32 component, texture_2d<i32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(f32 component, texture_2d<i32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(i32 component, texture_2d<u32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(u32 component, texture_2d<u32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(f32 component, texture_2d<u32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(i32 component, texture_2d<f32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(u32 component, texture_2d<f32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(f32 component, texture_2d<f32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(i32 component, texture_2d_array<i32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(u32 component, texture_2d_array<i32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(f32 component, texture_2d_array<i32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(i32 component, texture_2d_array<u32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(u32 component, texture_2d_array<u32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(f32 component, texture_2d_array<u32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(i32 component, texture_2d_array<f32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(u32 component, texture_2d_array<f32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(f32 component, texture_2d_array<f32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(i32 component, texture_2d_array<i32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(u32 component, texture_2d_array<i32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(f32 component, texture_2d_array<i32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(i32 component, texture_2d_array<u32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(u32 component, texture_2d_array<u32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(f32 component, texture_2d_array<u32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(i32 component, texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(u32 component, texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(f32 component, texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(i32 component, texture_cube<i32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(u32 component, texture_cube<i32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather(f32 component, texture_cube<i32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(i32 component, texture_cube<u32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(u32 component, texture_cube<u32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather(f32 component, texture_cube<u32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(i32 component, texture_cube<f32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(u32 component, texture_cube<f32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(f32 component, texture_cube<f32> t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(i32 component, texture_cube_array<i32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(u32 component, texture_cube_array<i32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureGather<A>(f32 component, texture_cube_array<i32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(i32 component, texture_cube_array<u32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(u32 component, texture_cube_array<u32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureGather<A>(f32 component, texture_cube_array<u32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(i32 component, texture_cube_array<f32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(u32 component, texture_cube_array<f32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(f32 component, texture_cube_array<f32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(t: texture_depth_2d, s: sampler, coords: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(texture_depth_2d t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(t: texture_depth_2d, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(texture_depth_2d t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(t: texture_depth_cube, s: sampler, coords: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather(texture_depth_cube t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(texture_depth_2d_array t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(texture_depth_2d_array t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGather(t: texture_depth_cube_array, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGather<A>(texture_depth_cube_array t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn gather_x_components(c: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f gather_x_components(vec2f c) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn gather_y_components(c: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f gather_y_components(vec2f c) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn gather_z_components(c: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f gather_z_components(vec2f c) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn gather_depth_components(c: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f gather_depth_components(vec2f c) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGatherCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGatherCompare(texture_depth_2d t, sampler_comparison s, vec2f coords, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGatherCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGatherCompare(texture_depth_2d t, sampler_comparison s, vec2f coords, f32 depth_ref, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGatherCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGatherCompare<A>(texture_depth_2d_array t, sampler_comparison s, vec2f coords, A array_index, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGatherCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGatherCompare<A>(texture_depth_2d_array t, sampler_comparison s, vec2f coords, A array_index, f32 depth_ref, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGatherCompare(t: texture_depth_cube, s: sampler_comparison, coords: vec3<f32>, depth_ref: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGatherCompare(texture_depth_cube t, sampler_comparison s, vec3f coords, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureGatherCompare(t: texture_depth_cube_array, s: sampler_comparison, coords: vec3<f32>, array_index: A, depth_ref: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureGatherCompare<A>(texture_depth_cube_array t, sampler_comparison s, vec3f coords, A array_index, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn gather_depth_compare(c: vec2<f32>, depth_ref: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f gather_depth_compare(vec2f c, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_1d<i32> t, i32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_1d<i32> t, u32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_1d<i32> t, f32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_1d<u32> t, i32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_1d<u32> t, u32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_1d<u32> t, f32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_1d<f32> t, i32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_1d<f32> t, u32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_1d<f32> t, f32 coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_2d<i32> t, vec2i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_2d<i32> t, vec2u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_2d<i32> t, vec2f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_2d<u32> t, vec2i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_2d<u32> t, vec2u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_2d<u32> t, vec2f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_2d<f32> t, vec2i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_2d<f32> t, vec2u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_2d<f32> t, vec2f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<A, L>(texture_2d_array<i32> t, vec2i coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<A, L>(texture_2d_array<i32> t, vec2u coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<A, L>(texture_2d_array<i32> t, vec2f coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<A, L>(texture_2d_array<u32> t, vec2i coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<A, L>(texture_2d_array<u32> t, vec2u coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<A, L>(texture_2d_array<u32> t, vec2f coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<A, L>(texture_2d_array<f32> t, vec2i coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<A, L>(texture_2d_array<f32> t, vec2u coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<A, L>(texture_2d_array<f32> t, vec2f coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_3d<i32> t, vec3i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_3d<i32> t, vec3u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<L>(texture_3d<i32> t, vec3f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_3d<u32> t, vec3i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_3d<u32> t, vec3u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<L>(texture_3d<u32> t, vec3f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_3d<f32> t, vec3i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_3d<f32> t, vec3u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<L>(texture_3d<f32> t, vec3f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2i coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2u coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2f coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2i coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2u coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2f coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2i coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2u coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_multisampled_2d<i32> t, vec2f coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2i coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2u coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2f coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2i coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2u coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2f coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2i coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2u coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_multisampled_2d<u32> t, vec2f coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2i coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2u coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2f coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2i coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2u coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2f coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2i coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2u coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_multisampled_2d<f32> t, vec2f coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_2d, coords: vec2<C>, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad<L>(texture_depth_2d t, vec2i coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_2d, coords: vec2<C>, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad<L>(texture_depth_2d t, vec2u coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_2d, coords: vec2<C>, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad<L>(texture_depth_2d t, vec2f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_2d_array, coords: vec2<C>, array_index: A, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad<A, L>(texture_depth_2d_array t, vec2i coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_2d_array, coords: vec2<C>, array_index: A, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad<A, L>(texture_depth_2d_array t, vec2u coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_2d_array, coords: vec2<C>, array_index: A, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad<A, L>(texture_depth_2d_array t, vec2f coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2i coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2u coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2f coords, i32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2i coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2u coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2f coords, u32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2i coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2u coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureLoad(texture_depth_multisampled_2d t, vec2f coords, f32 sample_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_external, coords: vec2<C>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_external t, vec2i coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_external, coords: vec2<C>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_external t, vec2u coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t: texture_external, coords: vec2<C>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_external t, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_1d<F, AM>, coords : C) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_storage_1d<i32> t, i32 coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_1d<F, AM>, coords : C) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_storage_1d<u32> t, u32 coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_1d<F, AM>, coords : C) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_storage_1d<f32> t, f32 coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_2d<F, AM>, coords : vec2<C>) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_storage_2d<i32> t, vec2i coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_2d<F, AM>, coords : vec2<C>) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_storage_2d<u32> t, vec2u coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_2d<F, AM>, coords : vec2<C>) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_storage_2d<f32> t, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_2d_array<F, AM>, coords : vec2<C>, array_index : A) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad<A>(texture_storage_2d_array<i32> t, vec2i coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_2d_array<F, AM>, coords : vec2<C>, array_index : A) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad<A>(texture_storage_2d_array<u32> t, vec2u coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_2d_array<F, AM>, coords : vec2<C>, array_index : A) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad<A>(texture_storage_2d_array<f32> t, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_3d<F, AM>, coords : vec3<C>) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4i textureLoad(texture_storage_3d<i32> t, vec3i coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_3d<F, AM>, coords : vec3<C>) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4u textureLoad(texture_storage_3d<u32> t, vec3u coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureLoad(t : texture_storage_3d<F, AM>, coords : vec3<C>) -> vec4<CF>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureLoad(texture_storage_3d<f32> t, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumLayers(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumLayers(i32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumLayers(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumLayers(u32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumLayers(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumLayers(f32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumLevels(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumLevels(i32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumLevels(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumLevels(u32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumLevels(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumLevels(f32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumSamples(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumSamples(i32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumSamples(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumSamples(u32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureNumSamples(t: T) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static u32 textureNumSamples(f32 t) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_1d<f32>, s: sampler, coords: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(texture_1d<f32> t, sampler s, f32 coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_2d<f32>, s: sampler, coords: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(texture_2d<f32> t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(texture_2d<f32> t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: T, s: sampler, coords: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(i32 t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: T, s: sampler, coords: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(u32 t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: T, s: sampler, coords: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(f32 t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, offset: vec3<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample(texture_3d<f32> t, sampler s, vec3f coords, vec3i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSample<A>(texture_cube_array<f32> t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_depth_2d, s: sampler, coords: vec2<f32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSample(texture_depth_2d t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_depth_2d, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSample(texture_depth_2d t, sampler s, vec2f coords, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSample<A>(texture_depth_2d_array t, sampler s, vec2f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSample<A>(texture_depth_2d_array t, sampler s, vec2f coords, A array_index, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_depth_cube, s: sampler, coords: vec3<f32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSample(texture_depth_cube t, sampler s, vec3f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSample(t: texture_depth_cube_array, s: sampler, coords: vec3<f32>, array_index: A) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSample<A>(texture_depth_cube_array t, sampler s, vec3f coords, A array_index) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, bias: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias(texture_2d<f32> t, sampler s, vec2f coords, f32 bias) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, bias: f32, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias(texture_2d<f32> t, sampler s, vec2f coords, f32 bias, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, bias: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, f32 bias) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, bias: f32, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, f32 bias, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: T, s: sampler, coords: vec3<f32>, bias: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias(i32 t, sampler s, vec3f coords, f32 bias) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: T, s: sampler, coords: vec3<f32>, bias: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias(u32 t, sampler s, vec3f coords, f32 bias) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: T, s: sampler, coords: vec3<f32>, bias: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias(f32 t, sampler s, vec3f coords, f32 bias) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, bias: f32, offset: vec3<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias(texture_3d<f32> t, sampler s, vec3f coords, f32 bias, vec3i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBias(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A, bias: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBias<A>(texture_cube_array<f32> t, sampler s, vec3f coords, A array_index, f32 bias) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompare(texture_depth_2d t, sampler_comparison s, vec2f coords, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompare(texture_depth_2d t, sampler_comparison s, vec2f coords, f32 depth_ref, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompare<A>(texture_depth_2d_array t, sampler_comparison s, vec2f coords, A array_index, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompare<A>(texture_depth_2d_array t, sampler_comparison s, vec2f coords, A array_index, f32 depth_ref, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompare(t: texture_depth_cube, s: sampler_comparison, coords: vec3<f32>, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompare(texture_depth_cube t, sampler_comparison s, vec3f coords, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompare(t: texture_depth_cube_array, s: sampler_comparison, coords: vec3<f32>, array_index: A, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompare<A>(texture_depth_cube_array t, sampler_comparison s, vec3f coords, A array_index, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompareLevel(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompareLevel(texture_depth_2d t, sampler_comparison s, vec2f coords, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompareLevel(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompareLevel(texture_depth_2d t, sampler_comparison s, vec2f coords, f32 depth_ref, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompareLevel(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompareLevel<A>(texture_depth_2d_array t, sampler_comparison s, vec2f coords, A array_index, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompareLevel(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompareLevel<A>(texture_depth_2d_array t, sampler_comparison s, vec2f coords, A array_index, f32 depth_ref, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompareLevel(t: texture_depth_cube, s: sampler_comparison, coords: vec3<f32>, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompareLevel(texture_depth_cube t, sampler_comparison s, vec3f coords, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleCompareLevel(t: texture_depth_cube_array, s: sampler_comparison, coords: vec3<f32>, array_index: A, depth_ref: f32) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleCompareLevel<A>(texture_depth_cube_array t, sampler_comparison s, vec3f coords, A array_index, f32 depth_ref) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, ddx: vec2<f32>, ddy: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad(texture_2d<f32> t, sampler s, vec2f coords, vec2f ddx, vec2f ddy) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, ddx: vec2<f32>, ddy: vec2<f32>, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad(texture_2d<f32> t, sampler s, vec2f coords, vec2f ddx, vec2f ddy, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, ddx: vec2<f32>, ddy: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, vec2f ddx, vec2f ddy) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, ddx: vec2<f32>, ddy: vec2<f32>, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, vec2f ddx, vec2f ddy, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: T, s: sampler, coords: vec3<f32>, ddx: vec3<f32>, ddy: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad(i32 t, sampler s, vec3f coords, vec3f ddx, vec3f ddy) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: T, s: sampler, coords: vec3<f32>, ddx: vec3<f32>, ddy: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad(u32 t, sampler s, vec3f coords, vec3f ddx, vec3f ddy) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: T, s: sampler, coords: vec3<f32>, ddx: vec3<f32>, ddy: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad(f32 t, sampler s, vec3f coords, vec3f ddx, vec3f ddy) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, ddx: vec3<f32>, ddy: vec3<f32>, offset: vec3<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad(texture_3d<f32> t, sampler s, vec3f coords, vec3f ddx, vec3f ddy, vec3i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleGrad(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A, ddx: vec3<f32>, ddy: vec3<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleGrad<A>(texture_cube_array<f32> t, sampler s, vec3f coords, A array_index, vec3f ddx, vec3f ddy) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, level: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel(texture_2d<f32> t, sampler s, vec2f coords, f32 level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, level: f32, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel(texture_2d<f32> t, sampler s, vec2f coords, f32 level, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, level: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, f32 level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, level: f32, offset: vec2<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel<A>(texture_2d_array<f32> t, sampler s, vec2f coords, A array_index, f32 level, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: T, s: sampler, coords: vec3<f32>, level: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel(i32 t, sampler s, vec3f coords, f32 level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: T, s: sampler, coords: vec3<f32>, level: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel(u32 t, sampler s, vec3f coords, f32 level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: T, s: sampler, coords: vec3<f32>, level: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel(f32 t, sampler s, vec3f coords, f32 level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, level: f32, offset: vec3<i32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel(texture_3d<f32> t, sampler s, vec3f coords, f32 level, vec3i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A, level: f32) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleLevel<A>(texture_cube_array<f32> t, sampler s, vec3f coords, A array_index, f32 level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_depth_2d, s: sampler, coords: vec2<f32>, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleLevel<L>(texture_depth_2d t, sampler s, vec2f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_depth_2d, s: sampler, coords: vec2<f32>, level: L, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleLevel<L>(texture_depth_2d t, sampler s, vec2f coords, L level, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleLevel<A, L>(texture_depth_2d_array t, sampler s, vec2f coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, level: L, offset: vec2<i32>) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleLevel<A, L>(texture_depth_2d_array t, sampler s, vec2f coords, A array_index, L level, vec2i offset) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_depth_cube, s: sampler, coords: vec3<f32>, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleLevel<L>(texture_depth_cube t, sampler s, vec3f coords, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleLevel(t: texture_depth_cube_array, s: sampler, coords: vec3<f32>, array_index: A, level: L) -> f32]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static f32 textureSampleLevel<A, L>(texture_depth_cube_array t, sampler s, vec3f coords, A array_index, L level) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBaseClampToEdge(t: T, s: sampler, coords: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBaseClampi32oEdge(i32 t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBaseClampToEdge(t: T, s: sampler, coords: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBaseClampu32oEdge(u32 t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn textureSampleBaseClampToEdge(t: T, s: sampler, coords: vec2<f32>) -> vec4<f32>]]></OriginalSignature>
    /// <Parameterization><![CDATA[<th>Overload ]]></Parameterization>
    public static vec4f textureSampleBaseClampf32oEdge(f32 t, sampler s, vec2f coords) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicAdd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicAdd<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicAdd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicAdd<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicAdd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicAdd<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicSub(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicSub<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicSub(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicSub<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicSub(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicSub<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicMax(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicMax<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicMax(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicMax<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicMax(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicMax<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicMin(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicMin<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicMin(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicMin<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicMin(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicMin<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicAnd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicAnd<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicAnd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicAnd<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicAnd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicAnd<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicOr(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicOr<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicOr(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicOr<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicOr(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicOr<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicXor(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicXor<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicXor(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicXor<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicXor(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicXor<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicExchange(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static i32 atomicExchange<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicExchange(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static u32 atomicExchange<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicExchange(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T]]></OriginalSignature>
    public static f32 atomicExchange<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 v) => default!;

    public partial struct _atomic_compare_exchange_result<T>
    {
        public T old_value;
        public bool success;
    }
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicCompareExchangeWeak(atomic_ptr: ptr<AS, atomic<T>, read_write>, cmp : T, v : T) -> _atomic_compare_exchange_result<T>]]></OriginalSignature>
    public static _atomic_compare_exchange_result<i32> atomicCompareExchangeWeak<AX>(ptr<AX, atomic<i32>, read_write> atomic_ptr, i32 cmp, i32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicCompareExchangeWeak(atomic_ptr: ptr<AS, atomic<T>, read_write>, cmp : T, v : T) -> _atomic_compare_exchange_result<T>]]></OriginalSignature>
    public static _atomic_compare_exchange_result<u32> atomicCompareExchangeWeak<AX>(ptr<AX, atomic<u32>, read_write> atomic_ptr, u32 cmp, u32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn atomicCompareExchangeWeak(atomic_ptr: ptr<AS, atomic<T>, read_write>, cmp : T, v : T) -> _atomic_compare_exchange_result<T>]]></OriginalSignature>
    public static _atomic_compare_exchange_result<f32> atomicCompareExchangeWeak<AX>(ptr<AX, atomic<f32>, read_write> atomic_ptr, f32 cmp, f32 v) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit signed integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit twos complement integer value &lfloor; 0.5 + 127 &times; min(1, max(-1, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack4x8snorm(e: vec4<f32>) -> u32]]></OriginalSignature>
    public static u32 pack4x8snorm(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Converts four normalized floating point values to 8-bit unsigned integers, and then combines them into one `u32` value. Component `e[i]` of the input is converted to an 8-bit unsigned integer value &lfloor; 0.5 + 255 &times; min(1, max(0, e[i])) &rfloor; which is then placed in bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack4x8unorm(e: vec4<f32>) -> u32]]></OriginalSignature>
    public static u32 pack4x8unorm(vec4f e) => default!;
    
    /// <summary>
    /// <![CDATA[Pack the lower 8 bits of each component of `e` into a [=u32=] value and drop all the unused bits. Component `e[i]` of the input is mapped to bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack4xI8(e: vec4<i32>) -> u32]]></OriginalSignature>
    public static u32 pack4xI8(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[Pack the lower 8 bits of each component of `e` into a [=u32=] value and drop all the unused bits. Component `e[i]` of the input is mapped to bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack4xU8(e: vec4<u32>) -> u32]]></OriginalSignature>
    public static u32 pack4xU8(vec4u e) => default!;
    
    /// <summary>
    /// <![CDATA[Clamp each component of `e` in the range [-128, 127] and then pack the lower 8 bits of each component into a [=u32=] value. Component `e[i]` of the input is mapped to bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack4xI8Clamp(e: vec4<i32>) -> u32]]></OriginalSignature>
    public static u32 pack4xI8Clamp(vec4i e) => default!;
    
    /// <summary>
    /// <![CDATA[Clamp each component of `e` in the range of [0, 255] and then pack the lower 8 bits of each component into a [=u32=] value. Component `e[i]` of the input is mapped to bits 8 &times; `i` through 8 &times; `i` + 7 of the result. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack4xU8Clamp(e: vec4<u32>) -> u32]]></OriginalSignature>
    public static u32 pack4xU8Clamp(vec4u e) => default!;
    
    /// <summary>
    /// 
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack2x16snorm(e: vec2<f32>) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=plain type=] with a [=fixed footprint=] that does not contain any [=atomic types=] ]]></Parameterization>
    public static u32 pack2x16snorm(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value pointed to by `p` to all invocations in the workgroup. The return value is [=uniform value|uniform=]. `p` [=shader-creation error|must=] be a [=uniform value=]. Executes a [=control barrier=] synchronization function that affects memory and atomic operations in the [=address spaces/workgroup=] address space. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack2x16unorm(e: vec2<f32>) -> u32]]></OriginalSignature>
    /// <Parameterization><![CDATA[`T` is a [=type/concrete=] [=plain type=] with a [=fixed footprint=] that does not contain any [=atomic types=] ]]></Parameterization>
    public static u32 pack2x16unorm(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn pack2x16float(e: vec2<f32>) -> u32]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 pack2x16float(vec2f e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack4x8snorm(e: u32) -> vec4<f32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec4f unpack4x8snorm(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack4x8unorm(e: u32) -> vec4<f32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec4f unpack4x8unorm(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack4xI8(e: u32) -> vec4<i32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec4i unpack4xI8(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack4xU8(e: u32) -> vec4<u32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec4u unpack4xU8(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack2x16snorm(e: u32) -> vec2<f32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec2f unpack2x16snorm(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack2x16unorm(e: u32) -> vec2<f32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec2f unpack2x16unorm(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn unpack2x16float(e: u32) -> vec2<f32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static vec2f unpack2x16float(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn workgroupUniformLoad(p : ptr<workgroup, T>) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 workgroupUniformLoad(ptr<workgroup, i32> p) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn workgroupUniformLoad(p : ptr<workgroup, T>) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 workgroupUniformLoad(ptr<workgroup, u32> p) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn workgroupUniformLoad(p : ptr<workgroup, T>) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 workgroupUniformLoad(ptr<workgroup, f32> p) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupAdd(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupAdd(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupAdd(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Exclusive prefix scan operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than the current invocation's id. The value returned for the invocation with the lowest id among active invocations is `T(0)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupExclusiveAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupExclusiveAdd(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Exclusive prefix scan operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than the current invocation's id. The value returned for the invocation with the lowest id among active invocations is `T(0)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupExclusiveAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupExclusiveAdd(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Exclusive prefix scan operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than the current invocation's id. The value returned for the invocation with the lowest id among active invocations is `T(0)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupExclusiveAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupExclusiveAdd(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Inclusive prefix scan operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than or equal to the current invocation's id. Note: equivalent to `subgroupInclusiveAdd(x) + x`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupInclusiveAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupInclusiveAdd(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Inclusive prefix scan operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than or equal to the current invocation's id. Note: equivalent to `subgroupInclusiveAdd(x) + x`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupInclusiveAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupInclusiveAdd(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Inclusive prefix scan operation.<br> Returns the sum of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than or equal to the current invocation's id. Note: equivalent to `subgroupInclusiveAdd(x) + x`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupInclusiveAdd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupInclusiveAdd(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise and (`&`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAll(e : bool) -> bool]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static bool subgroupAll(bool e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise and (`&`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAnd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static i32 subgroupAnd(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise and (`&`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAnd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static u32 subgroupAnd(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise and (`&`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAnd(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static f32 subgroupAnd(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=subgroup invocation ID=] matches `id` in the subgroup to all [=subgroups/active=] invocations in the [=subgroup=]. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 128). It is a [=dynamic error=] if `id` does not select an [=subgroups/active=] invocation. Note: If a non-constant version of `id` is required, use [[#subgroupshuffle-builtin|subgroupShuffle]] instead. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupAny(e : bool) -> bool]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static bool subgroupAny(bool e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=subgroup invocation ID=] matches `id` in the subgroup to all [=subgroups/active=] invocations in the [=subgroup=]. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 128). It is a [=dynamic error=] if `id` does not select an [=subgroups/active=] invocation. Note: If a non-constant version of `id` is required, use [[#subgroupshuffle-builtin|subgroupShuffle]] instead. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBallot(pred : bool) -> vec4<u32>]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static vec4u subgroupBallot(bool pred) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=subgroup invocation ID=] matches `id` in the subgroup to all [=subgroups/active=] invocations in the [=subgroup=]. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 128). It is a [=dynamic error=] if `id` does not select an [=subgroups/active=] invocation. Note: If a non-constant version of `id` is required, use [[#subgroupshuffle-builtin|subgroupShuffle]] instead. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBroadcast(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static i32 subgroupBroadcast<I>(i32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=subgroup invocation ID=] matches `id` in the subgroup to all [=subgroups/active=] invocations in the [=subgroup=]. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 128). It is a [=dynamic error=] if `id` does not select an [=subgroups/active=] invocation. Note: If a non-constant version of `id` is required, use [[#subgroupshuffle-builtin|subgroupShuffle]] instead. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBroadcast(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static u32 subgroupBroadcast<I>(u32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=subgroup invocation ID=] matches `id` in the subgroup to all [=subgroups/active=] invocations in the [=subgroup=]. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 128). It is a [=dynamic error=] if `id` does not select an [=subgroups/active=] invocation. Note: If a non-constant version of `id` is required, use [[#subgroupshuffle-builtin|subgroupShuffle]] instead. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBroadcast(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static f32 subgroupBroadcast<I>(f32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation that has the lowest [=subgroup invocation ID=] among [=subgroups/active=] invocations in the [=subgroup=] to all active invocations in the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBroadcastFirst(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupBroadcastFirst(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation that has the lowest [=subgroup invocation ID=] among [=subgroups/active=] invocations in the [=subgroup=] to all active invocations in the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBroadcastFirst(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupBroadcastFirst(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation that has the lowest [=subgroup invocation ID=] among [=subgroups/active=] invocations in the [=subgroup=] to all active invocations in the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupBroadcastFirst(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupBroadcastFirst(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the maximum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupElect() -> bool]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static bool subgroupElect() => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the maximum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMax(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupMax(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the maximum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMax(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupMax(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the maximum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMax(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupMax(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the minimum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMin(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupMin(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the minimum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMin(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupMin(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the minimum value of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMin(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupMin(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupMul(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupMul(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupMul(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Exclusive prefix scan operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than the current invocation's id. The value returned for the invocation with the lowest id among active invocations is `T(1)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupExclusiveMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupExclusiveMul(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Exclusive prefix scan operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than the current invocation's id. The value returned for the invocation with the lowest id among active invocations is `T(1)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupExclusiveMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupExclusiveMul(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Exclusive prefix scan operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than the current invocation's id. The value returned for the invocation with the lowest id among active invocations is `T(1)`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupExclusiveMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupExclusiveMul(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Inclusive prefix scan operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than or equal to the current invocation's id. Note: equivalent to `subgroupExclusiveMul(x) * x`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupInclusiveMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupInclusiveMul(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Inclusive prefix scan operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than or equal to the current invocation's id. Note: equivalent to `subgroupExclusiveMul(x) * x`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupInclusiveMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupInclusiveMul(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Inclusive prefix scan operation.<br> Returns the product of `e` among all [=subgroups/active=] invocations in the [=subgroup=] whose [=subgroup invocation IDs=] are less than or equal to the current invocation's id. Note: equivalent to `subgroupExclusiveMul(x) * x`. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupInclusiveMul(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupInclusiveMul(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise or (`|`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupOr(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static i32 subgroupOr(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise or (`|`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupOr(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static u32 subgroupOr(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise or (`|`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupOr(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static f32 subgroupOr(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `id`. If `id` is outside the range [0, 128), then: * It is a [=shader-creation error=] if `id` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `id` is an [=override-expression=]. An [=indeterminate value=] is returned if `id` does not select an [=subgroups/active=] invocation. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffle(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static i32 subgroupShuffle<I>(i32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `id`. If `id` is outside the range [0, 128), then: * It is a [=shader-creation error=] if `id` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `id` is an [=override-expression=]. An [=indeterminate value=] is returned if `id` does not select an [=subgroups/active=] invocation. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffle(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static u32 subgroupShuffle<I>(u32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `id`. If `id` is outside the range [0, 128), then: * It is a [=shader-creation error=] if `id` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `id` is an [=override-expression=]. An [=indeterminate value=] is returned if `id` does not select an [=subgroups/active=] invocation. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffle(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static f32 subgroupShuffle<I>(f32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id + delta` for the current invocation. If `delta` is greater than 127, then: * It is a [=shader-creation error=] if `delta` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `delta` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `delta` is not a [=uniform value=]. An [=indeterminate value=] is returned if `subgroup_invocation_id + delta` does not select an [=subgroups/active=] invocation or if `delta` is a not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleDown(e : T, delta : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupShuffleDown(i32 e, u32 delta) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id + delta` for the current invocation. If `delta` is greater than 127, then: * It is a [=shader-creation error=] if `delta` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `delta` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `delta` is not a [=uniform value=]. An [=indeterminate value=] is returned if `subgroup_invocation_id + delta` does not select an [=subgroups/active=] invocation or if `delta` is a not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleDown(e : T, delta : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupShuffleDown(u32 e, u32 delta) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id + delta` for the current invocation. If `delta` is greater than 127, then: * It is a [=shader-creation error=] if `delta` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `delta` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `delta` is not a [=uniform value=]. An [=indeterminate value=] is returned if `subgroup_invocation_id + delta` does not select an [=subgroups/active=] invocation or if `delta` is a not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleDown(e : T, delta : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupShuffleDown(f32 e, u32 delta) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id - delta` for the current invocation. If `delta` is greater than 127, then: * It is a [=shader-creation error=] if `delta` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `delta` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `delta` is not a [=uniform value=]. An [=indeterminate value=] is returned if `subgroup_invocation_id - delta` does not select an [=subgroups/active=] invocation or if `delta` is not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleUp(e : T, delta : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupShuffleUp(i32 e, u32 delta) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id - delta` for the current invocation. If `delta` is greater than 127, then: * It is a [=shader-creation error=] if `delta` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `delta` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `delta` is not a [=uniform value=]. An [=indeterminate value=] is returned if `subgroup_invocation_id - delta` does not select an [=subgroups/active=] invocation or if `delta` is not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleUp(e : T, delta : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupShuffleUp(u32 e, u32 delta) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id - delta` for the current invocation. If `delta` is greater than 127, then: * It is a [=shader-creation error=] if `delta` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `delta` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `delta` is not a [=uniform value=]. An [=indeterminate value=] is returned if `subgroup_invocation_id - delta` does not select an [=subgroups/active=] invocation or if `delta` is not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleUp(e : T, delta : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupShuffleUp(f32 e, u32 delta) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id ^ mask` for the current invocation. If `mask` is greater than 127, then: * It is a [=shader-creation error=] if `mask` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `mask` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `mask` is not a [=uniform value=]. An [=indeterminate value=] is returned if `mask` does not select an [=subgroups/active=] invocation or if `mask` is not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleXor(e : T, mask : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 subgroupShuffleXor(i32 e, u32 mask) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id ^ mask` for the current invocation. If `mask` is greater than 127, then: * It is a [=shader-creation error=] if `mask` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `mask` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `mask` is not a [=uniform value=]. An [=indeterminate value=] is returned if `mask` does not select an [=subgroups/active=] invocation or if `mask` is not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleXor(e : T, mask : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 subgroupShuffleXor(u32 e, u32 mask) => default!;
    
    /// <summary>
    /// <![CDATA[Returns `e` from the invocation whose [=subgroup invocation ID=] matches `subgroup_invocation_id ^ mask` for the current invocation. If `mask` is greater than 127, then: * It is a [=shader-creation error=] if `mask` is a [=const-expression=]. * It is a [=pipeline-creation error=] if `mask` is an [=override-expression=]. A [=trigger/subgroup_uniformity=] [=diagnostic=] is [=triggered=] if `mask` is not a [=uniform value=]. An [=indeterminate value=] is returned if `mask` does not select an [=subgroups/active=] invocation or if `mask` is not a uniform value within the subgroup. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupShuffleXor(e : T, mask : u32) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 subgroupShuffleXor(f32 e, u32 mask) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise xor (`^`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupXor(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static i32 subgroupXor(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise xor (`^`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupXor(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static u32 subgroupXor(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Reduction operation.<br> Returns the bitwise xor (`^`) of `e` among all [=subgroups/active=] invocations in the [=subgroup=]. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn subgroupXor(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [INTEGRAL] ]]></Preconditions>
    public static f32 subgroupXor(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=quad invocation ID=] matches `id` in the quad to all [=subgroups/active=] invocations in the quad. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 4). An [=indeterminate value=] is returned if `id` does not select an [=subgroups/active=] invocation. Note: Unlike [[#subgroupbroadcast-builtin|subgroupBroadcast]], there is currently no non-constant alternative. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadBroadcast(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static i32 quadBroadcast<I>(i32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=quad invocation ID=] matches `id` in the quad to all [=subgroups/active=] invocations in the quad. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 4). An [=indeterminate value=] is returned if `id` does not select an [=subgroups/active=] invocation. Note: Unlike [[#subgroupbroadcast-builtin|subgroupBroadcast]], there is currently no non-constant alternative. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadBroadcast(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static u32 quadBroadcast<I>(u32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation whose [=quad invocation ID=] matches `id` in the quad to all [=subgroups/active=] invocations in the quad. `id` [=shader-creation error|must=] be a [=const-expression=] in the range [0, 4). An [=indeterminate value=] is returned if `id` does not select an [=subgroups/active=] invocation. Note: Unlike [[#subgroupbroadcast-builtin|subgroupBroadcast]], there is currently no non-constant alternative. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadBroadcast(e : T, id : I) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=]<br> `I` is [=u32=] or [=i32=] ]]></Preconditions>
    public static f32 quadBroadcast<I>(f32 e, I id) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation in the [=quad=] with the opposite coordinates. That is: * IDs 0 and 3 swap. * IDs 1 and 2 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapDiagonal(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 quadSwapDiagonal(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation in the [=quad=] with the opposite coordinates. That is: * IDs 0 and 3 swap. * IDs 1 and 2 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapDiagonal(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 quadSwapDiagonal(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from the invocation in the [=quad=] with the opposite coordinates. That is: * IDs 0 and 3 swap. * IDs 1 and 2 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapDiagonal(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 quadSwapDiagonal(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from invocation in the [=quad=] sharing the same X dimension. That is: * IDs 0 and 1 swap. * IDs 2 and 3 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapX(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 quadSwapX(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from invocation in the [=quad=] sharing the same X dimension. That is: * IDs 0 and 1 swap. * IDs 2 and 3 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapX(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 quadSwapX(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from invocation in the [=quad=] sharing the same X dimension. That is: * IDs 0 and 1 swap. * IDs 2 and 3 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapX(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 quadSwapX(f32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from invocation in the [=quad=] sharing the same Y dimension. That is: * IDs 0 and 2 swap. * IDs 1 and 3 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapY(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static i32 quadSwapY(i32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from invocation in the [=quad=] sharing the same Y dimension. That is: * IDs 0 and 2 swap. * IDs 1 and 3 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapY(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static u32 quadSwapY(u32 e) => default!;
    
    /// <summary>
    /// <![CDATA[Returns the value of `e` from invocation in the [=quad=] sharing the same Y dimension. That is: * IDs 0 and 2 swap. * IDs 1 and 3 swap. ]]>
    /// </summary>
    /// <OriginalSignature><![CDATA[fn quadSwapY(e : T) -> T]]></OriginalSignature>
    /// <Preconditions><![CDATA[`T` is [=type/concrete=] [=numeric scalar=] or [=numeric vector=] ]]></Preconditions>
    public static f32 quadSwapY(f32 e) => default!;

}

