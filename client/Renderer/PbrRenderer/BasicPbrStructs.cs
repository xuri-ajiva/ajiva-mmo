using System.Numerics;

namespace client.Renderer.PbrRenderer;

public struct Vertex
{
    public Vector3 Position;
    public Vector2 TexCoord0;
    public Vector2 TexCoord1;
    public Vector3 Normal;
    public Vector3 Tangent;
}

public struct ModelUniform
{
    public Matrix4x4 Transform;
    public Vector3 Tex0Color;
    public u32 ModelId;
    public Vector3 Tex1Color;
    public f32 NormalMapStrength;
}

public struct SceneUniform
{
    public Matrix4x4 ViewProjection;
    public Vector3 CameraPosition;
    public f32 Time;
}

public struct Instance
{
    // public u32 ObjID; // indirect over instance_index
    public Vector4 Transform0;
    public Vector4 Transform1;
    public Vector4 Transform2;
    public Vector4 Transform3;
    public Vector4 HighlightColor;

    public void SetTransform(Matrix4x4 transform)
    {
        Transform0 = new Vector4(transform.M11, transform.M12, transform.M13, transform.M14);
        Transform1 = new Vector4(transform.M21, transform.M22, transform.M23, transform.M24);
        Transform2 = new Vector4(transform.M31, transform.M32, transform.M33, transform.M34);
        Transform3 = new Vector4(transform.M41, transform.M42, transform.M43, transform.M44);
    }
}
