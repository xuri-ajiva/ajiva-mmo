using System.Numerics;
using Common;
using Common.Systems;
using Server.Core;

namespace Server.Systems;

public class SimplePhysicsSystem(NServerEcs world) : IServerSystem
{
    public void Tick(UpdateInfo info)
    {
        UpdateVelocity(info.DeltaTime);
    }

    private void UpdateVelocity(f32 deltaTime)
    {
        using var act = Instrumentation.NamedTrace("PhysicsSystem.UpdateVelocity");
        foreach (var kv in world.Velocity)
        {
            var entityId = kv.Key;


            // linear
            {
                ref var posRef = ref world.Position.Update(entityId);
                var position = posRef.Value;
                const f32 minY = 1f;
                // simple bounce logic
                var linearVel = kv.Value.Linear;
                if (position.Y < minY)
                {
                    linearVel.Y = -linearVel.Y; // Reverse Y velocity on hit
                    position.Y = minY; // Reset to ground level
                }
                else
                {
                    linearVel.Y -= 9.81f * deltaTime; // Apply gravity
                }

                linearVel.Y = Math.Clamp(linearVel.Y, -20f, 20f);

                // Update position based on velocity
                position += linearVel * deltaTime;

                posRef.Value = position;
                kv.Value.Linear = linearVel;
            }
            
            // angular
            {
                ref var rotRef = ref world.Rotation.Update(entityId);
                var rotation = rotRef.Value;
                var angularVel = kv.Value.Angular;

                // Update rotation based on angular velocity
                var deltaRotation = Quaternion.CreateFromYawPitchRoll(
                    angularVel.Y * deltaTime,
                    angularVel.X * deltaTime,
                    angularVel.Z * deltaTime);
                rotation = Quaternion.Normalize(Quaternion.Concatenate(rotation, deltaRotation));

                rotRef.Value = rotation;
            }

            if (world.Spacial.TryGet(entityId, out var spacial))
            {
                world.Grid.GetChunkData(spacial.CurrentChunk).MarkDirty();
            }
        }
    }
}