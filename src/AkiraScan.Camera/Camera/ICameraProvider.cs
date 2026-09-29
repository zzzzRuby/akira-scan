using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera;

public interface ICameraProvider: IDisposable
{
    Task StartCameraAsync();

    Task StopCameraAsync();

    ICameraOutput Output { get; }

    event Action? AfterFrameUpdate;

    IDisposableCameraOutput CreateCompatibleOutput();

    Task CopyTo(IDisposableCameraOutput output);

    IGpuDevice? Gpu { get; }
}

public interface ICameraProviderFactory
{
    Task<ICameraProvider> OpenCameraAsync(CameraParameters? param);

    Task<CameraDescriptor[]> EnumCamerasAsync();
}