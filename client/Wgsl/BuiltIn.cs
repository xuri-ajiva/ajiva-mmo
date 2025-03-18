// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable InconsistentNaming
namespace Client.Wgsl;

public partial class BuiltIn
{
    /*public static vec4 textureLoad<V>(texture_2d<f32> tex, V pos)
        where V : IReadOnlyCollection<i32>, IReadOnlyCollection<u32> => default!;

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
    public static T refract<T>(T i, T n, f32 eta) => default!;
    public static T mix<T>(T a, T b, f32 t) => default!;
    public static T distance<T>(T a, T b) => default!;
    public static T dot<T>(T a, T b) => default!;
    public static T normalize<T>(T v, f32 l) => default!;
    public static T fma<T>(T a, f32 b, f32 c) => default!;*/
    public static T[] array<T>(params T[] value) => default!;
}

public static class StrExt
{
    public static void _raw(this string s)
    {
    }

    public static T _raw<T>(this string s) => default!;
}
