we wanna generate webgpu shader from c#, for this we use a source generator, it can use the visitor to visit a c# class and generate a shader code string out of it.
Next it generates a partial imple for this calss, containing a const string with the shader code.
As the shader contains binding information, and the c# synatx tree provides information over where the binding is used we can alsow generate the bind group layouts and provide methods for creating the bindings, alswell as the vertex layout and the vertex buffer layout.


// temple shader in c# anoteted
```csharp
using Generators;
using GlmSharp;

namespace WgslGenerator.Sample;

using static WgslGenerator.BuildIn;
using static GlmSharp.glm;

/*
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

 */
[GenerateShader]
public partial class TempleShader
{
    public partial record struct UbScene(
        mat4 ViewProjection,
        vec3 CameraPosition,
        float Time
    );

    public partial record struct UbModel(
        mat4 Transform,
        vec4 Tex0Color,
        uint ModelId,
        vec3 Tex1Color,
        float NormalMapStrength
    );

    public partial record struct Attributes(
        [Location(0)] vec3 Position,
        [Location(1)] vec2 Tex0Coord,
        [Location(2)] vec2 Tex1Coord,
        [Location(3)] vec3 Normal,
        [Location(4)] vec3 Tangent
    );

    public partial record struct Instance(
        [Location(5)] vec4 Transform0,
        [Location(6)] vec4 Transform1,
        [Location(7)] vec4 Transform2,
        [Location(8)] vec4 Transform3,
        [Location(9)] vec4 HighlightColor
    );

    public partial record struct Varyings(
        [Builtin("position")] vec4 Position,
        [Location(0)] vec2 Tex0Coord,
        [Location(1)] vec2 Tex1Coord,
        [Location(2)] vec3 Normal,
        [Location(3)] vec3 Tangent,
        [Location(4)] vec3 Bitangent,
        [Location(5)] uint MeshId,
        [Location(6)] uint InstanceId,
        [Location(7)] vec4 HighlightColor
    );

    public partial record struct ColorTargets(
        [Location(0)] vec4 Albedo,
        [Location(1)] vec4 Normal,
        [Location(2)] vec4 Emission
    );

    [Group(0), Binding(0), Access(ShaderAccess.Uniform)]
    private UbScene ubScene;

    [Group(1), Binding(0), Access(ShaderAccess.Uniform)]
    private UbModel ubModel;

    [Group(1)] [Binding(1)] private sampler uSampler;
    [Group(1)] [Binding(2)] private texture_2d<float> uAlbedo;
    [Group(1)] [Binding(3)] private texture_2d<float> uNormal;
    [Group(1)] [Binding(4)] private texture_2d<float> uEmission;

    [VertexShader]
    public Varyings vs_main(Attributes a, Instance i, [Builtin("instance_index")] uint iId)
    {
        // Build the instance transform matrix from the instance attributes
        var instanceTransform = new mat4(
            i.Transform0,
            i.Transform1,
            i.Transform2,
            i.Transform3
        );

        // Combine the model transform with the instance transform
        var modelTransform = ubModel.Transform * instanceTransform;

        var pos = new vec4(a.Position, 1.0f);
        var worldPos = modelTransform * pos;
        var outPos = ubScene.ViewProjection * worldPos;

        // For normal and tangent, extract the rotation part from the combined transform.
        var nrmTransform = new mat3(
            /*modelTransform[0].xyz,
            modelTransform[1].xyz,
            modelTransform[2].xyz*/
            modelTransform // dose this work?
        );

        var nrmTransform2 = "mat3x3(modelTransform[0].xyz, modelTransform[1].xyz, modelTransform[2].xyz);"._raw<mat3>();

        var nrm = normalize(nrmTransform * a.Normal);
        var tan = normalize(nrmTransform * a.Tangent);
        var bitan = cross(tan, nrm);

        return new Varyings(
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
}
```

// generated shader
```wgsl
//<auto-generated>

struct UbScene {
    ViewProjection: mat4x4<f32>;
    CameraPosition: vec3<f32>;
    Time: f32;
}

struct UbModel {
    Transform: mat4x4<f32>;
    Tex0Color: vec4<f32>;
    ModelId: u32;
    Tex1Color: vec3<f32>;
    NormalMapStrength: f32;
}

struct Attributes {
    @location(0) 
    Position: vec3<f32>;
    @location(1) 
    Tex0Coord: vec2<f32>;
    @location(2) 
    Tex1Coord: vec2<f32>;
    @location(3) 
    Normal: vec3<f32>;
    @location(4) 
    Tangent: vec3<f32>;
}

struct Instance {
    @location(5) 
    Transform0: vec4<f32>;
    @location(6) 
    Transform1: vec4<f32>;
    @location(7) 
    Transform2: vec4<f32>;
    @location(8) 
    Transform3: vec4<f32>;
    @location(9) 
    HighlightColor: vec4<f32>;
}

struct Varyings {
    @builtin(position) 
    Position: vec4<f32>;
    @location(0) 
    Tex0Coord: vec2<f32>;
    @location(1) 
    Tex1Coord: vec2<f32>;
    @location(2) 
    Normal: vec3<f32>;
    @location(3) 
    Tangent: vec3<f32>;
    @location(4) 
    Bitangent: vec3<f32>;
    @location(5) 
    MeshId: u32;
    @location(6) 
    InstanceId: u32;
    @location(7) 
    HighlightColor: vec4<f32>;
}

struct ColorTargets {
    @location(0) 
    Albedo: vec4<f32>;
    @location(1) 
    Normal: vec4<f32>;
    @location(2) 
    Emission: vec4<f32>;
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

@vertex 
fn vs_main(a: Attributes, 
        i: Instance, 
        @builtin(instance_index) iId: u32) -> Varyings
{
    let instanceTransform = mat4x4<f32>(i.Transform0, i.Transform1, i.Transform2, i.Transform3);
    let modelTransform = ubModel.Transform * instanceTransform;
    let pos = vec4<f32>(a.Position, 1.0f);
    let worldPos = modelTransform * pos;
    let outPos = ubScene.ViewProjection * worldPos;
    let nrmTransform = mat3x3<f32>(modelTransform);
    let nrmTransform2 = ;
    let nrm = normalize(nrmTransform * a.Normal);
    let tan = normalize(nrmTransform * a.Tangent);
    let bitan = cross(tan, nrm);
    return Varyings(outPos, a.Tex0Coord, a.Tex1Coord, nrm, tan, bitan, ubModel.ModelId, iId, i.HighlightColor);
}
```

// generated c# code
```csharp
//<auto-generated>
public partial class TempleShader
{
    public const string ShaderSource = """{{{shaderSource}}}""";
}
```

we are missing the generation of the bind group layout and the vertex layout, and the vertex buffer layout, and the method for creating the bindings.
example for binding are:

```csharp
var sceneBindGroupLayout = _api.Device.CreateBindGroupLayout(
    [Buffer(0, ShaderStage.Vertex, BufferBindingType.Uniform, (u32)Marshal.SizeOf<SceneUniform>())],
    label: "SceneBindGroupLayout"
);

_modelBindGroupLayout = _api.Device.CreateBindGroupLayout(
    [
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        Buffer(0, ShaderStage.Vertex | ShaderStage.Fragment, BufferBindingType.Uniform,
            (u64)Unsafe.SizeOf<ModelUniform>()),
        Sampler(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
        Texture(2, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
            multisampled: false),
        Texture(3, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
            multisampled: false),
        Texture(4, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
            multisampled: false)
    ],
    label: "ModelBindGroupLayout"
);

var pipelineLayout = _api.Device.CreatePipelineLayout(
    new[]
    {
        sceneBindGroupLayout,
        _modelBindGroupLayout
    },
    label: "PipelineLayout"
);

_sceneBindGroup = _api.Device.CreateBindGroup(
    bindGroupLayout: sceneBindGroupLayout,
    [Buffer(0, _sceneUniformBuffer.Buffer, _sceneUniformBuffer.Offset, _sceneUniformBuffer.Size)],
    label: "SceneBindGroup"
);

_modelPipeline = _api.Device.CreateRenderPipeline(
label: "DefaultPipeline",
layout: pipelineLayout,
vertex: new VertexState
{
    Module = shaderModule,
    EntryPoint = "vs_main",
    Constants = [],
    Buffers =
    [
        // vertex buffer
        new VertexBufferLayout(
            (u32)Marshal.SizeOf<Vertex>(), VertexStepMode.Vertex,
            [
                new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 0, offset:
                    (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Position))),
                new VertexAttribute(VertexFormat.Float32x2, shaderLocation: 1, offset:
                    (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord0))),
                new VertexAttribute(VertexFormat.Float32x2, shaderLocation: 2, offset:
                    (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord1))),
                new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 3, offset:
                    (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal))),
                new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 4, offset:
                    (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Tangent))),
            ]
        ),
        // instance buffer
        new VertexBufferLayout(
            (u32)Marshal.SizeOf<Instance>(), VertexStepMode.Instance,
            [
                new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 5, offset:
                    (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform0))),
                new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 6, offset:
                    (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform1))),
                new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 7, offset:
                    (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform2))),
                new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 8, offset:
                    (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform3))),
                new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 9, offset:
                    (u32)Marshal.OffsetOf<Instance>(nameof(Instance.HighlightColor))),
            ]
        )
    ]
},
primitive: new PrimitiveState
{
    Topology = PrimitiveTopology.TriangleList,
    FrontFace = FrontFace.Ccw,
    CullMode = CullMode.Back
},
depthStencil: new DepthStencilState
{
    Format = _sceneGBuffer.DepthStencilFormat,
    DepthCompare = CompareFunction.LessEqual,
    DepthWriteEnabled = true,
    StencilFront = new StencilFaceState { Compare = CompareFunction.Always },
    StencilBack = new StencilFaceState { Compare = CompareFunction.Always },
},
multisample: new MultisampleState
{
    Count = 1,
    AlphaToCoverageEnabled = false,
    Mask = u32.MaxValue
},
fragment: new FragmentState
{
    Module = shaderModule,
    EntryPoint = "fs_main",
    Constants = [],
    Targets =
    [
        new ColorTargetState(
            _sceneGBuffer.AlbedoChannelFormat,
            blendState: null,
            ColorWriteMask.All
        ),
        new ColorTargetState(
            _sceneGBuffer.NormalChannelFormat,
            blendState: null,
            ColorWriteMask.All
        ),
        new ColorTargetState(
            _sceneGBuffer.LightChannelFormat,
            blendState: null,
            ColorWriteMask.All
        )
    ]
}
);

private void RecreateBindGroup(InstancedMeshBuffer<ModelUniform, Instance> instancedMeshBuffer)
{
    instancedMeshBuffer.ModelBindGroup?.Release();
    instancedMeshBuffer.ModelBindGroup = _api.Device.CreateBindGroup(
        bindGroupLayout: _modelBindGroupLayout,
        [
            Buffer(0, instancedMeshBuffer.ModelUniformBuffer.Buffer, instancedMeshBuffer.ModelUniformBuffer.Offset,
                instancedMeshBuffer.ModelUniformBuffer.Size),
            Sampler(1, _linearSampler),
            Texture(2, instancedMeshBuffer.AlbedoTexture.View),
            Texture(3, instancedMeshBuffer.NormalTexture.View),
            Texture(4, instancedMeshBuffer.EmissionTexture.View),
        ],
        label: "ModelBindGroup"
    );
}
// some rendering
var pass = cmd.BeginRenderPass([
    new RenderPassColorAttachment(albedo.View, resolveTarget: null,
        LoadOp.Clear, StoreOp.Store, new Color(.06, .1, .1, 1)),
    new RenderPassColorAttachment(normal.View, resolveTarget: null,
        LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 1)),
    new RenderPassColorAttachment(emission.View, resolveTarget: null,
        LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0.1, 1))
],
null,
new RenderPassDepthStencilAttachment(depthStencil.View, LoadOp.Clear, StoreOp.Store, 1f, false,
    /*stencil*/ LoadOp.Clear, StoreOp.Discard, 0, true),
null);

pass.SetPipeline(_modelPipeline);

foreach (var (vertexBuffer, indexBuffer, instanceBuffer, modelBindGroup, instances) in _meshBuffers)
{
    pass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Size);
    pass.SetVertexBuffer(1, instanceBuffer!.Value.Buffer, instanceBuffer.Value.Offset,
        instanceBuffer.Value.Size);

    pass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.Offset, indexBuffer.Size);

    pass.SetBindGroup(0, _sceneBindGroup, ReadOnlySpan<u32>.Empty);
    pass.SetBindGroup(1, modelBindGroup!.Value, ReadOnlySpan<u32>.Empty);

    pass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Length, 0, 0, 0);
}
```

The user shoudle manage the actual textures buffers and samplers themself, we should just provide methods that take take only the variing arguments (e.g. multisampled, as format and dimension and attatchment are known by analyzing the cs class) and create the bindings for the user.
