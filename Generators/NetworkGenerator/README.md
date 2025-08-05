# NetworkPacket Source Generator

This source generator simplifies the creation and handling of network packets in C# applications using Riptide Networking. It automatically generates serialization and deserialization logic, as well as message handling interfaces, based on simple struct or class definitions.

## Usage

### 1. Define Your Packet

Declare your network packets as `public partial struct` or `public partial class` and annotate them with the `[NetworkPacket(ID)]` attribute. The `ID` is a unique `ushort` identifier for your packet.

Example:

```csharp
using NetworkPackets; // Ensure this using directive is present
using System.Numerics; // For TransformComponent example

// Define a simple packet
[NetworkPacket(200)]
public partial struct PlayerMovePacket(float x, float y, float z);

// Define a packet containing a custom IMessageSerializable type
[NetworkPacket(300)]
public partial class BeginWorldStatePacket(int entityCount);

[NetworkPacket(301)]
public partial class EntityCreatedPacket(int entityId, TransformComponent transform);
```

**Important Considerations for Packet Fields:**

*   **Primitive Types:** The generator supports standard C# primitive types (e.g., `bool`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `string`).
*   **Custom Types (`IMessageSerializable`):** For custom complex types (like `TransformComponent` in the example above), they **must** implement the `Riptide.IMessageSerializable` interface.
    *   The `IMessageSerializable` implementation should include `Serialize(Message message)` and a static `FromMessage(Message message)` method.
    *   The `Deserialize(Message message)` instance method on `IMessageSerializable` can be left empty for `readonly struct` types, as the static `FromMessage` method will be used for deserialization.

    Example `TransformComponent` (from `Common/Ecs/TransformComponent.cs`):
    ```csharp
    using System.Numerics;
    using Riptide;
    using Common.Networking; // For MessageExtensions

    namespace Common.Ecs;

    public readonly record struct TransformComponent : IComponentData, IMessageSerializable
    {
        public Vector3 Position { get; init; }
        public Quaternion Rotation { get; init; }
        public Vector3 Scale { get; init; }

        public void Serialize(Message message)
        {
            message.AddVector3(Position);
            message.AddQuaternion(Rotation);
            message.AddVector3(Scale);
        }

        public void Deserialize(Message message)
        {
            // Intentionally left empty for readonly struct; static FromMessage is used.
        }

        public static TransformComponent FromMessage(Message message)
        {
            return new TransformComponent
            {
                Position = message.GetVector3(),
                Rotation = message.GetQuaternion(),
                Scale = message.GetVector3()
            };
        }
    }
    ```

### 2. Send Your Packet

Once your packet is defined, you can send it from either the client or server using the generated `Send` or `SendToAll` extension methods.

```csharp
using Riptide;
using NetworkPackets; // Ensure this using directive is present

// Example of sending a packet to a single connection
var connection = /* your Riptide Connection object */;
var entityCount = 10;
new BeginWorldStatePacket(entityCount)
    .Send(connection, MessageSendMode.Reliable); // Choose Reliable, Unreliable, or Notify

// Example of broadcasting a packet to all clients from the server
var server = /* your Riptide Server object */;
new EntityUpdatePacket(/*...args*/)
    .SendToAll(server, MessageSendMode.Unreliable);
```

### 3. Consume Your Packet

To receive and handle packets, implement the `IUniversalMessageHandler` interface in your message consumer class (e.g., your `Worker` or `GameLogic` class).

```csharp
using Riptide;
using NetworkPackets; // Ensure this using directive is present
using NetworkPackets.Generated; // For generated PacketType enum and HandleMessage extension

public class MyMessageHandler : IUniversalMessageHandler
{
    // This method is called by Riptide when a message is received
    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // Call the generated HandleMessage extension method
        this.HandleMessage(e.Message!);
    }

    // Implement specific handlers for each packet type you want to process
    public void Handle(BeginWorldStatePacket message)
    {
        // Logic to handle BeginWorldStatePacket
        Console.WriteLine($"Received BeginWorldStatePacket with {message.EntityCount} entities.");
    }

    public void Handle(EntityCreatedPacket message)
    {
        // Logic to handle EntityCreatedPacket
        Console.WriteLine($"Entity {message.EntityId} created at {message.Transform.Position}");
    }

    // If a specific Handle method is not implemented for a packet type,
    // the MessageNotHandled method will be called.
    public void MessageNotHandled(Message message)
    {
        Console.WriteLine($"Received an unhandled message with ID: {message.GetUShort()}");
    }
}
```

### Troubleshooting

If you encounter issues with the generator, especially `KeyNotFoundException` or similar, ensure:

*   All custom types used in your packet definitions implement `Riptide.IMessageSerializable`.
*   Your `IMessageSerializable` types have both `Serialize(Message)` and a static `FromMessage(Message)` method.
*   The `NetworkPackets` namespace is correctly imported in your packet definitions.
*   Rebuild your project after making changes to packet definitions or custom serializable types.
