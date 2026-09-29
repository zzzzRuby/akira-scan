using System;

namespace AkiraScan.Camera;

public interface IGpuDevice
{
}

public interface IGLDevice : IGpuDevice, IDisposable
{
    IDisposable MakeCurrent();

    IntPtr GetProcAddress(string name);
}

public sealed class ModernGpuDevice(CameraGpu gpu, byte[]? luid, byte[]? uuid) : IGpuDevice
{
    public byte[]? DeviceLuid => luid;
    public byte[]? DeviceUuid => uuid;
    public CameraGpu Gpu => gpu;
}
