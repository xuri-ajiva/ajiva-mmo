using System.Numerics;
using Ajiva.Ecs;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.SpacialPartitioning;
using Serilog;

namespace Server;

public class DemoLevel
{
    /// <summary>
    /// Load level within the assigned region
    /// </summary>
    public static void LoadLevel(NServerEcs world, RegionAssignment region, ServerId localServerId)
    {
        var chunkSize = world.Grid.GridInfo.ChunkSize;

        // Find the chunk coordinates closest to the world origin (0,0) that are within the region's bounds.
        var closestChunkX = Math.Clamp(0, region.MinChunk.X, region.MaxChunk.X);
        var closestChunkZ = Math.Clamp(0, region.MinChunk.Z, region.MaxChunk.Z);
        var closestChunk = new Vector3(closestChunkX, region.MinChunk.Y, closestChunkZ);

        const i32 chunks = 16;
        var desiredMinChunk = new Vector3(closestChunk.X - chunks, region.MinChunk.Y, closestChunk.Z - chunks);
        var desiredMaxChunk = new Vector3(closestChunk.X + chunks, region.MaxChunk.Y, closestChunk.Z + chunks);

        // Clamp the desired 3x3 area to the actual region bounds. This handles regions smaller than 3x3.
        var placementMinChunk = new Vector3(
            Math.Max(region.MinChunk.X, desiredMinChunk.X),
            region.MinChunk.Y,
            Math.Max(region.MinChunk.Z, desiredMinChunk.Z)
        );

        var placementMaxChunk = new Vector3(
            Math.Min(region.MaxChunk.X, desiredMaxChunk.X),
            region.MaxChunk.Y,
            Math.Min(region.MaxChunk.Z, desiredMaxChunk.Z)
        );

        // --- Place the Rotating Box ---

        // Calculate the world center of the single chunk closest to the origin.
        var centerChunkWorld = new Vector3(
            (closestChunk.X + 0.5f) * chunkSize.X,
            (closestChunk.Y + 0.5f) * chunkSize.Y,
            (closestChunk.Z + 0.5f) * chunkSize.Z
        );

        // Create a rotating box at the center of that chunk.
        {
            var rotation = world.CreateEntity();

            world.Position.Add(rotation, new Position(centerChunkWorld + new Vector3(0, 5, 0)));
            world.Rotation.Add(rotation, new Rotation { Value = Quaternion.Identity });
            world.Scale.Add(rotation, new Scale { Value = Vector3.One });
            world.Velocity.Add(rotation, new Velocity { Angular = Vector3.UnitZ });
            world.EntityType.Add(rotation, new EntityType { TypeId = 1 });

            // Add to spatial grid
            var chunk = world.Grid.GetChunkCoord(new Position(centerChunkWorld + new Vector3(0, 5, 0)).Value, 0);
            world.Spacial.Add(rotation, new SpacialData { CurrentChunk = chunk, PreviousChunk = chunk });

            // Mark as owned by this server
            world.EntityOwnership.Add(rotation, new EntityOwnership
            {
                OwnerServerId = localServerId,
                IsProxy = false,
                IsInProxyRange = false,
                LastSyncTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        // Calculate world coordinates from the determined placement chunk area.
        var minWorld = new Vector3(
            placementMinChunk.X * chunkSize.X,
            placementMinChunk.Y * chunkSize.Y,
            placementMinChunk.Z * chunkSize.Z
        );

        var maxWorld = new Vector3(
            (placementMaxChunk.X + 1) * chunkSize.X,
            (placementMaxChunk.Y + 1) * chunkSize.Y,
            (placementMaxChunk.Z + 1) * chunkSize.Z
        );

        var regionWidth = maxWorld.X - minWorld.X;
        var regionDepth = maxWorld.Z - minWorld.Z;
        var spacing = 2.0f;

        var entitiesX = (i32)(regionWidth / spacing);
        var entitiesZ = (i32)(regionDepth / spacing);

        for (var i = 0; i < entitiesX; i++)
        {
            for (var j = 0; j < entitiesZ; j++)
            {
                var box = world.CreateEntity();

                world.Position.Add(box,
                    new Position(new Vector3(minWorld.X + i * spacing, chunkSize.Y * 10, minWorld.Z + j * spacing)));
                world.Rotation.Add(box, new Rotation { Value = Quaternion.Identity });
                world.Scale.Add(box, new Scale { Value = Vector3.One });
                world.Velocity.Add(box, new Velocity());
                world.EntityType.Add(box, new EntityType { TypeId = 1 });

                // Add to spatial grid
                var chunk1 = world.Grid.GetChunkCoord(new Position(new Vector3(
                    minWorld.X + i * spacing,
                    chunkSize.Y * 10,
                    minWorld.Z + j * spacing
                )).Value, 0);
                world.Spacial.Add(box, new SpacialData { CurrentChunk = chunk1, PreviousChunk = chunk1 });

                // Mark as owned by this server
                world.EntityOwnership.Add(box, new EntityOwnership
                {
                    OwnerServerId = localServerId,
                    IsProxy = false,
                    IsInProxyRange = false,
                    LastSyncTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
        }
    }
}