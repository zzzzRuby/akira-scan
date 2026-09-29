using System;

namespace AkiraScan.Camera;

public interface IGpuCameraOutput : ICameraOutput
{
    IntPtr Handle { get; }

    IGpuCameraOutputLock Lock { get; }

    CameraGpu Gpu { get; }
}