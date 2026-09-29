using AkiraScan.Camera.GL;
using System;
using System.Threading.Tasks;
using Tizen.Multimedia;

namespace AkiraScan.Camera.Platform.Tizen;

internal sealed class TizenCameraProvider : ICameraProvider
{
    private readonly global::Tizen.Multimedia.Camera _camera;
    private readonly ITizenCameraOutputHolder _output;

    public ICameraOutput Output => _output.GetOutput();

    public event Action? AfterFrameUpdate
    {
        add => _output.AfterFrameUpdate += value;
        remove => _output.AfterFrameUpdate -= value;
    }

    public IGpuDevice? Gpu => null;

    private TizenCameraProvider(ITizenCameraOutputHolder output, global::Tizen.Multimedia.Camera camera)
    {
        _camera = camera;
        _output = output;
        _output.SetupCamera(_camera);
    }

    public static TizenCameraProvider Create(CameraDevice device, GLContext? gl)
    {
        var camera = new global::Tizen.Multimedia.Camera(device);

        var size = camera.Settings.PreviewResolution;

        ITizenCameraOutputHolder output = gl is not null && gl.Oes is not null && gl.EGL is not null ?
            new TizenGLCameraOutputHolder(gl, gl.Oes, gl.EGL, size.Width, size.Height, CameraColor.Rgba8888):
            new TizenCpuCameraOutputHolder(size.Width, size.Height, CameraColor.Bgra8888);

        return new TizenCameraProvider(output, camera);
    }

    public Task StartCameraAsync()
    {
        _camera.StartPreview();
        return Task.CompletedTask;
    }

    public Task StopCameraAsync()
    {
        _camera.StopPreview();
        return Task.CompletedTask;
    }

    public IDisposableCameraOutput CreateCompatibleOutput()
    {
        return _output.CreateOutput();
    }

    public Task CopyTo(IDisposableCameraOutput output)
    {
        return _output.CopyTo(output);
    }

    public void Dispose()
    {
        _output.UnsetupCamera(_camera);
        _camera.Dispose();
        _output.Dispose();
    }
}