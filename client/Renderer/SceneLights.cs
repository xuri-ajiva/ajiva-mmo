using System.Numerics;

namespace Client.Renderer;

internal class SceneLights
{
    public u32 LightCount => (u32)_lightData.Length;
    public ReadOnlySpan<DeferredShader.LightData> LightData => _lightData;

    private f32 _time;

    private readonly DeferredShader.LightData[] _lightData =
        new DeferredShader.LightData[8];

    public SceneLights() => Update();

    private void Update()
    {
        var a = MathF.Sin(_time) * 0.5f + 0.5f;
        var b = MathF.Cos(_time) * 0.5f + 0.5f;
        var c = -MathF.Sin(_time) * 0.5f + 0.5f;
        var d = -MathF.Cos(_time) * 0.5f + 0.5f;

        var i = 0;

        const f32 circleRadius = 4;
        var (sin, cos) = MathF.SinCos(_time + MathF.Tau * 0 / 4f);
        _lightData[i++] = Light(
            x: circleRadius * sin, z: circleRadius * cos,
            y: 0.5f + a * 4,
            col: (1, 0.5f, 0),
            intensity: 3, radius: 10
        );
        (sin, cos) = MathF.SinCos(_time + MathF.Tau * 1 / 4f);
        _lightData[i++] = Light(
            x: circleRadius * sin, z: circleRadius * cos,
            y: 0.5f + b * 4,
            col: (0, 0.5f, 1),
            intensity: 3, radius: 10
        );
        (sin, cos) = MathF.SinCos(_time + MathF.Tau * 2 / 4f);
        _lightData[i++] = Light(
            x: circleRadius * sin, z: circleRadius * cos,
            y: 0.5f + c * 4,
            col: (0.5f, 0, 1),
            intensity: 3, radius: 10
        );
        (sin, cos) = MathF.SinCos(_time + MathF.Tau * 3 / 4f);
        _lightData[i++] = Light(
            x: circleRadius * sin, z: circleRadius * cos,
            y: 0.5f + d * 4,
            col: (0, 1, 0.5f),
            intensity: 3, radius: 10
        );

        _lightData[i++] = Light(
            x: -7.5f, y: 0, z: -6,
            col: (1, 1, 1),
            intensity: 2, radius: 10
        );
        _lightData[i++] = Light(
            x: 7.5f, y: 0, z: -6,
            col: (1, 1, 1),
            intensity: 2, radius: 10
        );
        _lightData[i++] = Light(
            x: -7.5f, y: 0, z: 6,
            col: (1, 1, 1),
            intensity: 2, radius: 10
        );
        _lightData[i++] = Light(
            x: 7.5f, y: 0, z: 6,
            col: (1, 1, 1),
            intensity: 2, radius: 10
        );
        return;

        static DeferredShader.LightData Light(f32 x, f32 y, f32 z, (f32 r, f32 g, f32 b) col,
            f32 intensity, f32 radius)
            => new()
            {
                Position = new Vector4(x, y, z, 1),
                Color = new Vector3(col.r, col.g, col.b) * intensity,
                Radius = radius
            };
    }

    public void Animate(f32 deltaTime)
    {
        _time += deltaTime;
        Update();
    }
}
