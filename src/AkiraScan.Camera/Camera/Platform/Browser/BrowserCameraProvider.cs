using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Browser;

[SupportedOSPlatform("browser")]
internal sealed class BrowserCameraProvider : ICameraProvider
{
    private readonly string _cameraId;
    private readonly BrowserCpuCameraOutputHolder _output;
    private readonly Action _frameCallback;

    public event Action? AfterFrameUpdate;

    public ICameraOutput Output => _output.GetOutput();

    public IGpuDevice? Gpu => null;

    private BrowserCameraProvider(string cameraId, BrowserCpuCameraOutputHolder output)
    {
        _cameraId = cameraId;
        _output = output;
        _frameCallback = OnFrame;
    }

    public static async Task<BrowserCameraProvider> CreateAsync(
        string? deviceId, int width, int height)
    {
        var cameraId = await BrowserCameraInterop.StartCamera(deviceId, width, height);
        var actualWidth = BrowserCameraInterop.GetWidth(cameraId);
        var actualHeight = BrowserCameraInterop.GetHeight(cameraId);

        if (actualWidth == 0 || actualHeight == 0)
        {
            BrowserCameraInterop.StopCamera(cameraId);
            throw CameraException.NoValidCameraSource();
        }

        var output = new BrowserCpuCameraOutputHolder(actualWidth, actualHeight);
        return new BrowserCameraProvider(cameraId, output);
    }

    public Task StartCameraAsync()
    {
        BrowserCameraInterop.StartFrameLoop(_cameraId, _frameCallback);
        return Task.CompletedTask;
    }

    public Task StopCameraAsync()
    {
        BrowserCameraInterop.StopFrameLoop(_cameraId);
        return Task.CompletedTask;
    }

    private async void OnFrame()
    {
        await _output.UpdateFrame(_cameraId);

        AfterFrameUpdate?.Invoke();
    }

    public void Dispose()
    {
        BrowserCameraInterop.StopCamera(_cameraId);
        _output.Dispose();
    }

    public IDisposableCameraOutput CreateCompatibleOutput()
    {
        return _output.CreateOutput();
    }

    public Task CopyTo(IDisposableCameraOutput output)
    {
        return _output.CopyTo(output);
    }
}