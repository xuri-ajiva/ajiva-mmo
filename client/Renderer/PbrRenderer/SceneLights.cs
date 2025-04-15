using GlmSharp;

namespace Client.Renderer.PbrRenderer;

using static Client.Wgsl.PbrDeferredLightingShader;

internal class SceneLights
{
    public u32 LightCount => (u32)_lightData.Length;
    public ReadOnlySpan<LightData> LightData => _lightData;

    private f32 _time;

    private readonly LightData[] _lightData = new LightData[10];

    public SceneLights() => Update();

    private void Update()
    {
        var a = MathF.Sin(_time) * 0.5f + 0.5f;
        var b = MathF.Cos(_time) * 0.5f + 0.5f;
        var c = -MathF.Sin(_time) * 0.5f + 0.5f;
        var d = -MathF.Cos(_time) * 0.5f + 0.5f;

        var i = 0;
        const f32 circleRadius = 4;
        /*
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
        );*/


        ReadOnlySpan<LightData> lights =
        [
            // sun
            new(glm.Normalized(new vec3(-0.5f, -1.0f, -0.3f)), 0, new vec3(1.0f, 0.95f, 0.9f) * 2f, 0.0f),

            // dynamic lights (n in [0, 1, 2, 3])
            CircleLight(_time, 0, 4),
            CircleLight(_time, 1, 4),
            CircleLight(_time, 2, 4),
            CircleLight(_time, 3, 4),

            // static lights
            Light(x: -7.5f, y: 0, z: -6, col: (1, 1, 1), intensity: 2, radius: 10),
            Light(x: 7.5f, y: 0, z: -6, col: (1, 1, 1), intensity: 2, radius: 10),
            Light(x: -7.5f, y: 0, z: 6, col: (1, 1, 1), intensity: 2, radius: 10),
            Light(x: 7.5f, y: 0, z: 6, col: (1, 1, 1), intensity: 2, radius: 10),

            // ambient light
            Light(x: 0, y: 0, z: 0, col: (1, 0.3f, 0), intensity: 200f, radius: 100f),
        ];

        lights.CopyTo(_lightData);
    }

    private static LightData CircleLight(f32 time, i32 light, i32 numLights)
    {
        var (sin, cos) = MathF.SinCos(time * 1f + MathF.Tau * light / numLights);
        const f32 circleRadius = 4;
        return Light(
            x: circleRadius * sin, z: circleRadius * cos,
            y: 1f + MathF.Sin(time),
            col: (1, 0.5f, 0),
            intensity: 3, radius: 10
        );
    }

    private static LightData Light(f32 x, f32 y, f32 z, (f32 r, f32 g, f32 b) col,
        f32 intensity, f32 radius)
        => new()
        {
            Position = new vec3(x, y, z),
            Type = 1,
            Color = new vec3(col.r, col.g, col.b) * intensity,
            Radius = radius
        };

    public void Animate(f32 deltaTime)
    {
        _time += deltaTime;
        Update();
    }
}
