using System.Numerics;
using System.Runtime.CompilerServices;

namespace Common.Helpers;

public class MathHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static f64 ToRadians(f64 degree) => degree * (Math.PI / 180.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static f64 ToDegrees(f64 radians) => radians * (180.0 / Math.PI);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static f32 ToRadians(f32 degree) => degree * (MathF.PI / 180.0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static f32 ToDegrees(f32 radians) => radians * (180.0f / MathF.PI);
}