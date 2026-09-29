using Metal;
using System;

namespace AkiraScan.Camera.Platform.Darwin;

internal sealed class MetalGpu(ModernGpuDevice device, IMTLDevice mtl) : IDisposable
{
    public ModernGpuDevice Device => device;

    public IMTLDevice MTLDevice => mtl;

    public void Dispose()
    {
        mtl.Dispose();
    }
}
