using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Client.Renderer.PbrRenderer;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Client.Renderer.Resources;

using System;

public class ResourceLoader(RendererApi api, ILogger<ResourceLoader> logger) : IHostedLifecycleService
{
    public readonly ILogger<ResourceLoader> Logger = logger;

    private List<AjivaTexture> _internal = [];
    internal AjivaTexture DefaultTexture = null!;
    internal AjivaTexture DefaultNormal = null!;
    internal AjivaTexture DefaultRoughness = null!;
    internal AjivaTexture DefaultEmissive = null!;

    private Task InitDefaultResources()
    {
        var extent3D = new Extent3D(64, 64, 1);
        var tex = CreateTexture(TextureUsage.TextureBinding, "DefaultTexture", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);

        Span<Rgba32> data = stackalloc Rgba32[(i32)(extent3D.Width * extent3D.Height)];

        //CoolTextureGenerator.GenerateCoolPattern(data, (i32)extent3D.Width, (i32)extent3D.Height);
        CoolTextureGenerator.GenerateMissingTexture(data, (i32)extent3D.Width, (i32)extent3D.Height);

        tex.WriteTexture<Rgba32>(data, extent3D);
        var defaultTexture = AjivaTexture.FromData(tex);

        var normalTex = CreateTexture(TextureUsage.TextureBinding, "DefaultNormal", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        CoolTextureGenerator.GenerateNormalMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        normalTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultNormal = AjivaTexture.FromData(normalTex);

        var roughnessTex = CreateTexture(TextureUsage.TextureBinding, "DefaultRoughness", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        CoolTextureGenerator.GenerateRoughnessMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        roughnessTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultRoughness = AjivaTexture.FromData(roughnessTex);

        var emissiveTex = CreateTexture(TextureUsage.TextureBinding, "DefaultEmissive", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);
        CoolTextureGenerator.GenerateEmissiveMap(data, (i32)extent3D.Width, (i32)extent3D.Height);
        emissiveTex.WriteTexture<Rgba32>(data, extent3D);
        var defaultEmissive = AjivaTexture.FromData(emissiveTex);

        _internal =
        [
            defaultTexture,
            defaultNormal,
            defaultRoughness,
            defaultEmissive
        ];
        DefaultTexture = defaultTexture.Clone("defensive_copy");
        DefaultNormal = defaultNormal.Clone("defensive_copy");
        DefaultRoughness = defaultRoughness.Clone("defensive_copy");
        DefaultEmissive = defaultEmissive.Clone("defensive_copy");

        return Task.CompletedTask;
    }

    public QueuePtr Queue => api.Device.GetQueue();
    private readonly SemaphoreSlim _loadSem = new(1, 1);
    private readonly SemaphoreSlim _writeSem = new(1, 1);
    private readonly ConcurrentDictionary<string, AjivaTexture> _loadedTextures = new();

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

            target.Assign(val.Data);
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
                Logger.LogCritical(e, "Failed to load texture {Path}", path);
                throw;
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
                    await _writeSem.WaitAsync();
                    try
                    {
                        target.WriteTexture<Rgba32>(rent.AsSpan(0, sizePixels),
                            new Extent3D((u32)imageToUpload.Width, (u32)imageToUpload.Height, 1), mipLevel: mipLevel);
                    }
                    finally
                    {
                        _writeSem.Release();
                    }

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
        var texture = api.Device.CreateTexture(
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

        Logger.LogInformation("Created texture {Label}", label);
        return new TextureData(this, label, texture, textureView, extent3D, aspect, mipLevels);
    }

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
        result.QueuedWrite(data, Queue);

        Logger.LogDebug("Created buffer {Label} with {Count} items ({Type}).", label, data.Length, typeof(T).Name);
        return result;
    }

    public BufferRange CreateBufferRaw(BufferUsage usage, u64 sizeInBytes, string? label) =>
        new(api.Device.CreateBuffer(usage, sizeInBytes, mappedAtCreation: false, label), 0, sizeInBytes);

    private readonly Channel<IDisposable> _disposables =
        Channel.CreateUnbounded<IDisposable>(new UnboundedChannelOptions { SingleReader = true });

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
        _writeSem.Dispose();

        // Do not dispose, as textures in cache, as the cache doesn't ref them
        // foreach (var key in _loadedTextures.Keys.ToArray()) if (_loadedTextures.TryRemove(key, out var value)) value.Dispose();
        _loadedTextures.Clear();

        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
