using System;

namespace AkiraScan.Camera;

public interface IGpuCameraOutputLock
{
}

public interface IKeyedMutexGpuCameraOutputLock : IGpuCameraOutputLock
{
    uint RenderLock { get; }

    uint PresentLock { get; }
}

public interface ISemaphoresGpuCameraOutputLock : IGpuCameraOutputLock
{
    IntPtr RenderLock { get; }

    IntPtr PresentLock { get; }
}

public interface ITimelineSemaphoresGpuCameraOutputLock : IGpuCameraOutputLock
{
    IntPtr RenderLock { get; }

    IntPtr PresentLock { get; }

    ulong PresentWait { get; }

    ulong RenderWait { get; }
}