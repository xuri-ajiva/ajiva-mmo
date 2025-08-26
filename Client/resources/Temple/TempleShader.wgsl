struct UbScene {
    ViewProjection: mat4x4<f32>,
    CameraPosition: vec3<f32>,
    Time: f32,
}

struct UbModel {
    Transform: mat4x4<f32>,
    Tex0Color: vec3<f32>,
    ModelId: u32,
    Tex1Color: vec3<f32>,
    NormalMapStrength: f32,
}

@group(0) @binding(0)
var<uniform> ubScene: UbScene;

@group(1) @binding(0)
var<uniform> ubModel: UbModel;
@group(1) @binding(1)
var uSampler: sampler;
@group(1) @binding(2)
var uAlbedo: texture_2d<f32>;
@group(1) @binding(3)
var uNormal: texture_2d<f32>;
@group(1) @binding(4)
var uEmission: texture_2d<f32>;

struct Attributes {
    @location(0) Position: vec3<f32>,
    @location(1) Tex0Coord: vec2<f32>,
    @location(2) Tex1Coord: vec2<f32>,
    @location(3) Normal: vec3<f32>,
    @location(4) Tangent: vec3<f32>,
}

struct Instance {
    @location(5) Transform0: vec4<f32>,
    @location(6) Transform1: vec4<f32>,
    @location(7) Transform2: vec4<f32>,
    @location(8) Transform3: vec4<f32>,
    @location(9) HighlightColor: vec4<f32>
}

struct Varyings {
    @builtin(position) Position: vec4<f32>,
    @location(0) Tex0Coord: vec2<f32>,
    @location(1) Tex1Coord: vec2<f32>,
    
    @location(2) Normal: vec3<f32>,
    @location(3) Tangent: vec3<f32>,
    @location(4) Bitangent: vec3<f32>,
    
    @location(5) MeshId: u32,
    @location(6) InstanceId: u32,
    @location(7) HighlightColor: vec4<f32>,
}

struct ColorTargets {
    @location(0) Albedo: vec4<f32>,
    @location(1) Normal: vec4<f32>,
    @location(2) Emission: vec4<f32>,
}

@vertex
fn vs_main(
    a: Attributes,
    i: Instance,
    @builtin(instance_index) iId: u32
) -> Varyings {
    // Build the instance transform matrix from the instance attributes
    let instanceTransform = mat4x4<f32>(
        i.Transform0, 
        i.Transform1, 
        i.Transform2, 
        i.Transform3
    );
    
    // Combine the model transform with the instance transform
    let modelTransform = ubModel.Transform * instanceTransform;

    let pos = vec4<f32>(a.Position, 1.0);
    let worldPos = modelTransform * pos;
    let outPos = ubScene.ViewProjection * worldPos;

    // For normal and tangent, extract the rotation part from the combined transform.
    let nrmTransform = mat3x3(
        modelTransform[0].xyz,
        modelTransform[1].xyz,
        modelTransform[2].xyz
    );
    
    let nrm = normalize(nrmTransform * a.Normal);
    let tan = normalize(nrmTransform * a.Tangent);
    let bitan = cross(tan, nrm);

    return Varyings(
        outPos,
        a.Tex0Coord,
        a.Tex1Coord,
        nrm, 
        tan, 
        bitan,
        ubModel.ModelId, 
        iId, 
        i.HighlightColor
    );
}

@fragment
fn fs_main(v: Varyings) -> ColorTargets {
    let oAlbedo = textureSample(uAlbedo, uSampler, v.Tex0Coord) * vec4(ubModel.Tex0Color, 1.0);

    let normalMap = textureSample(uNormal, uSampler, v.Tex0Coord).xyz;
    let nrm = 
        fma(normalMap.r, 2.0, -1.0) * v.Tangent +
        fma(normalMap.g, 2.0, -1.0) * v.Bitangent +
        fma(normalMap.b, 2.0, -1.0) * v.Normal;

    let ambient = vec3(0.0, 0.0, 0.1);

    let emission = textureSample(uEmission, uSampler, v.Tex1Coord).xyz * ubModel.Tex1Color;

    let oNormal = vec4(mix(v.Normal, nrm, ubModel.NormalMapStrength) * 0.5 + vec3<f32>(0.5), 0.0);

    return ColorTargets(oAlbedo, oNormal, vec4(ambient+emission + v.HighlightColor.xyz, 1.0));
} 
