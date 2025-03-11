// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable InconsistentNaming

using System.Net.NetworkInformation;
using System.Numerics;
using GlmSharp;
using Silk.NET.Maths;
using u32 = System.UInt32;
using i32 = System.Int32;
using f32 = System.Single;
using f64 = System.Double;
using u64 = System.UInt64;
using i64 = System.Int64;
using u8 = System.Byte;
using i8 = System.SByte;
using Silk.NET.WebGPU;


namespace WgslGenerator;

public static class BuildIn
{
    public partial class texture_2d<T>;

    public partial class texture_depth_2d;

    public partial class sampler;


    #region Functions

    public static vec4 textureLoad<V>(texture_2d<f32> tex, V pos) => default!;
    public static f32 textureLoad<V>(texture_depth_2d tex, V pos) => default!;
    public static vec4 textureSample<V>(texture_2d<f32> tex, sampler sampler, V pos) => default!;


    public static f32 floor(f32 v) => default!;
    public static f32 ceil(f32 v) => default!;
    public static f32 max(f32 a, f32 b) => default!;
    public static f32 min(f32 a, f32 b) => default!;
    public static f32 sqrt(f32 a) => default!;
    public static f32 pow(f32 a, f32 b) => default!;
    public static T normalize<T>(T v) => default!;
    public static T cross<T>(T a, T b) => default!;
    public static T reflect<T>(T i, T n) => default!;
    public static T refract<T>(T i, T n, f32 eta) => default!;
    public static T mix<T>(T a, T b, f32 t) => default!;
    public static T step<T>(T edge, T x) => default!;
    public static T smoothstep<T>(T edge0, T edge1, T x) => default!;
    public static T length<T>(T v) => default!;
    public static T distance<T>(T a, T b) => default!;
    public static T dot<T>(T a, T b) => default!;
    public static T normalize<T>(T v, f32 l) => default!;


    public static T[] array<T>(params T[] values) => default!;
    
    public static T fma<T>(T a, f32 b, f32 c) => default!;

    #endregion

    public static void _raw(this string s)
    {
    }

    public static T _raw<T>(this string s) => default!;
}
