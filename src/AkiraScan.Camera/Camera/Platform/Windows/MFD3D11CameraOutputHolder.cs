using AkiraScan.Camera.Cpu;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Media.MediaFoundation;

namespace AkiraScan.Camera.Platform.Windows;

[SupportedOSPlatform("windows8.0")]
internal sealed class MFD3D11CameraOutputHolder : IMFCameraOutputHolder
{
    private readonly ComPtr<ID3D11Device> _device;
    private readonly MFD3D11CameraOutput _output;
    private readonly IGpuDevice _gpu;

    public IGpuDevice? Gpu => _gpu;

    private MFD3D11CameraOutputHolder(IGpuDevice gpu, ComPtr<ID3D11Device> device, MFD3D11CameraOutput output)
    {
        _device = device;
        _output = output;
        _gpu = gpu;
    }

    public static MFD3D11CameraOutputHolder Create(IGpuDevice gpu, ComPtr<ID3D11Device> device, int width, int height, CameraColor color)
    {
        var output = MFD3D11CameraOutput.Create(device, width, height, color);
        return new MFD3D11CameraOutputHolder(gpu, device, output);
    }

    public Task UpdateFrame(ComPtr<IMFSample> pSample)
    {
        _output.FrameUpdate(pSample, _device);
        return Task.CompletedTask;
    }

    public ICameraOutput GetOutput()
    {
        return _output;
    }

    public void Dispose()
    {
        _output.Dispose();
        _device.Dispose();
    }

    public Task CopyTo(IDisposableCameraOutput output)
    {
        if (output is CpuCameraOutput cpuOutput)
        {
            _output.CopyTo(cpuOutput, _device);
        }
        else if (output is MFD3D11CameraOutput d3d11Output)
        {
            _output.CopyTo(d3d11Output, _device);
        }

        return Task.CompletedTask;
    }

    public IDisposableCameraOutput CreateOutput()
    {
        return MFD3D11CameraOutput.Create(_device, _output.Width, _output.Height, _output.Color);
    }
}
