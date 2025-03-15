
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
    // fn T() -> T
    public static T T<T>() => default!;
    // fn f16(e : T) -> f16
    public static f16 f16<T>(T e) => default!;
    // fn f32(e : T) -> f32
    public static f32 f32<T>(T e) => default!;
    // fn i32(e : T) -> i32
    public static i32 i32<T>(T e) => default!;
    // fn mat2x2<T>(e : mat2x2<S>) -> mat2x2<T>
    public static mat2x2f mat2x2<VM0>(VM0 e) => default!;
    // public static mat2x2f mat2x2<VM0>(VM0 e) => default!;
    // public static mat2x2f mat2x2<VM0>(VM0 e) => default!;
    // fn mat2x2(e : mat2x2<S>) -> mat2x2<S>
    public static mat2x2f mat2x2(mat2x2f e) => default!;
    // public static mat2x2f mat2x2(mat2x2f e) => default!;
    // public static mat2x2f mat2x2(mat2x2f e) => default!;
    // fn mat2x2<T>(v1 : vec2<T>, v2 : vec2<T>) -> mat2x2<T>
    public static mat2x2f mat2x2<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x2f mat2x2<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x2f mat2x2<VM0>(VM0 v1, VM0 v2) => default!;
    // fn mat2x2(v1 : vec2<T>, v2 : vec2<T>) -> mat2x2<T>
    // public static mat2x2f mat2x2<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x2f mat2x2<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x2f mat2x2<VM0>(VM0 v1, VM0 v2) => default!;
    // fn mat2x2<T>(e1 : T, e2 : T, e3 : T, e4 : T) -> mat2x2<T>
    public static mat2x2f mat2x2(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    public static mat2x2f mat2x2(i32 e1, i32 e2, i32 e3, i32 e4) => default!;
    public static mat2x2f mat2x2(u32 e1, u32 e2, u32 e3, u32 e4) => default!;
    // fn mat2x2(e1 : T, e2 : T, e3 : T, e4 : T) -> mat2x2<T>
    // public static mat2x2f mat2x2(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    // public static mat2x2f mat2x2(i32 e1, i32 e2, i32 e3, i32 e4) => default!;
    // public static mat2x2f mat2x2(u32 e1, u32 e2, u32 e3, u32 e4) => default!;
    // fn mat2x3<T>(e : mat2x3<S>) -> mat2x3<T>
    public static mat2x3f mat2x3<VM0>(VM0 e) => default!;
    // public static mat2x3f mat2x3<VM0>(VM0 e) => default!;
    // public static mat2x3f mat2x3<VM0>(VM0 e) => default!;
    // fn mat2x3(e : mat2x3<S>) -> mat2x3<S>
    public static mat2x3f mat2x3(mat2x3f e) => default!;
    // public static mat2x3f mat2x3(mat2x3f e) => default!;
    // public static mat2x3f mat2x3(mat2x3f e) => default!;
    // fn mat2x3<T>(v1 : vec3<T>, v2 : vec3<T>) -> mat2x3<T>
    public static mat2x3f mat2x3<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x3f mat2x3<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x3f mat2x3<VM0>(VM0 v1, VM0 v2) => default!;
    // fn mat2x3(v1 : vec3<T>, v2 : vec3<T>) -> mat2x3<T>
    // public static mat2x3f mat2x3<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x3f mat2x3<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x3f mat2x3<VM0>(VM0 v1, VM0 v2) => default!;
    // fn mat2x3<T>(e1 : T, ..., e6 : T) -> mat2x3<T>
    public static mat2x3f mat2x3(f32 e) => default!;
    public static mat2x3f mat2x3(i32 e) => default!;
    public static mat2x3f mat2x3(u32 e) => default!;
    // fn mat2x3(e1 : T, ..., e6 : T) -> mat2x3<T>
    // public static mat2x3f mat2x3(f32 e) => default!;
    // public static mat2x3f mat2x3(i32 e) => default!;
    // public static mat2x3f mat2x3(u32 e) => default!;
    // fn mat2x4<T>(e : mat2x4<S>) -> mat2x4<T>
    public static mat2x4f mat2x4<VM0>(VM0 e) => default!;
    // public static mat2x4f mat2x4<VM0>(VM0 e) => default!;
    // public static mat2x4f mat2x4<VM0>(VM0 e) => default!;
    // fn mat2x4(e : mat2x4<S>) -> mat2x4<S>
    public static mat2x4f mat2x4(mat2x4f e) => default!;
    // public static mat2x4f mat2x4(mat2x4f e) => default!;
    // public static mat2x4f mat2x4(mat2x4f e) => default!;
    // fn mat2x4<T>(v1 : vec4<T>, v2 : vec4<T>) -> mat2x4<T>
    public static mat2x4f mat2x4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x4f mat2x4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x4f mat2x4<VM0>(VM0 v1, VM0 v2) => default!;
    // fn mat2x4(v1 : vec4<T>, v2 : vec4<T>) -> mat2x4<T>
    // public static mat2x4f mat2x4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x4f mat2x4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static mat2x4f mat2x4<VM0>(VM0 v1, VM0 v2) => default!;
    // fn mat2x4<T>(e1 : T, ..., e8 : T) -> mat2x4<T>
    public static mat2x4f mat2x4(f32 e) => default!;
    public static mat2x4f mat2x4(i32 e) => default!;
    public static mat2x4f mat2x4(u32 e) => default!;
    // fn mat2x4(e1 : T, ..., e8 : T) -> mat2x4<T>
    // public static mat2x4f mat2x4(f32 e) => default!;
    // public static mat2x4f mat2x4(i32 e) => default!;
    // public static mat2x4f mat2x4(u32 e) => default!;
    // fn mat3x2<T>(e : mat3x2<S>) -> mat3x2<T>
    public static mat3x2f mat3x2<VM0>(VM0 e) => default!;
    // public static mat3x2f mat3x2<VM0>(VM0 e) => default!;
    // public static mat3x2f mat3x2<VM0>(VM0 e) => default!;
    // fn mat3x2(e : mat3x2<S>) -> mat3x2<S>
    public static mat3x2f mat3x2(mat3x2f e) => default!;
    // public static mat3x2f mat3x2(mat3x2f e) => default!;
    // public static mat3x2f mat3x2(mat3x2f e) => default!;
    // fn mat3x2<T>(v1 : vec2<T>, v2 : vec2<T>, v3 : vec2<T>) -> mat3x2<T>
    public static mat3x2f mat3x2<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x2f mat3x2<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x2f mat3x2<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // fn mat3x2(v1 : vec2<T>, v2 : vec2<T>, v3 : vec2<T>) -> mat3x2<T>
    // public static mat3x2f mat3x2<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x2f mat3x2<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x2f mat3x2<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // fn mat3x2<T>(e1 : T, ..., e6 : T) -> mat3x2<T>
    public static mat3x2f mat3x2(f32 e) => default!;
    public static mat3x2f mat3x2(i32 e) => default!;
    public static mat3x2f mat3x2(u32 e) => default!;
    // fn mat3x2(e1 : T, ..., e6 : T) -> mat3x2<T>
    // public static mat3x2f mat3x2(f32 e) => default!;
    // public static mat3x2f mat3x2(i32 e) => default!;
    // public static mat3x2f mat3x2(u32 e) => default!;
    // fn mat3x3<T>(e : mat3x3<S>) -> mat3x3<T>
    public static mat3x3f mat3x3<VM0>(VM0 e) => default!;
    // public static mat3x3f mat3x3<VM0>(VM0 e) => default!;
    // public static mat3x3f mat3x3<VM0>(VM0 e) => default!;
    // fn mat3x3(e : mat3x3<S>) -> mat3x3<S>
    public static mat3x3f mat3x3(mat3x3f e) => default!;
    // public static mat3x3f mat3x3(mat3x3f e) => default!;
    // public static mat3x3f mat3x3(mat3x3f e) => default!;
    // fn mat3x3<T>(v1 : vec3<T>, v2 : vec3<T>, v3 : vec3<T>) -> mat3x3<T>
    public static mat3x3f mat3x3<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x3f mat3x3<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x3f mat3x3<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // fn mat3x3(v1 : vec3<T>, v2 : vec3<T>, v3 : vec3<T>) -> mat3x3<T>
    // public static mat3x3f mat3x3<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x3f mat3x3<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x3f mat3x3<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // fn mat3x3<T>(e1 : T, ..., e9 : T) -> mat3x3<T>
    public static mat3x3f mat3x3(f32 e) => default!;
    public static mat3x3f mat3x3(i32 e) => default!;
    public static mat3x3f mat3x3(u32 e) => default!;
    // fn mat3x3(e1 : T, ..., e9 : T) -> mat3x3<T>
    // public static mat3x3f mat3x3(f32 e) => default!;
    // public static mat3x3f mat3x3(i32 e) => default!;
    // public static mat3x3f mat3x3(u32 e) => default!;
    // fn mat3x4<T>(e : mat3x4<S>) -> mat3x4<T>
    public static mat3x4f mat3x4<VM0>(VM0 e) => default!;
    // public static mat3x4f mat3x4<VM0>(VM0 e) => default!;
    // public static mat3x4f mat3x4<VM0>(VM0 e) => default!;
    // fn mat3x4(e : mat3x4<S>) -> mat3x4<S>
    public static mat3x4f mat3x4(mat3x4f e) => default!;
    // public static mat3x4f mat3x4(mat3x4f e) => default!;
    // public static mat3x4f mat3x4(mat3x4f e) => default!;
    // fn mat3x4<T>(v1 : vec4<T>, v2 : vec4<T>, v3 : vec4<T>) -> mat3x4<T>
    public static mat3x4f mat3x4<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x4f mat3x4<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x4f mat3x4<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // fn mat3x4(v1 : vec4<T>, v2 : vec4<T>, v3 : vec4<T>) -> mat3x4<T>
    // public static mat3x4f mat3x4<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x4f mat3x4<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // public static mat3x4f mat3x4<VM0>(VM0 v1, VM0 v2, VM0 v3) => default!;
    // fn mat3x4<T>(e1 : T, ..., e12 : T) -> mat3x4<T>
    public static mat3x4f mat3x4(f32 e) => default!;
    public static mat3x4f mat3x4(i32 e) => default!;
    public static mat3x4f mat3x4(u32 e) => default!;
    // fn mat3x4(e1 : T, ..., e12 : T) -> mat3x4<T>
    // public static mat3x4f mat3x4(f32 e) => default!;
    // public static mat3x4f mat3x4(i32 e) => default!;
    // public static mat3x4f mat3x4(u32 e) => default!;
    // fn mat4x2<T>(e : mat4x2<S>) -> mat4x2<T>
    public static mat4x2f mat4x2<VM0>(VM0 e) => default!;
    // public static mat4x2f mat4x2<VM0>(VM0 e) => default!;
    // public static mat4x2f mat4x2<VM0>(VM0 e) => default!;
    // fn mat4x2(e : mat4x2<S>) -> mat4x2<S>
    public static mat4x2f mat4x2(mat4x2f e) => default!;
    // public static mat4x2f mat4x2(mat4x2f e) => default!;
    // public static mat4x2f mat4x2(mat4x2f e) => default!;
    // fn mat4x2<T>(v1 : vec2<T>, v2 : vec2<T>, v3 : vec2<T>, v4: vec2<T>) -> mat4x2<T>
    public static mat4x2f mat4x2<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x2f mat4x2<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x2f mat4x2<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // fn mat4x2(v1 : vec2<T>, v2 : vec2<T>, v3 : vec2<T>, v4: vec2<T>) -> mat4x2<T>
    // public static mat4x2f mat4x2<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x2f mat4x2<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x2f mat4x2<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // fn mat4x2<T>(e1 : T, ..., e8 : T) -> mat4x2<T>
    public static mat4x2f mat4x2(f32 e) => default!;
    public static mat4x2f mat4x2(i32 e) => default!;
    public static mat4x2f mat4x2(u32 e) => default!;
    // fn mat4x2(e1 : T, ..., e8 : T) -> mat4x2<T>
    // public static mat4x2f mat4x2(f32 e) => default!;
    // public static mat4x2f mat4x2(i32 e) => default!;
    // public static mat4x2f mat4x2(u32 e) => default!;
    // fn mat4x3<T>(e : mat4x3<S>) -> mat4x3<T>
    public static mat4x3f mat4x3<VM0>(VM0 e) => default!;
    // public static mat4x3f mat4x3<VM0>(VM0 e) => default!;
    // public static mat4x3f mat4x3<VM0>(VM0 e) => default!;
    // fn mat4x3(e : mat4x3<S>) -> mat4x3<S>
    public static mat4x3f mat4x3(mat4x3f e) => default!;
    // public static mat4x3f mat4x3(mat4x3f e) => default!;
    // public static mat4x3f mat4x3(mat4x3f e) => default!;
    // fn mat4x3<T>(v1 : vec3<T>, v2 : vec3<T>, v3 : vec3<T>, v4 : vec3<T>) -> mat4x3<T>
    public static mat4x3f mat4x3<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x3f mat4x3<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x3f mat4x3<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // fn mat4x3(v1 : vec3<T>, v2 : vec3<T>, v3 : vec3<T>, v4 : vec3<T>) -> mat4x3<T>
    // public static mat4x3f mat4x3<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x3f mat4x3<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x3f mat4x3<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // fn mat4x3<T>(e1 : T, ..., e12 : T) -> mat4x3<T>
    public static mat4x3f mat4x3(f32 e) => default!;
    public static mat4x3f mat4x3(i32 e) => default!;
    public static mat4x3f mat4x3(u32 e) => default!;
    // fn mat4x3(e1 : T, ..., e12 : T) -> mat4x3<T>
    // public static mat4x3f mat4x3(f32 e) => default!;
    // public static mat4x3f mat4x3(i32 e) => default!;
    // public static mat4x3f mat4x3(u32 e) => default!;
    // fn mat4x4<T>(e : mat4x4<S>) -> mat4x4<T>
    public static mat4x4f mat4x4<VM0>(VM0 e) => default!;
    // public static mat4x4f mat4x4<VM0>(VM0 e) => default!;
    // public static mat4x4f mat4x4<VM0>(VM0 e) => default!;
    // fn mat4x4(e : mat4x4<S>) -> mat4x4<S>
    public static mat4x4f mat4x4(mat4x4f e) => default!;
    // public static mat4x4f mat4x4(mat4x4f e) => default!;
    // public static mat4x4f mat4x4(mat4x4f e) => default!;
    // fn mat4x4<T>(v1 : vec4<T>, v2 : vec4<T>, v3 : vec4<T>, v4 : vec4<T>) -> mat4x4<T>
    public static mat4x4f mat4x4<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x4f mat4x4<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x4f mat4x4<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // fn mat4x4(v1 : vec4<T>, v2 : vec4<T>, v3 : vec4<T>, v4 : vec4<T>) -> mat4x4<T>
    // public static mat4x4f mat4x4<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x4f mat4x4<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // public static mat4x4f mat4x4<VM0>(VM0 v1, VM0 v2, VM0 v3, VM0 v4) => default!;
    // fn mat4x4<T>(e1 : T, ..., e16 : T) -> mat4x4<T>
    public static mat4x4f mat4x4(f32 e) => default!;
    public static mat4x4f mat4x4(i32 e) => default!;
    public static mat4x4f mat4x4(u32 e) => default!;
    // fn mat4x4(e1 : T, ..., e16 : T) -> mat4x4<T>
    // public static mat4x4f mat4x4(f32 e) => default!;
    // public static mat4x4f mat4x4(i32 e) => default!;
    // public static mat4x4f mat4x4(u32 e) => default!;
    // fn S(e1 : T1, ..., eN : TN) -> S
    public static S S<T1, S>(T1 e) => default!;
    // fn u32(e : T) -> u32
    public static u32 u32<T>(T e) => default!;
    // fn vec2<T>(e : T) -> vec2<T>
    public static vec2f vec2(f32 e) => default!;
    public static vec2i vec2(i32 e) => default!;
    public static vec2u vec2(u32 e) => default!;
    // fn vec2(e : S) -> vec2<S>
    // public static vec2f vec2(f32 e) => default!;
    // public static vec2i vec2(i32 e) => default!;
    // public static vec2u vec2(u32 e) => default!;
    // fn vec2<T>(e : vec2<S>) -> vec2<T>
    // public static vec2f vec2<VM0>(VM0 e) => default!;
    // public static vec2i vec2<VM0>(VM0 e) => default!;
    public static vec2u vec2<VM0>(VM0 e) => default!;
    // fn vec2(e : vec2<S>) -> vec2<S>
    public static vec2f vec2(vec2f e) => default!;
    public static vec2i vec2(vec2i e) => default!;
    public static vec2u vec2(vec2u e) => default!;
    // fn vec2<T>(e1 : T, e2 : T) -> vec2<T>
    public static vec2f vec2(f32 e1, f32 e2) => default!;
    public static vec2i vec2(i32 e1, i32 e2) => default!;
    public static vec2u vec2(u32 e1, u32 e2) => default!;
    // fn vec2(e1 : T, e2 : T) -> vec2<T>
    // public static vec2f vec2(f32 e1, f32 e2) => default!;
    // public static vec2i vec2(i32 e1, i32 e2) => default!;
    // public static vec2u vec2(u32 e1, u32 e2) => default!;
    // fn vec2() -> vec2<T>
    // public static vec2f vec2() => default!;
    // public static vec2i vec2() => default!;
    public static vec2u vec2() => default!;
    // fn vec3<T>(e : T) -> vec3<T>
    public static vec3f vec3(f32 e) => default!;
    public static vec3i vec3(i32 e) => default!;
    public static vec3u vec3(u32 e) => default!;
    // fn vec3(e : S) -> vec3<S>
    // public static vec3f vec3(f32 e) => default!;
    // public static vec3i vec3(i32 e) => default!;
    // public static vec3u vec3(u32 e) => default!;
    // fn vec3<T>(e : vec3<S>) -> vec3<T>
    // public static vec3f vec3<VM0>(VM0 e) => default!;
    // public static vec3i vec3<VM0>(VM0 e) => default!;
    public static vec3u vec3<VM0>(VM0 e) => default!;
    // fn vec3(e : vec3<S>) -> vec3<S>
    public static vec3f vec3(vec3f e) => default!;
    public static vec3i vec3(vec3i e) => default!;
    public static vec3u vec3(vec3u e) => default!;
    // fn vec3<T>(e1 : T, e2 : T, e3 : T) -> vec3<T>
    public static vec3f vec3(f32 e1, f32 e2, f32 e3) => default!;
    public static vec3i vec3(i32 e1, i32 e2, i32 e3) => default!;
    public static vec3u vec3(u32 e1, u32 e2, u32 e3) => default!;
    // fn vec3(e1 : T, e2 : T, e3 : T) -> vec3<T>
    // public static vec3f vec3(f32 e1, f32 e2, f32 e3) => default!;
    // public static vec3i vec3(i32 e1, i32 e2, i32 e3) => default!;
    // public static vec3u vec3(u32 e1, u32 e2, u32 e3) => default!;
    // fn vec3<T>(v1 : vec2<T>, e1 : T) -> vec3<T>
    public static vec3f vec3<VM0>(VM0 v1, f32 e1) => default!;
    public static vec3i vec3<VM0>(VM0 v1, i32 e1) => default!;
    public static vec3u vec3<VM0>(VM0 v1, u32 e1) => default!;
    // fn vec3(v1 : vec2<T>, e1 : T) -> vec3<T>
    // public static vec3f vec3<VM0>(VM0 v1, f32 e1) => default!;
    // public static vec3i vec3<VM0>(VM0 v1, i32 e1) => default!;
    // public static vec3u vec3<VM0>(VM0 v1, u32 e1) => default!;
    // fn vec3<T>(e1 : T, v1 : vec2<T>) -> vec3<T>
    public static vec3f vec3<VM1>(f32 e1, VM1 v1) => default!;
    public static vec3i vec3<VM1>(i32 e1, VM1 v1) => default!;
    public static vec3u vec3<VM1>(u32 e1, VM1 v1) => default!;
    // fn vec3(e1 : T, v1 : vec2<T>) -> vec3<T>
    // public static vec3f vec3<VM1>(f32 e1, VM1 v1) => default!;
    // public static vec3i vec3<VM1>(i32 e1, VM1 v1) => default!;
    // public static vec3u vec3<VM1>(u32 e1, VM1 v1) => default!;
    // fn vec3() -> vec3<T>
    // public static vec3f vec3() => default!;
    // public static vec3i vec3() => default!;
    public static vec3u vec3() => default!;
    // fn vec4<T>(e : T) -> vec4<T>
    public static vec4f vec4(f32 e) => default!;
    public static vec4i vec4(i32 e) => default!;
    public static vec4u vec4(u32 e) => default!;
    // fn vec4(e : S) -> vec4<S>
    // public static vec4f vec4(f32 e) => default!;
    // public static vec4i vec4(i32 e) => default!;
    // public static vec4u vec4(u32 e) => default!;
    // fn vec4<T>(e : vec4<S>) -> vec4<T>
    // public static vec4f vec4<VM0>(VM0 e) => default!;
    // public static vec4i vec4<VM0>(VM0 e) => default!;
    public static vec4u vec4<VM0>(VM0 e) => default!;
    // fn vec4(e : vec4<S>) -> vec4<S>
    public static vec4f vec4(vec4f e) => default!;
    public static vec4i vec4(vec4i e) => default!;
    public static vec4u vec4(vec4u e) => default!;
    // fn vec4<T>(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>
    public static vec4f vec4(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    public static vec4i vec4(i32 e1, i32 e2, i32 e3, i32 e4) => default!;
    public static vec4u vec4(u32 e1, u32 e2, u32 e3, u32 e4) => default!;
    // fn vec4(e1 : T, e2 : T, e3 : T, e4 : T) -> vec4<T>
    // public static vec4f vec4(f32 e1, f32 e2, f32 e3, f32 e4) => default!;
    // public static vec4i vec4(i32 e1, i32 e2, i32 e3, i32 e4) => default!;
    // public static vec4u vec4(u32 e1, u32 e2, u32 e3, u32 e4) => default!;
    // fn vec4<T>(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>
    public static vec4f vec4<VM1>(f32 e1, VM1 v1, f32 e2) => default!;
    public static vec4i vec4<VM1>(i32 e1, VM1 v1, i32 e2) => default!;
    public static vec4u vec4<VM1>(u32 e1, VM1 v1, u32 e2) => default!;
    // fn vec4(e1 : T, v1 : vec2<T>, e2 : T) -> vec4<T>
    // public static vec4f vec4<VM1>(f32 e1, VM1 v1, f32 e2) => default!;
    // public static vec4i vec4<VM1>(i32 e1, VM1 v1, i32 e2) => default!;
    // public static vec4u vec4<VM1>(u32 e1, VM1 v1, u32 e2) => default!;
    // fn vec4<T>(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>
    public static vec4f vec4<VM1>(f32 e1, f32 e2, VM1 v1) => default!;
    public static vec4i vec4<VM1>(i32 e1, i32 e2, VM1 v1) => default!;
    public static vec4u vec4<VM1>(u32 e1, u32 e2, VM1 v1) => default!;
    // fn vec4(e1 : T, e2 : T, v1 : vec2<T>) -> vec4<T>
    // public static vec4f vec4<VM1>(f32 e1, f32 e2, VM1 v1) => default!;
    // public static vec4i vec4<VM1>(i32 e1, i32 e2, VM1 v1) => default!;
    // public static vec4u vec4<VM1>(u32 e1, u32 e2, VM1 v1) => default!;
    // fn vec4<T>(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>
    // public static vec4f vec4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static vec4i vec4<VM0>(VM0 v1, VM0 v2) => default!;
    public static vec4u vec4<VM0>(VM0 v1, VM0 v2) => default!;
    // fn vec4(v1 : vec2<T>, v2 : vec2<T>) -> vec4<T>
    // public static vec4f vec4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static vec4i vec4<VM0>(VM0 v1, VM0 v2) => default!;
    // public static vec4u vec4<VM0>(VM0 v1, VM0 v2) => default!;
    // fn vec4<T>(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>
    public static vec4f vec4<VM0>(VM0 v1, f32 e1, f32 e2) => default!;
    public static vec4i vec4<VM0>(VM0 v1, i32 e1, i32 e2) => default!;
    public static vec4u vec4<VM0>(VM0 v1, u32 e1, u32 e2) => default!;
    // fn vec4(v1 : vec2<T>, e1 : T, e2 : T) -> vec4<T>
    // public static vec4f vec4<VM0>(VM0 v1, f32 e1, f32 e2) => default!;
    // public static vec4i vec4<VM0>(VM0 v1, i32 e1, i32 e2) => default!;
    // public static vec4u vec4<VM0>(VM0 v1, u32 e1, u32 e2) => default!;
    // fn vec4<T>(v1 : vec3<T>, e1 : T) -> vec4<T>
    public static vec4f vec4<VM0>(VM0 v1, f32 e1) => default!;
    public static vec4i vec4<VM0>(VM0 v1, i32 e1) => default!;
    public static vec4u vec4<VM0>(VM0 v1, u32 e1) => default!;
    // fn vec4(v1 : vec3<T>, e1 : T) -> vec4<T>
    // public static vec4f vec4<VM0>(VM0 v1, f32 e1) => default!;
    // public static vec4i vec4<VM0>(VM0 v1, i32 e1) => default!;
    // public static vec4u vec4<VM0>(VM0 v1, u32 e1) => default!;
    // fn vec4<T>(e1 : T, v1 : vec3<T>) -> vec4<T>
    public static vec4f vec4<VM1>(f32 e1, VM1 v1) => default!;
    public static vec4i vec4<VM1>(i32 e1, VM1 v1) => default!;
    public static vec4u vec4<VM1>(u32 e1, VM1 v1) => default!;
    // fn vec4(e1 : T, v1 : vec3<T>) -> vec4<T>
    // public static vec4f vec4<VM1>(f32 e1, VM1 v1) => default!;
    // public static vec4i vec4<VM1>(i32 e1, VM1 v1) => default!;
    // public static vec4u vec4<VM1>(u32 e1, VM1 v1) => default!;
    // fn vec4() -> vec4<T>
    // public static vec4f vec4() => default!;
    // public static vec4i vec4() => default!;
    public static vec4u vec4() => default!;
    // fn bitcast<T>(e : T) -> T
    public static T bitcast<T>(T e) => default!;
    // fn bitcast<T>(e : S) -> T
    // public static T bitcast<S, T>(S e) => default!;
    // fn bitcast<u32>(e : AbstractInt) -> u32
    public static u32 bitcast(AbstractInt e) => default!;
    // fn bitcast<T>(e : vec2<f16>) -> T
    public static T bitcast<VM0, T>(VM0 e) => default!;
    // fn all(e: vecN<bool>) -> bool
    public static bool all<VM0>(VM0 e) => default!;
    // fn all(e: bool) -> bool
    public static bool all(bool e) => default!;
    // fn any(e: vecN<bool>) -> bool
    public static bool any<VM0>(VM0 e) => default!;
    // fn any(e: bool) -> bool
    public static bool any(bool e) => default!;
    // fn select(f: T, t: T, cond: bool) -> T
    public static T select<T>(T f, T t, bool cond) => default!;
    // fn select(f: vecN<T>, t: vecN<T>, cond: vecN<bool>) -> vecN<T>
    public static vec2f select<VM0>(vec2f f, vec2f t, VM0 cond) => default!;
    public static vec3f select<VM0>(vec3f f, vec3f t, VM0 cond) => default!;
    public static vec4f select<VM0>(vec4f f, vec4f t, VM0 cond) => default!;
    public static vec2i select<VM0>(vec2i f, vec2i t, VM0 cond) => default!;
    public static vec3i select<VM0>(vec3i f, vec3i t, VM0 cond) => default!;
    public static vec4i select<VM0>(vec4i f, vec4i t, VM0 cond) => default!;
    public static vec2u select<VM0>(vec2u f, vec2u t, VM0 cond) => default!;
    public static vec3u select<VM0>(vec3u f, vec3u t, VM0 cond) => default!;
    public static vec4u select<VM0>(vec4u f, vec4u t, VM0 cond) => default!;
    // fn arrayLength(p: ptr<storage, array<E>, AM>) -> u32
    public static u32 arrayLength(ptr<storage, dynamic, dynamic> p) => default!;
    // fn num_point_lights() -> u32
    public static u32 num_point_lights() => default!;
    // fn abs(e: T ) -> T
    public static T abs<T>(T e) => default!;
    // fn acos(e: T) -> T
    public static T acos<T>(T e) => default!;
    // fn acosh(x: T) -> T
    public static T acosh<T>(T x) => default!;
    // fn asin(e: T) -> T
    public static T asin<T>(T e) => default!;
    // fn asinh(y: T) -> T
    public static T asinh<T>(T y) => default!;
    // fn atan(e: T) -> T
    public static T atan<T>(T e) => default!;
    // fn atanh(t: T) -> T
    public static T atanh<T>(T t) => default!;
    // fn atan2(y: T, x: T) -> T
    public static T atan2<T>(T y, T x) => default!;
    // fn ceil(e: T) -> T
    public static T ceil<T>(T e) => default!;
    // fn clamp(e: T, low: T, high: T) -> T
    public static T clamp<T>(T e, T low, T high) => default!;
    // fn cos(e: T) -> T
    public static T cos<T>(T e) => default!;
    // fn cosh(a: T) -> T
    public static T cosh<T>(T a) => default!;
    // fn countLeadingZeros(e: T) -> T
    public static T countLeadingZeros<T>(T e) => default!;
    // fn countOneBits(e: T) -> T
    public static T countOneBits<T>(T e) => default!;
    // fn countTrailingZeros(e: T) -> T
    public static T countTrailingZeros<T>(T e) => default!;
    // fn cross(a: vec3<T>, b: vec3<T>) -> vec3<T>
    public static vec3f cross(vec3f a, vec3f b) => default!;
    public static vec3i cross(vec3i a, vec3i b) => default!;
    public static vec3u cross(vec3u a, vec3u b) => default!;
    // fn degrees(e1: T) -> T
    public static T degrees<T>(T e1) => default!;
    // fn determinant(e: matCxC<T>) -> T
    public static T determinant<VM0, T>(VM0 e) => default!;
    // fn distance(e1: T, e2: T) -> S
    public static S distance<T, S>(T e1, T e2) => default!;
    // fn dot(e1: vecN<T>, e2: vecN<T>) -> T
    public static T dot<VM0, T>(VM0 e1, VM0 e2) => default!;
    // fn dot4U8Packed(e1: u32, e2: u32) -> u32
    public static u32 dot4U8Packed(u32 e1, u32 e2) => default!;
    // fn dot4I8Packed(e1: u32, e2: u32) -> i32
    public static i32 dot4I8Packed(u32 e1, u32 e2) => default!;
    // fn exp(e1: T) -> T
    public static T exp<T>(T e1) => default!;
    // fn exp2(e: T) -> T
    public static T exp2<T>(T e) => default!;
    // fn extractBits(e: T, offset: u32, count: u32) -> T
    public static T extractBits<T>(T e, u32 offset, u32 count) => default!;
    // // fn extractBits(e: T, offset: u32, count: u32) -> T
    // public static T extractBits<T>(T e, u32 offset, u32 count) => default!;
    // fn faceForward(e1: T, e2: T, e3: T) -> T
    public static T faceForward<T>(T e1, T e2, T e3) => default!;
    // fn firstLeadingBit(e: T) -> T
    public static T firstLeadingBit<T>(T e) => default!;
    // // fn firstLeadingBit(e: T) -> T
    // public static T firstLeadingBit<T>(T e) => default!;
    // fn firstTrailingBit(e: T) -> T
    public static T firstTrailingBit<T>(T e) => default!;
    // fn floor(e: T) -> T
    public static T floor<T>(T e) => default!;
    // fn fma(e1: T, e2: T, e3: T) -> T
    public static T fma<T>(T e1, T e2, T e3) => default!;
    // fn fract(e: T) -> T
    public static T fract<T>(T e) => default!;
    // fn frexp(e: T) -> __frexp_result_f32
    public partial class __frexp_result_f32;
    // public static __frexp_result_f32 frexp<T>(T e) => default!;
    // fn frexp(e: T) -> __frexp_result_f16
    public partial class __frexp_result_f16;
    // public static __frexp_result_f16 frexp<T>(T e) => default!;
    // fn frexp(e: T) -> __frexp_result_abstract
    public partial class __frexp_result_abstract;
    // public static __frexp_result_abstract frexp<T>(T e) => default!;
    // fn frexp(e: T) -> __frexp_result_vecN_f32
    public partial class __frexp_result_vecN_f32;
    // public static __frexp_result_vecN_f32 frexp<T>(T e) => default!;
    // fn frexp(e: T) -> __frexp_result_vecN_f16
    public partial class __frexp_result_vecN_f16;
    // public static __frexp_result_vecN_f16 frexp<T>(T e) => default!;
    // fn frexp(e: T) -> __frexp_result_vecN_abstract
    public partial class __frexp_result_vecN_abstract;
    public static __frexp_result_vecN_abstract frexp<T>(T e) => default!;
    // fn insertBits(e: T, newbits: T, offset: u32, count: u32) -> T
    public static T insertBits<T>(T e, T newbits, u32 offset, u32 count) => default!;
    // fn inverseSqrt(e: T) -> T
    public static T inverseSqrt<T>(T e) => default!;
    // fn ldexp(e1: T, e2: I) -> T
    public static T ldexp<T, I>(T e1, I e2) => default!;
    // fn length(e: T) -> S
    public static S length<T, S>(T e) => default!;
    // fn log(e: T) -> T
    public static T log<T>(T e) => default!;
    // fn log2(e: T) -> T
    public static T log2<T>(T e) => default!;
    // fn max(e1: T, e2: T) -> T
    public static T max<T>(T e1, T e2) => default!;
    // fn min(e1: T, e2: T) -> T
    public static T min<T>(T e1, T e2) => default!;
    // fn mix(e1: T, e2: T, e3: T) -> T
    public static T mix<T>(T e1, T e2, T e3) => default!;
    // fn mix(e1: T2, e2: T2, e3: T) -> T2
    public static T2 mix<T2, T>(T2 e1, T2 e2, T e3) => default!;
    // fn modf(e: T) -> __modf_result_f32
    public partial class __modf_result_f32;
    // public static __modf_result_f32 modf<T>(T e) => default!;
    // fn modf(e: T) -> __modf_result_f16
    public partial class __modf_result_f16;
    // public static __modf_result_f16 modf<T>(T e) => default!;
    // fn modf(e: T) -> __modf_result_abstract
    public partial class __modf_result_abstract;
    // public static __modf_result_abstract modf<T>(T e) => default!;
    // fn modf(e: T) -> __modf_result_vecN_f32
    public partial class __modf_result_vecN_f32;
    // public static __modf_result_vecN_f32 modf<T>(T e) => default!;
    // fn modf(e: T) -> __modf_result_vecN_f16
    public partial class __modf_result_vecN_f16;
    // public static __modf_result_vecN_f16 modf<T>(T e) => default!;
    // fn modf(e: T) -> __modf_result_vecN_abstract
    public partial class __modf_result_vecN_abstract;
    public static __modf_result_vecN_abstract modf<T>(T e) => default!;
    // fn normalize(e: vecN<T> ) -> vecN<T>
    public static vec2f normalize(vec2f e) => default!;
    public static vec3f normalize(vec3f e) => default!;
    public static vec4f normalize(vec4f e) => default!;
    public static vec2i normalize(vec2i e) => default!;
    public static vec3i normalize(vec3i e) => default!;
    public static vec4i normalize(vec4i e) => default!;
    public static vec2u normalize(vec2u e) => default!;
    public static vec3u normalize(vec3u e) => default!;
    public static vec4u normalize(vec4u e) => default!;
    // fn pow(e1: T, e2: T) -> T
    public static T pow<T>(T e1, T e2) => default!;
    // fn quantizeToF16(e: T) -> T
    public static T quantizeToF16<T>(T e) => default!;
    // fn radians(e1: T) -> T
    public static T radians<T>(T e1) => default!;
    // fn reflect(e1: T, e2: T) -> T
    public static T reflect<T>(T e1, T e2) => default!;
    // fn refract(e1: T, e2: T, e3: I) -> T
    public static T refract<T, I>(T e1, T e2, I e3) => default!;
    // fn reverseBits(e: T) -> T
    public static T reverseBits<T>(T e) => default!;
    // fn round(e: T) -> T
    public static T round<T>(T e) => default!;
    // fn saturate(e: T) -> T
    public static T saturate<T>(T e) => default!;
    // fn sign(e: T) -> T
    public static T sign<T>(T e) => default!;
    // fn sin(e: T) -> T
    public static T sin<T>(T e) => default!;
    // fn sinh(a: T) -> T
    public static T sinh<T>(T a) => default!;
    // fn smoothstep(edge0: T, edge1: T, x: T) -> T
    public static T smoothstep<T>(T edge0, T edge1, T x) => default!;
    // fn sqrt(e: T) -> T
    public static T sqrt<T>(T e) => default!;
    // fn step(edge: T, x: T) -> T
    public static T step<T>(T edge, T x) => default!;
    // fn tan(e: T) -> T
    public static T tan<T>(T e) => default!;
    // fn tanh(a: T) -> T
    public static T tanh<T>(T a) => default!;
    // fn transpose(e: matRxC<T>) -> matCxR<T>
    public static mat2x2f transpose(mat2x2f e) => default!;
    public static mat2x3f transpose(mat3x2f e) => default!;
    public static mat2x4f transpose(mat4x2f e) => default!;
    public static mat3x2f transpose(mat2x3f e) => default!;
    public static mat3x3f transpose(mat3x3f e) => default!;
    public static mat3x4f transpose(mat4x3f e) => default!;
    public static mat4x2f transpose(mat2x4f e) => default!;
    public static mat4x3f transpose(mat3x4f e) => default!;
    public static mat4x4f transpose(mat4x4f e) => default!;
    // public static mat2x2f transpose(mat2x2f e) => default!;
    // public static mat2x3f transpose(mat3x2f e) => default!;
    // public static mat2x4f transpose(mat4x2f e) => default!;
    // public static mat3x2f transpose(mat2x3f e) => default!;
    // public static mat3x3f transpose(mat3x3f e) => default!;
    // public static mat3x4f transpose(mat4x3f e) => default!;
    // public static mat4x2f transpose(mat2x4f e) => default!;
    // public static mat4x3f transpose(mat3x4f e) => default!;
    // public static mat4x4f transpose(mat4x4f e) => default!;
    // public static mat2x2f transpose(mat2x2f e) => default!;
    // public static mat2x3f transpose(mat3x2f e) => default!;
    // public static mat2x4f transpose(mat4x2f e) => default!;
    // public static mat3x2f transpose(mat2x3f e) => default!;
    // public static mat3x3f transpose(mat3x3f e) => default!;
    // public static mat3x4f transpose(mat4x3f e) => default!;
    // public static mat4x2f transpose(mat2x4f e) => default!;
    // public static mat4x3f transpose(mat3x4f e) => default!;
    // public static mat4x4f transpose(mat4x4f e) => default!;
    // fn trunc(e: T) -> T
    public static T trunc<T>(T e) => default!;
    // fn dpdx(e: T) -> T
    public static T dpdx<T>(T e) => default!;
    // fn dpdxCoarse(e: T) -> T
    public static T dpdxCoarse<T>(T e) => default!;
    // fn dpdxFine(e: T) -> T
    public static T dpdxFine<T>(T e) => default!;
    // fn dpdy(e: T) -> T
    public static T dpdy<T>(T e) => default!;
    // fn dpdyCoarse(e: T) -> T
    public static T dpdyCoarse<T>(T e) => default!;
    // fn dpdyFine(e: T) -> T
    public static T dpdyFine<T>(T e) => default!;
    // fn fwidth(e: T) -> T
    public static T fwidth<T>(T e) => default!;
    // fn fwidthCoarse(e: T) -> T
    public static T fwidthCoarse<T>(T e) => default!;
    // fn fwidthFine(e: T) -> T
    public static T fwidthFine<T>(T e) => default!;
    // fn textureDimensions(t: T) -> u32
    public static u32 textureDimensions<T>(T t) => default!;
    // fn textureDimensions(t: T, level: L) -> u32
    public static u32 textureDimensions<T>(T t, u32 level = default!) => default!;
    // fn textureDimensions(t: T) -> vec2<u32>
    // public static vec2u textureDimensions(f32 t) => default!;
    // public static vec2u textureDimensions(i32 t) => default!;
    // public static vec2u textureDimensions(u32 t) => default!;
    // fn textureDimensions(t: T, level: L) -> vec2<u32>
    // public static vec2u textureDimensions(f32 t, u32 level = default!) => default!;
    // public static vec2u textureDimensions(i32 t, u32 level = default!) => default!;
    // public static vec2u textureDimensions(u32 t, u32 level = default!) => default!;
    // fn textureDimensions(t: T) -> vec3<u32>
    public static vec3u textureDimensions(f32 t) => default!;
    public static vec3u textureDimensions(i32 t) => default!;
    public static vec3u textureDimensions(u32 t) => default!;
    // fn textureDimensions(t: T, level: L) -> vec3<u32>
    public static vec3u textureDimensions(f32 t, u32 level = default!) => default!;
    public static vec3u textureDimensions(i32 t, u32 level = default!) => default!;
    public static vec3u textureDimensions(u32 t, u32 level = default!) => default!;
    // fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>) -> vec4<ST>
    public static vec4f textureGather<C, VM1>(C component, texture_2d<f32> t, sampler s, VM1 coords) => default!;
    public static vec4i textureGather<C, VM1>(C component, texture_2d<i32> t, sampler s, VM1 coords) => default!;
    public static vec4u textureGather<C, VM1>(C component, texture_2d<u32> t, sampler s, VM1 coords) => default!;
    // fn textureGather(component: C, t: texture_2d<ST>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<ST>
    public static vec4f textureGather<C, VM1, VM2>(C component, texture_2d<f32> t, sampler s, VM1 coords, VM2 offset) => default!;
    public static vec4i textureGather<C, VM1, VM2>(C component, texture_2d<i32> t, sampler s, VM1 coords, VM2 offset) => default!;
    public static vec4u textureGather<C, VM1, VM2>(C component, texture_2d<u32> t, sampler s, VM1 coords, VM2 offset) => default!;
    // fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<ST>
    public static vec4f textureGather<C, VM1>(C component, texture_2d_array<f32> t, sampler s, VM1 coords, u32 array_index = default!) => default!;
    public static vec4i textureGather<C, VM1>(C component, texture_2d_array<i32> t, sampler s, VM1 coords, u32 array_index = default!) => default!;
    public static vec4u textureGather<C, VM1>(C component, texture_2d_array<u32> t, sampler s, VM1 coords, u32 array_index = default!) => default!;
    // fn textureGather(component: C, t: texture_2d_array<ST>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<ST>
    public static vec4f textureGather<C, VM1, VM2>(C component, texture_2d_array<f32> t, sampler s, VM1 coords, u32 array_index = default!, VM2 offset = default!) => default!;
    public static vec4i textureGather<C, VM1, VM2>(C component, texture_2d_array<i32> t, sampler s, VM1 coords, u32 array_index = default!, VM2 offset = default!) => default!;
    public static vec4u textureGather<C, VM1, VM2>(C component, texture_2d_array<u32> t, sampler s, VM1 coords, u32 array_index = default!, VM2 offset = default!) => default!;
    // fn textureGather(component: C, t: texture_cube<ST>, s: sampler, coords: vec3<f32>) -> vec4<ST>
    public static vec4f textureGather<C, VM1>(C component, texture_cube<f32> t, sampler s, VM1 coords) => default!;
    public static vec4i textureGather<C, VM1>(C component, texture_cube<i32> t, sampler s, VM1 coords) => default!;
    public static vec4u textureGather<C, VM1>(C component, texture_cube<u32> t, sampler s, VM1 coords) => default!;
    // fn textureGather(component: C, t: texture_cube_array<ST>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<ST>
    public static vec4f textureGather<C, VM1>(C component, texture_cube_array<f32> t, sampler s, VM1 coords, u32 array_index = default!) => default!;
    public static vec4i textureGather<C, VM1>(C component, texture_cube_array<i32> t, sampler s, VM1 coords, u32 array_index = default!) => default!;
    public static vec4u textureGather<C, VM1>(C component, texture_cube_array<u32> t, sampler s, VM1 coords, u32 array_index = default!) => default!;
    // fn textureGather(t: texture_depth_2d, s: sampler, coords: vec2<f32>) -> vec4<f32>
    public static vec4f textureGather<VM0>(texture_depth_2d t, sampler s, VM0 coords) => default!;
    // fn textureGather(t: texture_depth_2d, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureGather<VM0, VM1>(texture_depth_2d t, sampler s, VM0 coords, VM1 offset) => default!;
    // fn textureGather(t: texture_depth_cube, s: sampler, coords: vec3<f32>) -> vec4<f32>
    public static vec4f textureGather<VM0>(texture_depth_cube t, sampler s, VM0 coords) => default!;
    // fn textureGather(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<f32>
    public static vec4f textureGather<VM0>(texture_depth_2d_array t, sampler s, VM0 coords, u32 array_index = default!) => default!;
    // fn textureGather(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureGather<VM0, VM1>(texture_depth_2d_array t, sampler s, VM0 coords, u32 array_index = default!, VM1 offset = default!) => default!;
    // fn textureGather(t: texture_depth_cube_array, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<f32>
    public static vec4f textureGather<VM0>(texture_depth_cube_array t, sampler s, VM0 coords, u32 array_index = default!) => default!;
    // fn gather_x_components(c: vec2<f32>) -> vec4<f32>
    public static vec4f gather_x_components<VM0>(VM0 c) => default!;
    // fn gather_y_components(c: vec2<f32>) -> vec4<f32>
    public static vec4f gather_y_components<VM0>(VM0 c) => default!;
    // fn gather_z_components(c: vec2<f32>) -> vec4<f32>
    public static vec4f gather_z_components<VM0>(VM0 c) => default!;
    // fn gather_depth_components(c: vec2<f32>) -> vec4<f32>
    public static vec4f gather_depth_components<VM0>(VM0 c) => default!;
    // fn textureGatherCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32) -> vec4<f32>
    public static vec4f textureGatherCompare<VM0>(texture_depth_2d t, sampler_comparison s, VM0 coords, f32 depth_ref) => default!;
    // fn textureGatherCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureGatherCompare<VM0, VM1>(texture_depth_2d t, sampler_comparison s, VM0 coords, f32 depth_ref, VM1 offset) => default!;
    // fn textureGatherCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32) -> vec4<f32>
    public static vec4f textureGatherCompare<VM0>(texture_depth_2d_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!) => default!;
    // fn textureGatherCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureGatherCompare<VM0, VM1>(texture_depth_2d_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!, VM1 offset = default!) => default!;
    // fn textureGatherCompare(t: texture_depth_cube, s: sampler_comparison, coords: vec3<f32>, depth_ref: f32) -> vec4<f32>
    public static vec4f textureGatherCompare<VM0>(texture_depth_cube t, sampler_comparison s, VM0 coords, f32 depth_ref) => default!;
    // fn textureGatherCompare(t: texture_depth_cube_array, s: sampler_comparison, coords: vec3<f32>, array_index: A, depth_ref: f32) -> vec4<f32>
    public static vec4f textureGatherCompare<VM0>(texture_depth_cube_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!) => default!;
    // fn gather_depth_compare(c: vec2<f32>, depth_ref: f32) -> vec4<f32>
    public static vec4f gather_depth_compare<VM0>(VM0 c, f32 depth_ref) => default!;
    // fn textureLoad(t: texture_1d<ST>, coords: C, level: L) -> vec4<ST>
    public static vec4f textureLoad<C>(texture_1d<f32> t, C coords, u32 level = default!) => default!;
    public static vec4i textureLoad<C>(texture_1d<i32> t, C coords, u32 level = default!) => default!;
    public static vec4u textureLoad<C>(texture_1d<u32> t, C coords, u32 level = default!) => default!;
    // fn textureLoad(t: texture_2d<ST>, coords: vec2<C>, level: L) -> vec4<ST>
    public static vec4f textureLoad<VM0>(texture_2d<f32> t, VM0 coords, u32 level = default!) => default!;
    public static vec4i textureLoad<VM0>(texture_2d<i32> t, VM0 coords, u32 level = default!) => default!;
    public static vec4u textureLoad<VM0>(texture_2d<u32> t, VM0 coords, u32 level = default!) => default!;
    // fn textureLoad(t: texture_2d_array<ST>, coords: vec2<C>, array_index: A, level: L) -> vec4<ST>
    public static vec4f textureLoad<VM0>(texture_2d_array<f32> t, VM0 coords, u32 array_index = default!, u32 level = default!) => default!;
    public static vec4i textureLoad<VM0>(texture_2d_array<i32> t, VM0 coords, u32 array_index = default!, u32 level = default!) => default!;
    public static vec4u textureLoad<VM0>(texture_2d_array<u32> t, VM0 coords, u32 array_index = default!, u32 level = default!) => default!;
    // fn textureLoad(t: texture_3d<ST>, coords: vec3<C>, level: L) -> vec4<ST>
    public static vec4f textureLoad<VM0>(texture_3d<f32> t, VM0 coords, u32 level = default!) => default!;
    public static vec4i textureLoad<VM0>(texture_3d<i32> t, VM0 coords, u32 level = default!) => default!;
    public static vec4u textureLoad<VM0>(texture_3d<u32> t, VM0 coords, u32 level = default!) => default!;
    // fn textureLoad(t: texture_multisampled_2d<ST>, coords: vec2<C>, sample_index: S)-> vec4<ST>
    public static vec4f textureLoad<VM0, S>(texture_multisampled_2d<f32> t, VM0 coords, S sample_index) => default!;
    public static vec4i textureLoad<VM0, S>(texture_multisampled_2d<i32> t, VM0 coords, S sample_index) => default!;
    public static vec4u textureLoad<VM0, S>(texture_multisampled_2d<u32> t, VM0 coords, S sample_index) => default!;
    // fn textureLoad(t: texture_depth_2d, coords: vec2<C>, level: L) -> f32
    public static f32 textureLoad<VM0>(texture_depth_2d t, VM0 coords, u32 level = default!) => default!;
    // fn textureLoad(t: texture_depth_2d_array, coords: vec2<C>, array_index: A, level: L) -> f32
    public static f32 textureLoad<VM0>(texture_depth_2d_array t, VM0 coords, u32 array_index = default!, u32 level = default!) => default!;
    // fn textureLoad(t: texture_depth_multisampled_2d, coords: vec2<C>, sample_index: S)-> f32
    public static f32 textureLoad<VM0, T>(texture_depth_multisampled_2d t, VM0 coords, T sample_index) => default!;
    // public static f32 textureLoad<VM0, T>(texture_depth_multisampled_2d t, VM0 coords, T sample_index) => default!;
    // public static f32 textureLoad<VM0, T>(texture_depth_multisampled_2d t, VM0 coords, T sample_index) => default!;
    // fn textureLoad(t: texture_external, coords: vec2<C>) -> vec4<f32>
    public static vec4f textureLoad<VM0>(texture_external t, VM0 coords) => default!;
    // fn textureLoad(t : texture_storage_1d<F, AM>, coords : C) -> vec4<CF>
    public static vec4f textureLoad<C>(texture_storage_1d<f32> t, C coords) => default!;
    public static vec4i textureLoad<C>(texture_storage_1d<i32> t, C coords) => default!;
    public static vec4u textureLoad<C>(texture_storage_1d<u32> t, C coords) => default!;
    // fn textureLoad(t : texture_storage_2d<F, AM>, coords : vec2<C>) -> vec4<CF>
    public static vec4f textureLoad<VM0>(texture_storage_2d<f32> t, VM0 coords) => default!;
    public static vec4i textureLoad<VM0>(texture_storage_2d<i32> t, VM0 coords) => default!;
    public static vec4u textureLoad<VM0>(texture_storage_2d<u32> t, VM0 coords) => default!;
    // fn textureLoad(t : texture_storage_2d_array<F, AM>, coords : vec2<C>, array_index : A) -> vec4<CF>
    public static vec4f textureLoad<VM0>(texture_storage_2d_array<f32> t, VM0 coords, u32 array_index = default!) => default!;
    public static vec4i textureLoad<VM0>(texture_storage_2d_array<i32> t, VM0 coords, u32 array_index = default!) => default!;
    public static vec4u textureLoad<VM0>(texture_storage_2d_array<u32> t, VM0 coords, u32 array_index = default!) => default!;
    // fn textureLoad(t : texture_storage_3d<F, AM>, coords : vec3<C>) -> vec4<CF>
    public static vec4f textureLoad<VM0>(texture_storage_3d<f32> t, VM0 coords) => default!;
    public static vec4i textureLoad<VM0>(texture_storage_3d<i32> t, VM0 coords) => default!;
    public static vec4u textureLoad<VM0>(texture_storage_3d<u32> t, VM0 coords) => default!;
    // fn textureNumLayers(t: T) -> u32
    public static u32 textureNumLayers<T>(T t) => default!;
    // fn textureNumLevels(t: T) -> u32
    public static u32 textureNumLevels<T>(T t) => default!;
    // fn textureNumSamples(t: T) -> u32
    public static u32 textureNumSamples<T>(T t) => default!;
    // fn textureSample(t: texture_1d<f32>, s: sampler, coords: f32) -> vec4<f32>
    public static vec4f textureSample(texture_1d<f32> t, sampler s, f32 coords) => default!;
    // fn textureSample(t: texture_2d<f32>, s: sampler, coords: vec2<f32>) -> vec4<f32>
    public static vec4f textureSample<VM0>(texture_2d<f32> t, sampler s, VM0 coords) => default!;
    // fn textureSample(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSample<VM0, VM1>(texture_2d<f32> t, sampler s, VM0 coords, VM1 offset) => default!;
    // fn textureSample(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A) -> vec4<f32>
    public static vec4f textureSample<VM0>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!) => default!;
    // fn textureSample(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSample<VM0, VM1>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, VM1 offset = default!) => default!;
    // fn textureSample(t: T, s: sampler, coords: vec3<f32>) -> vec4<f32>
    public static vec4f textureSample<VM1>(f32 t, sampler s, VM1 coords) => default!;
    public static vec4f textureSample<VM1>(i32 t, sampler s, VM1 coords) => default!;
    public static vec4f textureSample<VM1>(u32 t, sampler s, VM1 coords) => default!;
    // fn textureSample(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, offset: vec3<i32>) -> vec4<f32>
    public static vec4f textureSample<VM0, VM1>(texture_3d<f32> t, sampler s, VM0 coords, VM1 offset) => default!;
    // fn textureSample(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A) -> vec4<f32>
    public static vec4f textureSample<VM0>(texture_cube_array<f32> t, sampler s, VM0 coords, u32 array_index = default!) => default!;
    // fn textureSample(t: texture_depth_2d, s: sampler, coords: vec2<f32>) -> f32
    public static f32 textureSample<VM0>(texture_depth_2d t, sampler s, VM0 coords) => default!;
    // fn textureSample(t: texture_depth_2d, s: sampler, coords: vec2<f32>, offset: vec2<i32>) -> f32
    public static f32 textureSample<VM0, VM1>(texture_depth_2d t, sampler s, VM0 coords, VM1 offset) => default!;
    // fn textureSample(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A) -> f32
    public static f32 textureSample<VM0>(texture_depth_2d_array t, sampler s, VM0 coords, u32 array_index = default!) => default!;
    // fn textureSample(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, offset: vec2<i32>) -> f32
    public static f32 textureSample<VM0, VM1>(texture_depth_2d_array t, sampler s, VM0 coords, u32 array_index = default!, VM1 offset = default!) => default!;
    // fn textureSample(t: texture_depth_cube, s: sampler, coords: vec3<f32>) -> f32
    public static f32 textureSample<VM0>(texture_depth_cube t, sampler s, VM0 coords) => default!;
    // fn textureSample(t: texture_depth_cube_array, s: sampler, coords: vec3<f32>, array_index: A) -> f32
    public static f32 textureSample<VM0>(texture_depth_cube_array t, sampler s, VM0 coords, u32 array_index = default!) => default!;
    // fn textureSampleBias(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, bias: f32) -> vec4<f32>
    public static vec4f textureSampleBias<VM0>(texture_2d<f32> t, sampler s, VM0 coords, f32 bias) => default!;
    // fn textureSampleBias(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, bias: f32, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSampleBias<VM0, VM1>(texture_2d<f32> t, sampler s, VM0 coords, f32 bias, VM1 offset) => default!;
    // fn textureSampleBias(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, bias: f32) -> vec4<f32>
    public static vec4f textureSampleBias<VM0>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, f32 bias = default!) => default!;
    // fn textureSampleBias(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, bias: f32, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSampleBias<VM0, VM1>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, f32 bias = default!, VM1 offset = default!) => default!;
    // fn textureSampleBias(t: T, s: sampler, coords: vec3<f32>, bias: f32) -> vec4<f32>
    public static vec4f textureSampleBias<VM1>(f32 t, sampler s, VM1 coords, f32 bias) => default!;
    public static vec4f textureSampleBias<VM1>(i32 t, sampler s, VM1 coords, f32 bias) => default!;
    public static vec4f textureSampleBias<VM1>(u32 t, sampler s, VM1 coords, f32 bias) => default!;
    // fn textureSampleBias(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, bias: f32, offset: vec3<i32>) -> vec4<f32>
    public static vec4f textureSampleBias<VM0, VM1>(texture_3d<f32> t, sampler s, VM0 coords, f32 bias, VM1 offset) => default!;
    // fn textureSampleBias(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A, bias: f32) -> vec4<f32>
    public static vec4f textureSampleBias<VM0>(texture_cube_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, f32 bias = default!) => default!;
    // fn textureSampleCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32) -> f32
    public static f32 textureSampleCompare<VM0>(texture_depth_2d t, sampler_comparison s, VM0 coords, f32 depth_ref) => default!;
    // fn textureSampleCompare(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32, offset: vec2<i32>) -> f32
    public static f32 textureSampleCompare<VM0, VM1>(texture_depth_2d t, sampler_comparison s, VM0 coords, f32 depth_ref, VM1 offset) => default!;
    // fn textureSampleCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32) -> f32
    public static f32 textureSampleCompare<VM0>(texture_depth_2d_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!) => default!;
    // fn textureSampleCompare(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32, offset: vec2<i32>) -> f32
    public static f32 textureSampleCompare<VM0, VM1>(texture_depth_2d_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!, VM1 offset = default!) => default!;
    // fn textureSampleCompare(t: texture_depth_cube, s: sampler_comparison, coords: vec3<f32>, depth_ref: f32) -> f32
    public static f32 textureSampleCompare<VM0>(texture_depth_cube t, sampler_comparison s, VM0 coords, f32 depth_ref) => default!;
    // fn textureSampleCompare(t: texture_depth_cube_array, s: sampler_comparison, coords: vec3<f32>, array_index: A, depth_ref: f32) -> f32
    public static f32 textureSampleCompare<VM0>(texture_depth_cube_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!) => default!;
    // fn textureSampleCompareLevel(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32) -> f32
    public static f32 textureSampleCompareLevel<VM0>(texture_depth_2d t, sampler_comparison s, VM0 coords, f32 depth_ref) => default!;
    // fn textureSampleCompareLevel(t: texture_depth_2d, s: sampler_comparison, coords: vec2<f32>, depth_ref: f32, offset: vec2<i32>) -> f32
    public static f32 textureSampleCompareLevel<VM0, VM1>(texture_depth_2d t, sampler_comparison s, VM0 coords, f32 depth_ref, VM1 offset) => default!;
    // fn textureSampleCompareLevel(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32) -> f32
    public static f32 textureSampleCompareLevel<VM0>(texture_depth_2d_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!) => default!;
    // fn textureSampleCompareLevel(t: texture_depth_2d_array, s: sampler_comparison, coords: vec2<f32>, array_index: A, depth_ref: f32, offset: vec2<i32>) -> f32
    public static f32 textureSampleCompareLevel<VM0, VM1>(texture_depth_2d_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!, VM1 offset = default!) => default!;
    // fn textureSampleCompareLevel(t: texture_depth_cube, s: sampler_comparison, coords: vec3<f32>, depth_ref: f32) -> f32
    public static f32 textureSampleCompareLevel<VM0>(texture_depth_cube t, sampler_comparison s, VM0 coords, f32 depth_ref) => default!;
    // fn textureSampleCompareLevel(t: texture_depth_cube_array, s: sampler_comparison, coords: vec3<f32>, array_index: A, depth_ref: f32) -> f32
    public static f32 textureSampleCompareLevel<VM0>(texture_depth_cube_array t, sampler_comparison s, VM0 coords, u32 array_index = default!, f32 depth_ref = default!) => default!;
    // fn textureSampleGrad(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, ddx: vec2<f32>, ddy: vec2<f32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM0>(texture_2d<f32> t, sampler s, VM0 coords, VM0 ddx, VM0 ddy) => default!;
    // fn textureSampleGrad(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, ddx: vec2<f32>, ddy: vec2<f32>, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM0, VM1>(texture_2d<f32> t, sampler s, VM0 coords, VM0 ddx, VM0 ddy, VM1 offset) => default!;
    // fn textureSampleGrad(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, ddx: vec2<f32>, ddy: vec2<f32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM0>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, VM0 ddx = default!, VM0 ddy = default!) => default!;
    // fn textureSampleGrad(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, ddx: vec2<f32>, ddy: vec2<f32>, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM0, VM1>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, VM0 ddx = default!, VM0 ddy = default!, VM1 offset = default!) => default!;
    // fn textureSampleGrad(t: T, s: sampler, coords: vec3<f32>, ddx: vec3<f32>, ddy: vec3<f32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM1>(f32 t, sampler s, VM1 coords, VM1 ddx, VM1 ddy) => default!;
    public static vec4f textureSampleGrad<VM1>(i32 t, sampler s, VM1 coords, VM1 ddx, VM1 ddy) => default!;
    public static vec4f textureSampleGrad<VM1>(u32 t, sampler s, VM1 coords, VM1 ddx, VM1 ddy) => default!;
    // fn textureSampleGrad(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, ddx: vec3<f32>, ddy: vec3<f32>, offset: vec3<i32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM0, VM1>(texture_3d<f32> t, sampler s, VM0 coords, VM0 ddx, VM0 ddy, VM1 offset) => default!;
    // fn textureSampleGrad(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A, ddx: vec3<f32>, ddy: vec3<f32>) -> vec4<f32>
    public static vec4f textureSampleGrad<VM0>(texture_cube_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, VM0 ddx = default!, VM0 ddy = default!) => default!;
    // fn textureSampleLevel(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, level: f32) -> vec4<f32>
    public static vec4f textureSampleLevel<VM0>(texture_2d<f32> t, sampler s, VM0 coords, f32 level) => default!;
    // fn textureSampleLevel(t: texture_2d<f32>, s: sampler, coords: vec2<f32>, level: f32, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSampleLevel<VM0, VM1>(texture_2d<f32> t, sampler s, VM0 coords, f32 level, VM1 offset) => default!;
    // fn textureSampleLevel(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, level: f32) -> vec4<f32>
    public static vec4f textureSampleLevel<VM0>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, f32 level = default!) => default!;
    // fn textureSampleLevel(t: texture_2d_array<f32>, s: sampler, coords: vec2<f32>, array_index: A, level: f32, offset: vec2<i32>) -> vec4<f32>
    public static vec4f textureSampleLevel<VM0, VM1>(texture_2d_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, f32 level = default!, VM1 offset = default!) => default!;
    // fn textureSampleLevel(t: T, s: sampler, coords: vec3<f32>, level: f32) -> vec4<f32>
    public static vec4f textureSampleLevel<VM1>(f32 t, sampler s, VM1 coords, f32 level) => default!;
    public static vec4f textureSampleLevel<VM1>(i32 t, sampler s, VM1 coords, f32 level) => default!;
    public static vec4f textureSampleLevel<VM1>(u32 t, sampler s, VM1 coords, f32 level) => default!;
    // fn textureSampleLevel(t: texture_3d<f32>, s: sampler, coords: vec3<f32>, level: f32, offset: vec3<i32>) -> vec4<f32>
    public static vec4f textureSampleLevel<VM0, VM1>(texture_3d<f32> t, sampler s, VM0 coords, f32 level, VM1 offset) => default!;
    // fn textureSampleLevel(t: texture_cube_array<f32>, s: sampler, coords: vec3<f32>, array_index: A, level: f32) -> vec4<f32>
    public static vec4f textureSampleLevel<VM0>(texture_cube_array<f32> t, sampler s, VM0 coords, u32 array_index = default!, f32 level = default!) => default!;
    // fn textureSampleLevel(t: texture_depth_2d, s: sampler, coords: vec2<f32>, level: L) -> f32
    public static f32 textureSampleLevel<VM0>(texture_depth_2d t, sampler s, VM0 coords, u32 level = default!) => default!;
    // fn textureSampleLevel(t: texture_depth_2d, s: sampler, coords: vec2<f32>, level: L, offset: vec2<i32>) -> f32
    public static f32 textureSampleLevel<VM0, VM1>(texture_depth_2d t, sampler s, VM0 coords, u32 level = default!, VM1 offset = default!) => default!;
    // fn textureSampleLevel(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, level: L) -> f32
    public static f32 textureSampleLevel<VM0>(texture_depth_2d_array t, sampler s, VM0 coords, u32 array_index = default!, u32 level = default!) => default!;
    // fn textureSampleLevel(t: texture_depth_2d_array, s: sampler, coords: vec2<f32>, array_index: A, level: L, offset: vec2<i32>) -> f32
    public static f32 textureSampleLevel<VM0, VM1>(texture_depth_2d_array t, sampler s, VM0 coords, u32 array_index = default!, u32 level = default!, VM1 offset = default!) => default!;
    // fn textureSampleLevel(t: texture_depth_cube, s: sampler, coords: vec3<f32>, level: L) -> f32
    public static f32 textureSampleLevel<VM0>(texture_depth_cube t, sampler s, VM0 coords, u32 level = default!) => default!;
    // fn textureSampleLevel(t: texture_depth_cube_array, s: sampler, coords: vec3<f32>, array_index: A, level: L) -> f32
    public static f32 textureSampleLevel<VM0>(texture_depth_cube_array t, sampler s, VM0 coords, u32 array_index = default!, u32 level = default!) => default!;
    // fn textureSampleBaseClampToEdge(t: T, s: sampler, coords: vec2<f32>) -> vec4<f32>
    public static vec4f textureSampleBaseClampToEdge<VM1>(f32 t, sampler s, VM1 coords) => default!;
    public static vec4f textureSampleBaseClampToEdge<VM1>(i32 t, sampler s, VM1 coords) => default!;
    public static vec4f textureSampleBaseClampToEdge<VM1>(u32 t, sampler s, VM1 coords) => default!;
    // fn atomicAdd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicAdd<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicSub(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicSub<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicMax(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicMax<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicMin(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicMin<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicAnd(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicAnd<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicOr(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicOr<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicXor(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicXor<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicExchange(atomic_ptr: ptr<AS, atomic<T>, read_write>, v : T) -> T
    public static T atomicExchange<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T v) => default!;
    // fn atomicCompareExchangeWeak(atomic_ptr: ptr<AS, atomic<T>, read_write>, cmp : T, v : T) -> _atomic_compare_exchange_result<T>
    public partial class _atomic_compare_exchange_result<T>;
    public static _atomic_compare_exchange_result<T> atomicCompareExchangeWeak<T>(ptr<dynamic, atomic<T>, read_write> atomic_ptr, T cmp, T v) => default!;
    // fn pack4x8snorm(e: vec4<f32>) -> u32
    public static u32 pack4x8snorm<VM0>(VM0 e) => default!;
    // fn pack4x8unorm(e: vec4<f32>) -> u32
    public static u32 pack4x8unorm<VM0>(VM0 e) => default!;
    // fn pack4xI8(e: vec4<i32>) -> u32
    public static u32 pack4xI8<VM0>(VM0 e) => default!;
    // fn pack4xU8(e: vec4<u32>) -> u32
    public static u32 pack4xU8<VM0>(VM0 e) => default!;
    // fn pack4xI8Clamp(e: vec4<i32>) -> u32
    public static u32 pack4xI8Clamp<VM0>(VM0 e) => default!;
    // fn pack4xU8Clamp(e: vec4<u32>) -> u32
    public static u32 pack4xU8Clamp<VM0>(VM0 e) => default!;
    // fn pack2x16snorm(e: vec2<f32>) -> u32
    public static u32 pack2x16snorm<VM0>(VM0 e) => default!;
    // fn pack2x16unorm(e: vec2<f32>) -> u32
    public static u32 pack2x16unorm<VM0>(VM0 e) => default!;
    // fn pack2x16float(e: vec2<f32>) -> u32
    public static u32 pack2x16float<VM0>(VM0 e) => default!;
    // fn unpack4x8snorm(e: u32) -> vec4<f32>
    public static vec4f unpack4x8snorm(u32 e) => default!;
    // fn unpack4x8unorm(e: u32) -> vec4<f32>
    public static vec4f unpack4x8unorm(u32 e) => default!;
    // fn unpack4xI8(e: u32) -> vec4<i32>
    public static vec4i unpack4xI8(u32 e) => default!;
    // fn unpack4xU8(e: u32) -> vec4<u32>
    public static vec4u unpack4xU8(u32 e) => default!;
    // fn unpack2x16snorm(e: u32) -> vec2<f32>
    public static vec2f unpack2x16snorm(u32 e) => default!;
    // fn unpack2x16unorm(e: u32) -> vec2<f32>
    public static vec2f unpack2x16unorm(u32 e) => default!;
    // fn unpack2x16float(e: u32) -> vec2<f32>
    public static vec2f unpack2x16float(u32 e) => default!;
    // fn workgroupUniformLoad(p : ptr<workgroup, T>) -> T
    public static T workgroupUniformLoad<T>(ptr<workgroup, T> p) => default!;
    // fn subgroupAdd(e : T) -> T
    public static T subgroupAdd<T>(T e) => default!;
    // fn subgroupExclusiveAdd(e : T) -> T
    public static T subgroupExclusiveAdd<T>(T e) => default!;
    // fn subgroupInclusiveAdd(e : T) -> T
    public static T subgroupInclusiveAdd<T>(T e) => default!;
    // fn subgroupAll(e : bool) -> bool
    public static bool subgroupAll(bool e) => default!;
    // fn subgroupAnd(e : T) -> T
    public static T subgroupAnd<T>(T e) => default!;
    // fn subgroupAny(e : bool) -> bool
    public static bool subgroupAny(bool e) => default!;
    // fn subgroupBallot(pred : bool) -> vec4<u32>
    public static vec4u subgroupBallot(bool pred) => default!;
    // fn subgroupBroadcast(e : T, id : I) -> T
    public static T subgroupBroadcast<T, I>(T e, I id) => default!;
    // fn subgroupBroadcastFirst(e : T) -> T
    public static T subgroupBroadcastFirst<T>(T e) => default!;
    // fn subgroupElect() -> bool
    public static bool subgroupElect() => default!;
    // fn subgroupMax(e : T) -> T
    public static T subgroupMax<T>(T e) => default!;
    // fn subgroupMin(e : T) -> T
    public static T subgroupMin<T>(T e) => default!;
    // fn subgroupMul(e : T) -> T
    public static T subgroupMul<T>(T e) => default!;
    // fn subgroupExclusiveMul(e : T) -> T
    public static T subgroupExclusiveMul<T>(T e) => default!;
    // fn subgroupInclusiveMul(e : T) -> T
    public static T subgroupInclusiveMul<T>(T e) => default!;
    // fn subgroupOr(e : T) -> T
    public static T subgroupOr<T>(T e) => default!;
    // fn subgroupShuffle(e : T, id : I) -> T
    public static T subgroupShuffle<T, I>(T e, I id) => default!;
    // fn subgroupShuffleDown(e : T, delta : u32) -> T
    public static T subgroupShuffleDown<T>(T e, u32 delta) => default!;
    // fn subgroupShuffleUp(e : T, delta : u32) -> T
    public static T subgroupShuffleUp<T>(T e, u32 delta) => default!;
    // fn subgroupShuffleXor(e : T, mask : u32) -> T
    public static T subgroupShuffleXor<T>(T e, u32 mask) => default!;
    // fn subgroupXor(e : T) -> T
    public static T subgroupXor<T>(T e) => default!;
    // fn quadBroadcast(e : T, id : I) -> T
    public static T quadBroadcast<T, I>(T e, I id) => default!;
    // fn quadSwapDiagonal(e : T) -> T
    public static T quadSwapDiagonal<T>(T e) => default!;
    // fn quadSwapX(e : T) -> T
    public static T quadSwapX<T>(T e) => default!;
    // fn quadSwapY(e : T) -> T
    public static T quadSwapY<T>(T e) => default!;
    
    // Valid Function Names:
    
    // helper
    // halve_it
    // min
    // foo
    // particle_velocity_component
    // get_age
    // two_pi
    // increment_and_yield_previous
    // advance_item
    // simple
    // nested
    // add_two
    // f4
    // f7
    // fragShader
    // foo_for_analysis
    // scale
    // T
    // array
    // bool
    // f16
    // f32
    // i32
    // mat2x2
    // mat2x3
    // mat2x4
    // mat3x2
    // mat3x3
    // mat3x4
    // mat4x2
    // mat4x3
    // mat4x4
    // S
    // u32
    // vec2
    // vec3
    // vec4
    // bitcast
    // all
    // any
    // select
    // arrayLength
    // num_point_lights
    // abs
    // acos
    // acosh
    // asin
    // asinh
    // atan
    // atanh
    // atan2
    // ceil
    // clamp
    // cos
    // cosh
    // countLeadingZeros
    // countOneBits
    // countTrailingZeros
    // cross
    // degrees
    // determinant
    // distance
    // dot
    // dot4U8Packed
    // dot4I8Packed
    // exp
    // exp2
    // extractBits
    // faceForward
    // firstLeadingBit
    // firstTrailingBit
    // floor
    // fma
    // fract
    // frexp
    // insertBits
    // inverseSqrt
    // ldexp
    // length
    // log
    // log2
    // max
    // mix
    // modf
    // normalize
    // pow
    // quantizeToF16
    // radians
    // reflect
    // refract
    // reverseBits
    // round
    // saturate
    // sign
    // sin
    // sinh
    // smoothstep
    // sqrt
    // step
    // tan
    // tanh
    // transpose
    // trunc
    // dpdx
    // dpdxCoarse
    // dpdxFine
    // dpdy
    // dpdyCoarse
    // dpdyFine
    // fwidth
    // fwidthCoarse
    // fwidthFine
    // textureDimensions
    // textureGather
    // gather_x_components
    // gather_y_components
    // gather_z_components
    // gather_depth_components
    // textureGatherCompare
    // gather_depth_compare
    // textureLoad
    // textureNumLayers
    // textureNumLevels
    // textureNumSamples
    // textureSample
    // textureSampleBias
    // textureSampleCompare
    // textureSampleCompareLevel
    // textureSampleGrad
    // textureSampleLevel
    // textureSampleBaseClampToEdge
    // atomicAdd
    // atomicSub
    // atomicMax
    // atomicMin
    // atomicAnd
    // atomicOr
    // atomicXor
    // atomicExchange
    // atomicCompareExchangeWeak
    // pack4x8snorm
    // pack4x8unorm
    // pack4xI8
    // pack4xU8
    // pack4xI8Clamp
    // pack4xU8Clamp
    // pack2x16snorm
    // pack2x16unorm
    // pack2x16float
    // unpack4x8snorm
    // unpack4x8unorm
    // unpack4xI8
    // unpack4xU8
    // unpack2x16snorm
    // unpack2x16unorm
    // unpack2x16float
    // workgroupUniformLoad
    // subgroupAdd
    // subgroupExclusiveAdd
    // subgroupInclusiveAdd
    // subgroupAll
    // subgroupAnd
    // subgroupAny
    // subgroupBallot
    // subgroupBroadcast
    // subgroupBroadcastFirst
    // subgroupElect
    // subgroupMax
    // subgroupMin
    // subgroupMul
    // subgroupExclusiveMul
    // subgroupInclusiveMul
    // subgroupOr
    // subgroupShuffle
    // subgroupShuffleDown
    // subgroupShuffleUp
    // subgroupShuffleXor
    // subgroupXor
    // quadBroadcast
    // quadSwapDiagonal
    // quadSwapX
    // quadSwapY
    }

