using System.Collections.Generic;

namespace AkiraScan.Camera;

public sealed class CameraParameters
{
    public string? CameraUniqueId { get; set; }

    public IGpuDevice[]? PreferGpuLists { get; set; }
    
    public Dictionary<string, object>? BackendArgs { get; set; }
}

public sealed record CameraDescriptor
{
    public required string UniqueId { get; set; }

    public required CameraFacing Facing { get; set; }
}

public enum CameraFacing
{
    Unspecified,
    Front,
    Back,
}

public enum CameraGpu
{
    D3D11,
    Vulkan,
    Metal,
}