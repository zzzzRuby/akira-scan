using System;
using System.Runtime.Versioning;
using Windows.Win32.Graphics.Dxgi;

namespace AkiraScan.Camera.Platform.Windows;

[SupportedOSPlatform("windows6.1")]
internal sealed class D3D11Gpu(ModernGpuDevice device, ComPtr<IDXGIAdapter1> dxgi) : IDisposable
{
    public ModernGpuDevice Device => device;
    public ComPtr<IDXGIAdapter1> DXGI => dxgi;

    public void Dispose() => dxgi.Dispose();
}
