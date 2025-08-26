using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Client.Renderer.PbrRenderer;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Client.Wgsl;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace Client.Renderer.Resources;

using System;

public class ResourceManager(RendererApi api, ILogger<ResourceManager> logger, ModelHelper modelHelper)
    : IHostedLifecycleService
{
    public readonly ILogger<ResourceManager> Logger = logger;
    public readonly RendererApi Api = api;

    #region Textures

    private List<IAjivaVersionedObject> _internal = [];
    internal AjivaTexture DefaultTexture = null!;
    internal AjivaTexture DefaultNormal = null!;
    internal AjivaTexture DefaultRoughness = null!;
    internal AjivaTexture DefaultMetallicRoughnessAo = null!;
    internal AjivaTexture DefaultEmissive = null!;
    internal AjivaMesh<PbrGBufferShader.Attributes> DefaultPbrCubeMesh = null!;
    internal AjivaMesh<WboitAccumShader.Attributes> DefaultTransparentCubeMesh = null!;

    private Task InitDefaultResources()
    {
        var extent3D = new Extent3D(64, 64, 1);
        var tex = CreateTexture(TextureUsage.TextureBinding, "DefaultTexture", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);

        Span<Rgba32> data = stackalloc Rgba32[(i32)(extent3D.Width * extent3D.Height)];

        //CoolTextureGenerator.GenerateCoolPattern(data, (i32)extent3D.Width, (i32)extent3D.Height);
        ResourceGenerator.GenerateMissingTexture(data, (i32)extent3D.Width, (i32)extent3D.Height);

        tex.WriteTexture<Rgba32>(data, extent3D);
        var defaultTexture = AjivaTexture.FromData(tex);

        var normalTex = CreateTexture(TextureUsage.TextureBinding, "DefaultNormal", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        ResourceGenerator.GenerateNormalMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        normalTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultNormal = AjivaTexture.FromData(normalTex);

        var roughnessTex = CreateTexture(TextureUsage.TextureBinding, "DefaultRoughness", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        ResourceGenerator.GenerateRoughnessMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        roughnessTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultRoughness = AjivaTexture.FromData(roughnessTex);

        var emissiveTex = CreateTexture(TextureUsage.TextureBinding, "DefaultEmissive", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        ResourceGenerator.GenerateEmissiveMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        emissiveTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultEmissive = AjivaTexture.FromData(emissiveTex);

        var metallicRoughnessTex = CreateTexture(TextureUsage.TextureBinding, "DefaultMetallicRoughnessAO", 1,
            extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        ResourceGenerator.GenerateMetallicRoughnessAoMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        metallicRoughnessTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultMetallicRoughnessAo = AjivaTexture.FromData(metallicRoughnessTex);

        Span<PbrGBufferShader.Attributes> cubeVerticesPbr = stackalloc PbrGBufferShader.Attributes[6 * 4];
        Span<u32> indices = stackalloc u32[6 * 6];
        const f32 tex1Scale = 0;
        ResourceGenerator.GenerateCubeMesh(cubeVerticesPbr, indices, (u, rot, normal, tangent) =>
        {
            ReadOnlySpan<Vector3D<f32>> positions = [new(-1, 1, 1), new(1, 1, 1), new(-1, -1, 1), new(1, -1, 1)];
            ReadOnlySpan<Vector2> tex0Coords = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];
            ReadOnlySpan<Vector2> tex1Coords =
            [
                new(tex1Scale, tex1Scale), new(1 - tex1Scale, tex1Scale), new(tex1Scale, 1 - tex1Scale),
                new(1 - tex1Scale, 1 - tex1Scale)
            ];
            return new PbrGBufferShader.Attributes
            {
                PositionRef = Vector3D.Transform(positions[u], rot).ToSystem(),
                Tex0CoordRef = tex0Coords[u],
                Tex1CoordRef = tex1Coords[u],
                NormalRef = normal,
                TangentRef = new Vector4(tangent, 1)
            };
        });
        var defaultPbrCubeMesh = new AjivaMesh<PbrGBufferShader.Attributes>(
            CreateMesh<PbrGBufferShader.Attributes>(nameof(DefaultPbrCubeMesh), cubeVerticesPbr, indices));

        Span<WboitAccumShader.Attributes> cubeVerticesWboit = stackalloc WboitAccumShader.Attributes[6 * 4];
        ResourceGenerator.GenerateCubeMesh(cubeVerticesWboit, indices, (u, rot, _, _) =>
        {
            ReadOnlySpan<Vector3D<f32>> positions =
                [new(-1, 1, 1), new(1, 1, 1), new(-1, -1, 1), new(1, -1, 1)];
            ReadOnlySpan<Vector2> tex0Coords = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];
            return new WboitAccumShader.Attributes
            {
                PositionRef = Vector3D.Transform(positions[u], rot).ToSystem(),
                Tex0CoordRef = tex0Coords[u],
            };
        });
        var defaultTransparentCubeMesh = new AjivaMesh<WboitAccumShader.Attributes>(
            CreateMesh<WboitAccumShader.Attributes>(nameof(DefaultTransparentCubeMesh), cubeVerticesWboit, indices));

        _internal =
        [
            defaultTexture,
            defaultNormal,
            defaultRoughness,
            defaultMetallicRoughnessAo,
            defaultEmissive,
            defaultTransparentCubeMesh,
            defaultPbrCubeMesh
        ];
        DefaultTexture = defaultTexture.Clone("defensive_copy");
        DefaultNormal = defaultNormal.Clone("defensive_copy");
        DefaultRoughness = defaultRoughness.Clone("defensive_copy");
        DefaultEmissive = defaultEmissive.Clone("defensive_copy");
        DefaultMetallicRoughnessAo = defaultMetallicRoughnessAo.Clone("defensive_copy");
        DefaultPbrCubeMesh = defaultPbrCubeMesh.Clone("defensive_copy");
        DefaultTransparentCubeMesh = defaultTransparentCubeMesh.Clone("defensive_copy");


        return Task.CompletedTask;
    }

    private readonly SemaphoreSlim _loadSem = new(1, 100);
    private readonly ConcurrentDictionary<string, AjivaTexture> _loadedTextures = new();
    private readonly ConcurrentDictionary<string, AjivaMesh<PbrGBufferShader.Attributes>> _loadedMeshes = new();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_currentCount")]
    static extern ref i32 SemaphoreSlim_currentCount(SemaphoreSlim semaphore);

    public void UpdateMaxConcurrentLoads(i32 maxConcurrentLoads)
    {
        if(maxConcurrentLoads > 100)
        {
            Logger.LogWarning("Max concurrent loads set to {MaxConcurrentLoads}, which is higher than the default of 100. This may lead to performance issues.", maxConcurrentLoads);
            maxConcurrentLoads = 100;
        }
        ref var max = ref SemaphoreSlim_currentCount(_loadSem);
        var diff = maxConcurrentLoads - max;
        if (diff > 0)
        {
            _loadSem.Release(diff);
            Logger.LogInformation("Increased max concurrent loads to {MaxConcurrentLoads}", maxConcurrentLoads);
        }
        else if (diff < 0)
        {
            for (var i = 0; i < -diff; i++)
            {
                _loadSem.Wait();
            }
            Logger.LogInformation("Decreased max concurrent loads to {MaxConcurrentLoads}", maxConcurrentLoads);
        }
        else
        {
            Logger.LogInformation("Max concurrent loads already set to {MaxConcurrentLoads}", maxConcurrentLoads);
        }
    }

    public List<AjivaTexture> GetAllTextures()
    {
        var res = new List<AjivaTexture>(_loadedTextures.Count);
        foreach (var (key, texture) in _loadedTextures.ToArray())
        {
            if (!texture.IsDisposed)
            {
                res.Add(texture);
            }
            else
            {
                _loadedTextures.TryRemove(key, out _);
            }
        }

        return res;
    }

    public AjivaMesh<PbrGBufferShader.Attributes>[] GetAllMeshes()
    {
        var res = new List<AjivaMesh<PbrGBufferShader.Attributes>>(_loadedMeshes.Count);
        foreach (var (key, mesh) in _loadedMeshes.ToArray())
        {
            if (!mesh.IsDisposed)
            {
                res.Add(mesh);
            }
            else
            {
                _loadedMeshes.TryRemove(key, out _);
            }
        }

        return res.ToArray();
    }

    public void AsyncTextureLoad(string path, AjivaTexture target,
        TextureUsage usage = TextureUsage.TextureBinding, string? label = null, u32 mipLevels = 8,
        TextureFormat format = TextureFormat.Rgba8Unorm, TextureAspect aspect = TextureAspect.All)
    {
        start:
        if (!_loadedTextures.TryAdd(path, target))
        {
            var val = _loadedTextures[path];
            if (val.IsDisposed)
            {
                _loadedTextures.TryRemove(path, out _);
                goto start;
            }

            target.Assign(val);
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                await _loadSem.WaitAsync();
                await LoadTextureAndMips(path, target.Data, usage, label ?? Path.GetFileNameWithoutExtension(path),
                    mipLevels, format, aspect);
            }
            catch (Exception e)
            {
                //TODO eneable for production Logger.LogCritical(e, "Failed to load texture {Path}", path);
            }
            finally
            {
                _loadSem.Release();
            }
        });
    }

    public AjivaTexture AsyncTextureLoad(string path, TextureUsage usage = TextureUsage.TextureBinding,
        string? label = null, u32 mipLevels = 8, TextureFormat format = TextureFormat.Rgba8Unorm,
        TextureAspect aspect = TextureAspect.All)
    {
        var result = DefaultTexture.Clone(label ?? Path.GetFileNameWithoutExtension(path));
        AsyncTextureLoad(path, result, usage, label, mipLevels, format, aspect);
        return result;
    }

    private async Task LoadTextureAndMips(string path, TextureData target, TextureUsage usage, string label,
        u32 requestedMipLevels, TextureFormat format, TextureAspect aspect)
    {
        await using var data = new FileStream(FindFile(path), FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var originalImage = Image.Load<Rgba32>(data);

        var extent3D = new Extent3D((u32)originalImage.Width, (u32)originalImage.Height, 1);

        var maxPossibleMipLevels = CalculateMaxMipLevels(extent3D);
        var effectiveMipLevels = Math.Max(1u, Math.Min(requestedMipLevels, maxPossibleMipLevels));

        var tmpTexture = CreateTexture(usage | TextureUsage.CopyDst, label, effectiveMipLevels, extent3D, format,
            aspect);
        target.Swap(tmpTexture);

        var currentLevelImage = originalImage;

        try
        {
            for (u32 mipLevel = 0; mipLevel < effectiveMipLevels; ++mipLevel)
            {
                Image<Rgba32> imageToUpload;
                if (mipLevel != 0)
                {
                    var image = currentLevelImage;
                    var nextLevelImage = currentLevelImage.Clone(ctx => ctx.Resize(
                        Math.Max(1, image.Width / 2),
                        Math.Max(1, image.Height / 2),
                        KnownResamplers.Triangle));

                    if (mipLevel > 1)
                    {
                        currentLevelImage.Dispose();
                    }

                    currentLevelImage = nextLevelImage;
                    imageToUpload = currentLevelImage;
                }
                else
                {
                    imageToUpload = originalImage;
                }

                var sizePixels = imageToUpload.Width * imageToUpload.Height; // Size in pixels

                var rent = ArrayPool<Rgba32>.Shared.Rent(sizePixels);
                try
                {
                    imageToUpload.CopyPixelDataTo(rent.AsSpan(0, sizePixels));
                    target.WriteTexture<Rgba32>(rent.AsSpan(0, sizePixels),
                        new Extent3D((u32)imageToUpload.Width, (u32)imageToUpload.Height, 1), mipLevel: mipLevel);
                    Logger.LogTrace("Uploaded mip level {MipLevel} ({MipWidth}x{MipHeight}) for '{Label}'.", mipLevel,
                        imageToUpload.Width, imageToUpload.Height, label ?? path);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing/uploading mip level {MipLevel} for '{Label}'.", mipLevel,
                        label ?? path);
                    break;
                }
                finally
                {
                    ArrayPool<Rgba32>.Shared.Return(rent);
                }
            }
        }
        finally
        {
            if (effectiveMipLevels > 1)
            {
                currentLevelImage.Dispose();
            }
        }

        Logger.LogDebug("Successfully loaded texture '{Label}' with {EffectiveMipLevels} mip levels.", label ?? path,
            effectiveMipLevels);
        DisposeLater(tmpTexture);
    }

    private string FindFile(string filePath)
    {
        // search for a given path, in the following order:
        // 1. filePath
        // 2. resources/filePath
        // 3. resources/**/filePath
        // 4. ../resources/**/filePath
        // 5. ../**/filePath
        if (File.Exists(filePath))
        {
            return filePath;
        }

        var resourcesFile = Path.Combine("resources", filePath);

        if (File.Exists(resourcesFile))
        {
            return resourcesFile;
        }

        var pattern = Path.GetFileName(filePath);
        var directory = Path.GetDirectoryName(filePath);
        var join = Path.Join(directory ?? string.Empty, pattern);

        bool Compare(string testAgainst)
        {
            return testAgainst.EndsWith(join, StringComparison.InvariantCultureIgnoreCase);
        }

        var errors = 0;
        try
        {
            if (Directory.EnumerateFiles("resources", pattern, SearchOption.AllDirectories)
                    .FirstOrDefault(Compare) is
                { } foundFile)
            {
                return foundFile;
            }
        }
        catch
        {
            errors++;
        }

        try
        {
            if (Directory.EnumerateFiles("../resources/", pattern, SearchOption.AllDirectories)
                    .FirstOrDefault(Compare) is
                { } foundFile2)
            {
                return foundFile2;
            }
        }
        catch
        {
            errors++;
        }

        try
        {
            if (Directory.EnumerateFiles("..", pattern, SearchOption.AllDirectories)
                    .FirstOrDefault(Compare) is
                { } foundFile3)
            {
                return foundFile3;
            }

            if (Directory.EnumerateFiles("..", pattern, SearchOption.AllDirectories).FirstOrDefault() is { } foundFile4)
            {
                return foundFile4;
            }
        }
        catch
        {
            errors++;
        }

        Logger.LogError(
            "Failed to find file '{FilePath}' in any of the searched locations. Errors: {Errors}, Checks: 5", filePath,
            errors);

        throw new FileNotFoundException($"File '{filePath}' not found in any of the searched locations.");
    }

    private static u32 CalculateMaxMipLevels(Extent3D extent3D) =>
        (u32)MathF.Floor(MathF.Log2(Math.Max(extent3D.Width, extent3D.Height))) + 1;

    private TextureData CreateTexture(TextureUsage usage, string label, u32 mipLevels, Extent3D extent3D,
        TextureFormat format, TextureAspect aspect)
    {
        var texture = Api.Device.CreateTexture(
            usage | TextureUsage.CopyDst,
            TextureDimension.Dimension2D,
            extent3D,
            format,
            mipLevelCount: mipLevels,
            sampleCount: 1,
            viewFormats: new[] { format },
            label: label);

        var textureView = texture.CreateView(format, TextureViewDimension.Dimension2D,
            aspect, baseMipLevel: 0, mipLevelCount: 1, baseArrayLayer: 0, arrayLayerCount: 1,
            label: $"{label} - View");

        Logger.LogDebug("Created texture {Label}", label);
        return new TextureData(this, label, texture, textureView, extent3D, aspect, mipLevels);
    }

    #endregion

    #region Buffer

    public AjivaBuffer<T> CreateBuffer<T>(BufferUsage usage, i32 count, string? label = null)
        where T : unmanaged
    {
        var result = new AjivaBuffer<T>(this, label, default, usage, 0, true);
        result.ResizeWithoutDataPreserve((u64)count);
        Logger.LogDebug("Created buffer {Label} with {Count} items ({Type}).", label, count, typeof(T).Name);
        return result;
    }

    public AjivaBuffer<T> CreateBuffer<T>(BufferUsage usage, ReadOnlySpan<T> data, string? label = null)
        where T : unmanaged
    {
        var result = new AjivaBuffer<T>(this, label, default, usage, 0, true);
        result.ResizeWithoutDataPreserve((u64)data.Length);

        // result.WriteBuffer(data);
        // Do not use queue here, as it is not guaranteed that the span is available after this method
        // Accutualy Unmap dose issue a queue submit, so this is not needed
        // w.r.t. that the map dose copy the data to a memory location that is guaranteed to be available
        using (var guard = Api.EnterQueue())
        {
            result.QueuedWrite(data, guard.Queue);
        }

        Logger.LogDebug("Created buffer {Label} with {Count} items ({Type}).", label, data.Length, typeof(T).Name);
        return result;
    }

    public BufferRange CreateBufferRaw(BufferUsage usage, u64 sizeInBytes, string? label) =>
        new(Api.Device.CreateBuffer(usage, sizeInBytes, mappedAtCreation: false, label), 0, sizeInBytes);

    private readonly Channel<IDisposable> _disposables =
        Channel.CreateUnbounded<IDisposable>(new UnboundedChannelOptions { SingleReader = true });

    #endregion

    #region Mesh

    public AjivaMesh<PbrGBufferShader.Attributes> AsyncMeshLoad(string path, ModelHelper.MeshData mdl,
        string? label = null)
    {
        var result = DefaultPbrCubeMesh.Clone(label ?? Path.GetFileNameWithoutExtension(path));
        AsyncMeshLoad(path, mdl, result, label);
        return result;
    }

    private void AsyncMeshLoad(string path, ModelHelper.MeshData mdl, AjivaMesh<PbrGBufferShader.Attributes> target,
        string? label = null)
    {
        var key = $"{path}_{label}_{mdl.Name}";
        start:
        if (!_loadedMeshes.TryAdd(key, target))
        {
            var val = _loadedMeshes[key];
            if (val.IsDisposed)
            {
                _loadedMeshes.TryRemove(key, out _);
                goto start;
            }

            target.Assign(val);
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                await _loadSem.WaitAsync();
                // todo array pool?
                var modelVertices = new PbrGBufferShader.Attributes[mdl.Vertices.Length];

                for (var iVertex = 0u; iVertex < mdl.Vertices.Length; iVertex++)
                {
                    modelVertices[iVertex].PositionRef = mdl.Vertices[iVertex];
                    modelVertices[iVertex].Tex0CoordRef = mdl.TexCoords0[iVertex];
                    modelVertices[iVertex].Tex1CoordRef = mdl.TexCoords1[iVertex];
                    modelVertices[iVertex].NormalRef = mdl.Normals[iVertex];
                    modelVertices[iVertex].TangentRef = new Vector4(mdl.Tangents[iVertex], 1.0f);
                }

                target.Assign(CreateMesh<PbrGBufferShader.Attributes>(
                    $"{label ?? Path.GetFileNameWithoutExtension(path)}::{mdl.Name}",
                    modelVertices, mdl.Indices));
                Logger.LogDebug("Loaded mesh {Label} with {VertexCount} vertices and {IndexCount} indices",
                    label ?? Path.GetFileNameWithoutExtension(path), mdl.Vertices.Length, mdl.Indices.Length);
            }
            catch (Exception e)
            {
                //TODO eneable for production Logger.LogCritical(e, "Failed to load texture {Path}", path);
            }
            finally
            {
                _loadSem.Release();
            }
        });
    }

    public MeshData<T> CreateMesh<T>(string label, ReadOnlySpan<T> modelVertices, ReadOnlySpan<u32> indices)
        where T : unmanaged
    {
        var vertexBuffer =
            CreateBuffer(BufferUsage.Vertex | BufferUsage.CopyDst, modelVertices, $"{label}::VertexBuffer");
        var indexBuffer = CreateBuffer(BufferUsage.Index | BufferUsage.CopyDst, indices, $"{label}::IndexBuffer");

        return new MeshData<T>(this, label, vertexBuffer, indexBuffer);
    }

    #endregion

    public void DisposeLater(IDisposable? disposable)
    {
        if (disposable is null) return;
        // skip textures that are not supposed to be cleaned up (clone of default textures)
        //if (disposable is AjivaTexture) return;

        while (!_disposables.Writer.TryWrite(disposable))
        {
            Logger.LogWarning("Disposables Channel Full, waiting for space");
            Thread.Sleep(10);
        }
    }

    public void DoCollection()
    {
        while (_disposables.Reader.TryRead(out var disposable))
        {
            try
            {
                Logger.LogDebug("Disposing {Disposable}", disposable);
                disposable.Dispose();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to dispose {Disposable}", disposable);
            }
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => InitDefaultResources();
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DoCollection();
        foreach (var texture in _internal)
        {
            texture.Dispose();
        }

        _loadSem.Dispose();
        // Do not dispose, as textures in cache, as the cache doesn't ref them
        // foreach (var key in _loadedTextures.Keys.ToArray()) if (_loadedMeshes.TryRemove(key, out var value)) value.Dispose();
        _loadedTextures.Clear();
        _loadedMeshes.Clear();

        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
