using System;
using Windows.Win32.Graphics.Dxgi;

namespace AkiraScan.Camera.Platform.Windows;

internal class MFD3D11CameraOutputLock(ComPtr<IDXGIKeyedMutex> mutex) : IKeyedMutexGpuCameraOutputLock, IDisposable
{
    public uint RenderLock => 0;
    public uint PresentLock => 1;

    internal void AcquireAtRender()
    {
        mutex.AcquireSync(RenderLock, unchecked((uint)-1));
    }

    internal void ReleaseAtRender()
    {
        mutex.ReleaseSync(PresentLock);
    }

    public void Dispose()
    {
        mutex.Dispose();
    }
}
