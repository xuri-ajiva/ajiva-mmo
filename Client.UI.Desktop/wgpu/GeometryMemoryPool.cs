using System.Runtime.InteropServices;
using Client.Renderer.Resources;
using Microsoft.Extensions.Logging;
using RmlUiNet;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.UI.Desktop.wgpu;

/// <summary>
/// Represents a rented range of geometry data in the memory pool.
/// Contains both vertex and index buffers with their respective offsets.
/// </summary>
public class GeometryRent : IDisposable
{
    /// <summary>Vertex buffer containing the geometry vertices</summary>
    public required AjivaBuffer<Vertex> VertexBuffer { get; set; }

    /// <summary>Index buffer containing the geometry indices</summary>
    public required AjivaBuffer<u32> IndexBuffer { get; set; }

    /// <summary>Base vertex offset in the vertex buffer (in vertices, not bytes)</summary>
    public required u32 BaseVertex { get; set; }

    /// <summary>First index offset in the index buffer (in indices, not elements)</summary>
    public required u32 FirstIndex { get; set; }

    /// <summary>Number of indices in this geometry</summary>
    public required u32 IndexCount { get; set; }

    /// <summary>Number of vertices in this geometry</summary>
    public required u32 VertexCount { get; set; }

    /// <summary>Internal pool reference for cleanup</summary>
    internal GeometryMemoryPool? Pool { get; set; }

    public void Dispose()
    {
        Pool?.Return(this);
    }
}

/// <summary>
/// Tracks an allocated range within a buffer
/// </summary>
internal record struct AllocationInfo(u32 Offset, u32 Count, bool IsUsed, GeometryRent? Rent);

/// <summary>
/// Represents a single geometry buffer with its allocation tracking and fragmentation score
/// </summary>
internal class BufferPool<T>(AjivaBuffer<T> buffer, u32 capacity)
    where T : unmanaged
{
    public AjivaBuffer<T> Buffer { get; } = buffer;
    public List<AllocationInfo> Allocations { get; init; } = [];
    public u32 Capacity { get; } = capacity;
    public u32 UsedCount { get; set; }

    /// <summary>
    /// Calculates fragmentation score (0 = no fragmentation, 1 = maximum fragmentation)
    /// Based on the ratio of free holes to total free space
    /// </summary>
    public f32 GetFragmentationScore()
    {
        var totalFree = Capacity - UsedCount;
        if (totalFree == 0)
            return 0f; // Fully used, no fragmentation

        // Count free holes (unused ranges)
        var holeCount = Allocations.Count(a => !a.IsUsed);
        if (holeCount == 0)
            return 0f;

        // Fragmentation = number of holes / total free space
        // More holes relative to free space = higher fragmentation
        return MathF.Min(1f, holeCount / (f32)totalFree);
    }

    /// <summary>
    /// Tries to allocate a contiguous range. Returns offset or u32.MaxValue if failed.
    /// </summary>
    public u32 TryAllocate(u32 count)
    {
        if (UsedCount + count > Capacity)
            return u32.MaxValue;

        // Try to find a free hole that fits
        for (var i = 0; i < Allocations.Count; i++)
        {
            var (offset, used, isUsed, _) = Allocations[i];
            if (isUsed || used < count) continue;

            if (used > count)
            {
                // Split the hole
                Allocations[i] = new AllocationInfo(offset, count, true, null);
                Allocations.Insert(i + 1, new AllocationInfo(offset + count, used - count, false, null));
            }
            else
            {
                Allocations[i] = new AllocationInfo(offset, count, true, null);
            }

            UsedCount += count;
            return offset;
        }

        // No hole found, try to allocate at the end
        var endOffset = Allocations.Count > 0
            ? Allocations[^1].Offset + Allocations[^1].Count
            : 0;

        if (endOffset + count <= Capacity)
        {
            Allocations.Add(new AllocationInfo(endOffset, count, true, null));
            UsedCount += count;
            return endOffset;
        }

        return u32.MaxValue;
    }

    public void Deallocate(u32 offset, u32 count)
    {
        for (var i = 0; i < Allocations.Count; i++)
        {
            var alloc = Allocations[i];
            if (alloc.Offset == offset && alloc.Count == count && alloc.IsUsed)
            {
                Allocations[i] = new AllocationInfo(offset, count, false, null);
                UsedCount -= count;

                // Merge with adjacent free blocks
                if (i > 0 && !Allocations[i - 1].IsUsed)
                {
                    var prev = Allocations[i - 1];
                    Allocations[i - 1] = new AllocationInfo(prev.Offset, prev.Count + count, false, null);
                    Allocations.RemoveAt(i);
                    i--;
                }

                if (i < Allocations.Count - 1 && !Allocations[i + 1].IsUsed)
                {
                    var next = Allocations[i + 1];
                    var merged = Allocations[i];
                    Allocations[i] = new AllocationInfo(merged.Offset, merged.Count + next.Count, false, null);
                    Allocations.RemoveAt(i + 1);
                }

                return;
            }
        }
    }
}

/// <summary>
/// Geometry memory pool with multiple buffers, freelist allocation, and compaction support
/// </summary>
public class GeometryMemoryPool(ResourceManager resourceManager, ILogger<GeometryMemoryPool> logger)
{
    private const u32 BufferCapacity = 262144; // 256K vertices/indices per buffer
    private const f32 FragmentationThreshold = 0.4f; // Compact when > 40% fragmented

    private readonly List<BufferPool<Vertex>> _vertexBuffers = [];
    private readonly List<BufferPool<u32>> _indexBuffers = [];

    private readonly Dictionary<GeometryRent, (i32 VertexBufferIdx, i32 IndexBufferIdx)> _rentToBuffers = new();

    private bool _isInitialized = false;

    public void Initialize()
    {
        if (_isInitialized)
        {
            logger.LogWarning("GeometryMemoryPool already initialized");
            return;
        }

        // Create initial buffers
        CreateNewVertexBuffer();
        CreateNewIndexBuffer();

        _isInitialized = true;
        logger.LogInformation("GeometryMemoryPool initialized with initial buffers (capacity: {Capacity})",
            BufferCapacity);
    }

    private void CreateNewVertexBuffer()
    {
        var buffer = resourceManager.CreateBuffer<Vertex>(
            BufferUsage.CopyDst | BufferUsage.Vertex | BufferUsage.CopySrc,
            (i32)BufferCapacity,
            $"GeometryPool_VertexBuffer_{_vertexBuffers.Count}"
        );
        _vertexBuffers.Add(new BufferPool<Vertex>(buffer, BufferCapacity));
    }

    private void CreateNewIndexBuffer()
    {
        var buffer = resourceManager.CreateBuffer<u32>(
            BufferUsage.CopyDst | BufferUsage.Index | BufferUsage.CopySrc,
            (i32)BufferCapacity,
            $"GeometryPool_IndexBuffer_{_indexBuffers.Count}"
        );
        _indexBuffers.Add(new BufferPool<u32>(buffer, BufferCapacity));
    }

    /// <summary>
    /// Rents a contiguous range from the memory pool for geometry data.
    /// </summary>
    public GeometryRent Rent(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<u32> indices, QueuePtr queue)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("GeometryMemoryPool must be initialized before use");

        var vertexCount = (u32)vertices.Length;
        var indexCount = (u32)indices.Length;

        // Allocate space in vertex and index buffers
        var (vertexBufferIdx, vertexOffset) = AllocateInVertexBuffer(vertexCount);
        var (indexBufferIdx, indexOffset) = AllocateInIndexBuffer(indexCount);

        // Write data
        WriteToVertexBuffer(vertexBufferIdx, vertexOffset, vertices, queue);
        WriteToIndexBuffer(indexBufferIdx, indexOffset, indices, queue);

        var rent = new GeometryRent
        {
            VertexBuffer = _vertexBuffers[vertexBufferIdx].Buffer,
            IndexBuffer = _indexBuffers[indexBufferIdx].Buffer,
            BaseVertex = vertexOffset,
            FirstIndex = indexOffset,
            VertexCount = vertexCount,
            IndexCount = indexCount,
            Pool = this
        };

        _rentToBuffers[rent] = (vertexBufferIdx, indexBufferIdx);

        logger.LogTrace(
            "Rented geometry: {VertexCount} vertices at VB[{VBIdx}]:{VO}, {IndexCount} indices at IB[{IBIdx}]:{IO}",
            vertexCount, vertexBufferIdx, vertexOffset, indexCount, indexBufferIdx, indexOffset
        );

        return rent;
    }

    private (i32 BufferIdx, u32 Offset) AllocateInVertexBuffer(u32 count)
    {
        // Try to find space in existing buffers
        for (var i = 0; i < _vertexBuffers.Count; i++)
        {
            var offset = _vertexBuffers[i].TryAllocate(count);
            if (offset != u32.MaxValue)
                return (i, offset);
        }

        // Need new buffer
        CreateNewVertexBuffer();
        var newOffset = _vertexBuffers[^1].TryAllocate(count);
        if (newOffset == u32.MaxValue)
            throw new InvalidOperationException($"Cannot allocate {count} vertices even in new buffer");

        return (_vertexBuffers.Count - 1, newOffset);
    }

    private (i32 BufferIdx, u32 Offset) AllocateInIndexBuffer(u32 count)
    {
        // Try to find space in existing buffers
        for (var i = 0; i < _indexBuffers.Count; i++)
        {
            var offset = _indexBuffers[i].TryAllocate(count);
            if (offset != u32.MaxValue)
                return (i, offset);
        }

        // Need new buffer
        CreateNewIndexBuffer();
        var newOffset = _indexBuffers[^1].TryAllocate(count);
        if (newOffset == u32.MaxValue)
            throw new InvalidOperationException($"Cannot allocate {count} indices even in new buffer");

        return (_indexBuffers.Count - 1, newOffset);
    }

    private void WriteToVertexBuffer(i32 bufferIdx, u32 offset, ReadOnlySpan<Vertex> data, QueuePtr queue)
    {
        queue.WriteBuffer(
            _vertexBuffers[bufferIdx].Buffer.Buffer,
            offset * AjivaBuffer<Vertex>.SizeOfT,
            data
        );
    }

    private void WriteToIndexBuffer(i32 bufferIdx, u32 offset, ReadOnlySpan<u32> data, QueuePtr queue)
    {
        queue.WriteBuffer(
            _indexBuffers[bufferIdx].Buffer.Buffer,
            offset * (u64)sizeof(u32),
            data
        );
    }

    /// <summary>
    /// Returns a rented geometry range back to the pool, freeing its allocation.
    /// </summary>
    public void Return(GeometryRent rent)
    {
        if (!_rentToBuffers.TryGetValue(rent, out var bufferIndices))
            return;

        var vbPool = _vertexBuffers[bufferIndices.VertexBufferIdx];
        var ibPool = _indexBuffers[bufferIndices.IndexBufferIdx];

        vbPool.Deallocate(rent.BaseVertex, rent.VertexCount);
        ibPool.Deallocate(rent.FirstIndex, rent.IndexCount);

        _rentToBuffers.Remove(rent);

        logger.LogTrace(
            "Returned geometry: {VertexCount} vertices from VB[{VBIdx}], {IndexCount} indices from IB[{IBIdx}]",
            rent.VertexCount, bufferIndices.VertexBufferIdx, rent.IndexCount, bufferIndices.IndexBufferIdx
        );
    }

    /// <summary>
    /// Compacts fragmented buffers by copying used data to new buffers and updating rents.
    /// Uses the provided command encoder for buffer copy operations.
    /// </summary>
    public void Compact(CommandEncoderPtr commandEncoder)
    {
        logger.LogInformation("Starting geometry buffer compaction");

        // Compact vertex buffers
        var fragmentedVertexBuffers = _vertexBuffers
            .Select((pool, idx) => (Pool: pool, Index: idx, Fragmentation: pool.GetFragmentationScore()))
            .Where(x => x.Fragmentation > FragmentationThreshold);

        foreach (var (_, bufferIdx, fragmentation) in fragmentedVertexBuffers)
        {
            logger.LogWarning("Compacting vertex buffer {Index} with fragmentation {Fragmentation:P}", bufferIdx,
                fragmentation);
            CompactVertexBuffer(commandEncoder, bufferIdx);
        }

        // Compact index buffers
        var fragmentedIndexBuffers = _indexBuffers
            .Select((pool, idx) => (Pool: pool, Index: idx, Fragmentation: pool.GetFragmentationScore()))
            .Where(x => x.Fragmentation > FragmentationThreshold);

        foreach (var (_, bufferIdx, fragmentation) in fragmentedIndexBuffers)
        {
            logger.LogWarning("Compacting index buffer {Index} with fragmentation {Fragmentation:P}", bufferIdx,
                fragmentation);
            CompactIndexBuffer(commandEncoder, bufferIdx);
        }

        logger.LogInformation("Compaction complete");
    }

    private void CompactVertexBuffer(CommandEncoderPtr commandEncoder, i32 bufferIdx)
    {
        var oldPool = _vertexBuffers[bufferIdx];
        var usedAllocations = oldPool.Allocations.Where(a => a.IsUsed).OrderBy(a => a.Offset).ToList();

        if (usedAllocations.Count == 0)
        {
            // All free, just reset
            oldPool.Allocations.Clear();
            oldPool.UsedCount = 0;
            oldPool.Allocations.Add(new AllocationInfo(0, oldPool.Capacity, false, null));
            return;
        }

        // Create new buffer for compacted data
        var newBuffer = resourceManager.CreateBuffer<Vertex>(
            BufferUsage.CopyDst | BufferUsage.Vertex | BufferUsage.CopySrc,
            (i32)oldPool.Capacity,
            $"GeometryPool_VertexBuffer_{_vertexBuffers.Count}_Compact"
        );

        // Copy each used allocation to new buffer at packed offset
        u32 newOffset = 0;
        var newAllocations = new List<AllocationInfo>();

        foreach (var alloc in usedAllocations)
        {
            var copySize = alloc.Count * (u32)Marshal.SizeOf<Vertex>();
            commandEncoder.CopyBufferToBuffer(
                oldPool.Buffer.Buffer,
                alloc.Offset * (u64)Marshal.SizeOf<Vertex>(),
                newBuffer.Buffer,
                newOffset * (u64)Marshal.SizeOf<Vertex>(),
                copySize
            );

            // Update the rent to point to new buffer and offset
            if (alloc.Rent != null)
            {
                alloc.Rent.VertexBuffer = newBuffer;
                alloc.Rent.BaseVertex = newOffset;
            }

            newAllocations.Add(new AllocationInfo(newOffset, alloc.Count, true, alloc.Rent));
            newOffset += alloc.Count;
        }

        // Add remaining free space as single hole
        if (newOffset < oldPool.Capacity)
        {
            newAllocations.Add(new AllocationInfo(newOffset, oldPool.Capacity - newOffset, false, null));
        }

        // Replace old buffer
        oldPool.Buffer.Dispose();
        _vertexBuffers[bufferIdx] = new BufferPool<Vertex>(newBuffer, oldPool.Capacity)
        {
            Allocations = newAllocations,
            UsedCount = newOffset
        };

        logger.LogDebug("Compacted vertex buffer {Index}: {UsedCount}/{Capacity}", bufferIdx, newOffset,
            oldPool.Capacity);
    }

    private void CompactIndexBuffer(CommandEncoderPtr commandEncoder, i32 bufferIdx)
    {
        var oldPool = _indexBuffers[bufferIdx];
        var usedAllocations = oldPool.Allocations.Where(a => a.IsUsed).OrderBy(a => a.Offset).ToList();

        if (usedAllocations.Count == 0)
        {
            // All free, just reset
            oldPool.Allocations.Clear();
            oldPool.UsedCount = 0;
            oldPool.Allocations.Add(new AllocationInfo(0, oldPool.Capacity, false, null));
            return;
        }

        // Create new buffer for compacted data
        var newBuffer = resourceManager.CreateBuffer<u32>(
            BufferUsage.CopyDst | BufferUsage.Index | BufferUsage.CopySrc,
            (i32)oldPool.Capacity,
            $"GeometryPool_IndexBuffer_{_indexBuffers.Count}_Compact"
        );

        // Copy each used allocation to new buffer at packed offset
        u32 newOffset = 0;
        var newAllocations = new List<AllocationInfo>();

        foreach (var alloc in usedAllocations)
        {
            var copySize = alloc.Count * sizeof(u32);
            commandEncoder.CopyBufferToBuffer(
                oldPool.Buffer.Buffer,
                alloc.Offset * (u64)sizeof(u32),
                newBuffer.Buffer,
                newOffset * (u64)sizeof(u32),
                copySize
            );

            // Update the rent to point to new buffer and offset
            if (alloc.Rent != null)
            {
                alloc.Rent.IndexBuffer = newBuffer;
                alloc.Rent.FirstIndex = newOffset;
            }

            newAllocations.Add(new AllocationInfo(newOffset, alloc.Count, true, alloc.Rent));
            newOffset += alloc.Count;
        }

        // Add remaining free space as single hole
        if (newOffset < oldPool.Capacity)
        {
            newAllocations.Add(new AllocationInfo(newOffset, oldPool.Capacity - newOffset, false, null));
        }

        // Replace old buffer
        oldPool.Buffer.Dispose();
        _indexBuffers[bufferIdx] = new BufferPool<u32>(newBuffer, oldPool.Capacity)
        {
            Allocations = newAllocations,
            UsedCount = newOffset
        };

        logger.LogDebug("Compacted index buffer {Index}: {UsedCount}/{Capacity}", bufferIdx, newOffset,
            oldPool.Capacity);
    }

    /// <summary>
    /// Gets statistics about the current buffer usage and fragmentation
    /// </summary>
    public (i32 VertexBufferCount, i32 IndexBufferCount, f32 AvgVertexFragmentation, f32 AvgIndexFragmentation, u32
        totalAllocatedBytes, u32 totalUsedBytes)
        GetStats()
    {
        return (_vertexBuffers.Count, _indexBuffers.Count,
                _vertexBuffers.Count > 0 ? _vertexBuffers.Average(b => b.GetFragmentationScore()) : 0f,
                _indexBuffers.Count > 0 ? _indexBuffers.Average(b => b.GetFragmentationScore()) : 0f,
                (u32)_vertexBuffers.Sum(b => b.Capacity * (u32)Marshal.SizeOf<Vertex>()) +
                (u32)_indexBuffers.Sum(b => b.Capacity * sizeof(u32)),
                (u32)_vertexBuffers.Sum(b => b.UsedCount * (u32)Marshal.SizeOf<Vertex>()) +
                (u32)_indexBuffers.Sum(b => b.UsedCount * sizeof(u32))
            );
    }

    /// <summary>
    /// Clears all allocations. This should only be called when you're sure no rents are in use.
    /// </summary>
    public void Clear()
    {
        _rentToBuffers.Clear();
        _vertexBuffers.ForEach(b => b.Buffer.Dispose());
        _indexBuffers.ForEach(b => b.Buffer.Dispose());
        _vertexBuffers.Clear();
        _indexBuffers.Clear();
        CreateNewVertexBuffer();
        CreateNewIndexBuffer();
        logger.LogInformation("GeometryMemoryPool cleared");
    }

    public void Dispose()
    {
        _vertexBuffers.ForEach(b => b.Buffer.Dispose());
        _indexBuffers.ForEach(b => b.Buffer.Dispose());
        logger.LogInformation("GeometryMemoryPool disposed");
    }
}