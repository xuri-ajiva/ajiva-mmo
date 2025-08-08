using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AjivaGenerator.Ecs;

/// <summary>
/// Information about a component discovered during generation
/// </summary>
internal record ComponentInfo(
    string Name,
    string Namespace,
    ComponentScope Scope,
    ComponentSync Sync,
    ComponentFlags Flags,
    ImmutableArray<FieldInfo> Fields,
    StructDeclarationSyntax StructSyntax,
    INamedTypeSymbol StructSymbol
);

/// <summary>
/// Information about a field in a component
/// </summary>
internal record FieldInfo(
    string Name,
    string TypeName
);

/// <summary>
/// Component scope enum for internal use
/// </summary>
[System.Flags]
internal enum ComponentScope
{
    Server = 1 << 0,
    Client = 1 << 1,
    Shared = Server | Client,
}

/// <summary>
/// Component sync enum for internal use
/// </summary>
[System.Flags]
internal enum ComponentSync
{
    None = 0,
    ServerToClient = 1 << 0,
    ClientToServer = 1 << 1,
    Bidirectional = ServerToClient | ClientToServer,
}

/// <summary>
/// Component flags enum for internal use
/// </summary>
[System.Flags]
internal enum ComponentFlags
{
    None = 0,
    Dense = 1 << 0,
    Sparse = 1 << 1,
    DirtyTracked = 1 << 2,
    ImmediateSync = 1 << 3,
    ReliableSync = 1 << 4,
    UnreliableSync = 1 << 5,
    SyncOnce = 1 << 6,
}
