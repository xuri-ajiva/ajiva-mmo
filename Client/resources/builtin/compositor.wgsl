struct Varyings {
    @builtin(position) Position: vec4<f32>,
    @location(0) TexCoord: vec2<f32>,
}

@vertex
fn vs_main(@builtin(vertex_index) vertex_index : u32) -> Varyings {
    let a = vec2<f32>(-1.0,  1.0);
    let b = vec2<f32>( 1.0,  1.0);
    let c = vec2<f32>(-1.0, -1.0);
    let d = vec2<f32>( 1.0, -1.0);

    var pos = array(
        b, a, c,
        b, c, d
    );

    return Varyings(
        vec4(pos[vertex_index], 0.0, 1.0),
        pos[vertex_index] * 0.5 + vec2<f32>(0.5)
    );
}

struct Uniforms {
    Gamma: f32
}

@group(0) @binding(0)
var<uniform> ub: Uniforms;
@group(0) @binding(1)
var uColor: texture_2d<f32>;
@group(0) @binding(4)
var uDepth: texture_depth_2d;

@fragment
fn fs_main(v: Varyings) -> @location(0) vec4<f32> {
    let color = textureLoad(uColor, vec2<i32>(floor(v.Position.xy)), 0);
    let depth = textureLoad(uDepth, vec2<i32>(floor(v.Position.xy)), 0);

    let checker = (fract(v.Position.x/20.0)<0.5) != (fract(v.Position.y/20.0)<0.5);

    let checkerValue = select(0.0, 0.25, checker);

    var oColor = color;
    return oColor;
}
