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
    InverseViewProjection: mat4x4<f32>,
    LightCount: u32,
}

struct LightData {
  position : vec4<f32>,
  color : vec3<f32>,
  radius : f32,
}
struct LightsBuffer {
  lights: array<LightData>,
}

@group(0) @binding(0)
var<uniform> ub: Uniforms;
@group(0) @binding(1)
var<storage, read> lightsBuffer: LightsBuffer;
@group(0) @binding(2)
var uAlbedo: texture_2d<f32>;
@group(0) @binding(3)
var uNormal: texture_2d<f32>;
@group(0) @binding(4)
var uDepth: texture_depth_2d;

@fragment
fn fs_main(v: Varyings) -> @location(0) vec4<f32> {
    let albedo = textureLoad(uAlbedo, vec2<i32>(floor(v.Position.xy)), 0).rgb;
    let normal = textureLoad(uNormal, vec2<i32>(floor(v.Position.xy)), 0).xyz;
    let depth = textureLoad(uDepth, vec2<i32>(floor(v.Position.xy)), 0);

    let nrm = normal.xyz * 2.0 - vec3(1.0);
    let _position = ub.InverseViewProjection * 
        vec4(v.TexCoord*2.0-vec2(1.0), depth, 1.0);
    let position = _position.xyz/_position.w;

    var result = vec3(0.0);

    for (var i = 0u; i < ub.LightCount; i++) {
      let L = lightsBuffer.lights[i].position.xyz - position;
      let distance = length(L);
      if (distance > lightsBuffer.lights[i].radius) {
        continue;
      }
      let lambert = max(dot(nrm, normalize(L)), 0.0);
      result += vec3<f32>(
        lambert * pow(1.0 - distance / lightsBuffer.lights[i].radius, 2.0) * lightsBuffer.lights[i].color * albedo
      );
    }

    // some manual ambient
    result += vec3(0.1);

    if (depth == 1.0) {
        return vec4(albedo, 1.0);
    } else {
        return vec4(result, 1.0);
    }
}
