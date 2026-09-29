using AkiraScan.Camera.GL;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Views;
using Java.Util.Concurrent;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Android;

[SupportedOSPlatform("android21.0")]
internal sealed class AndroidCameraProvider : ICameraProvider
{
    private readonly CameraDevice _cameraDevice;
    private readonly IAndroidCameraExecutor _executor;
    private readonly Handler _backgroundHandler;
    private readonly HandlerThread _backgroundThread;
    private readonly IAndroidCameraOutputHolder _output;

    private CameraCaptureSession? _captureSession;
    private bool _isDisposed;

    public event Action? AfterFrameUpdate
    {
        add => _output.AfterFrameUpdate += value;
        remove => _output.AfterFrameUpdate -= value;
    }

    public ICameraOutput Output => _output.GetOutput();

    public IGpuDevice? Gpu => _output.Gpu;

    private AndroidCameraProvider(
        CameraDevice device,
        IAndroidCameraExecutor executor,
        HandlerThread thread,
        Handler handler,
        IAndroidCameraOutputHolder output)
    {
        _cameraDevice = device;
        _executor = executor;
        _backgroundThread = thread;
        _backgroundHandler = handler;

        _output = output;
    }

    public static async Task<AndroidCameraProvider> CreateAsync(CameraManager cameraManager, string cameraId, CameraCharacteristics characteristics, GLContext? gl)
    {
        if (characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap) is not StreamConfigurationMap map)
        {
            throw CameraException.NoValidCameraSource();
        }

        var supportedSizes = map.GetOutputSizes((int)ImageFormatType.Yuv420888);

        if (supportedSizes == null || supportedSizes.Length == 0)
        {
            throw CameraException.NoValidCameraSource();
        }

        var thread = new HandlerThread("CameraBackgroundThread");
        thread.Start();
        var handler = new Handler(thread.Looper ?? throw CameraException.NoValidCameraSource());

        var bestSize = supportedSizes
            .OrderByDescending(s => s.Width * s.Height)
            .First();

        var finalWidth = bestSize.Width;
        var finalHeight = bestSize.Height;

        IAndroidCameraOutputHolder output;
        if (gl is not null)
        {
            output = new AndroidGLCameraOutputHolder(gl, finalWidth, finalHeight, CameraColor.Rgba8888, handler);
        }
        else
        {
            output = AndroidCpuCameraOutputHolder.Create(finalWidth, finalHeight, CameraColor.Rgba8888, handler);
        }

        var cameraOpenTcs = new TaskCompletionSource<CameraDevice>();
        var deviceStateCallback = new CameraDeviceStateCallback(cameraOpenTcs);

        IAndroidCameraExecutor executor;
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            executor = AndroidCameraExecutor28.Create(handler);
        }
        else
        {
            executor = AndroidCameraExecutor21.Create(handler);
        }
        executor.OpenCamera(cameraManager, cameraId, deviceStateCallback);

        var device = await cameraOpenTcs.Task;

        return new AndroidCameraProvider(device, executor, thread, handler, output);
    }

    public async Task StartCameraAsync()
    {
        if (_captureSession != null) return;
        var surface = _output.Surface;

        var sessionConfigTcs = new TaskCompletionSource<CameraCaptureSession>();
        var sessionStateCallback = new SessionStateCallback(sessionConfigTcs);

        _executor.CreateCaptureSession(_cameraDevice, surface, sessionStateCallback);

        _captureSession = await sessionConfigTcs.Task;

        using var requestBuilder = _cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
        requestBuilder.AddTarget(surface);

        requestBuilder.Set(CaptureRequest.ControlAfMode!, (int)ControlAFMode.ContinuousVideo);
        requestBuilder.Set(CaptureRequest.ControlAeMode!, (int)ControlAEMode.On);

        _captureSession.SetRepeatingRequest(requestBuilder.Build(), null, _backgroundHandler);
    }

    public async Task StopCameraAsync()
    {
        _captureSession?.StopRepeating();
        _captureSession?.Dispose();
        _captureSession = null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _captureSession?.StopRepeating();
        _captureSession?.Dispose();

        _cameraDevice.Close();
        _cameraDevice.Dispose();

        _executor.Dispose();
        _backgroundThread.QuitSafely();
        try
        {
            _backgroundThread.Join();
        }
        catch
        {
        }

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

    private class CameraDeviceStateCallback(TaskCompletionSource<CameraDevice> tcs) : CameraDevice.StateCallback
    {
        public override void OnOpened(CameraDevice camera) => tcs.TrySetResult(camera);
        public override void OnDisconnected(CameraDevice camera) => camera.Close();
        public override void OnError(CameraDevice camera, CameraError error)
            => tcs.TrySetException(CameraException.NoValidCameraSource());
    }

    private class SessionStateCallback(TaskCompletionSource<CameraCaptureSession> tcs) : CameraCaptureSession.StateCallback
    {
        public override void OnConfigured(CameraCaptureSession session) => tcs.TrySetResult(session);
        public override void OnConfigureFailed(CameraCaptureSession session)
            => tcs.TrySetException(CameraException.NoValidCameraSource());
    }
}

internal interface IAndroidCameraExecutor : IDisposable
{
    void CreateCaptureSession(CameraDevice cameraDevice, Surface surface, CameraCaptureSession.StateCallback callback);

    void OpenCamera(CameraManager cameraManager, string cameraId, CameraDevice.StateCallback callback);
}

[SupportedOSPlatform("android28.0")]
internal sealed class AndroidCameraExecutor28 : IAndroidCameraExecutor
{
    private readonly IExecutor _cameraExecutor;

    private AndroidCameraExecutor28(IExecutor cameraExecutor)
    {
        _cameraExecutor = cameraExecutor;
    }

    public static AndroidCameraExecutor28 Create(Handler handler)
    {
        var executor = new HandlerExecutor(handler);
        return new AndroidCameraExecutor28(executor);
    }

    public void CreateCaptureSession(CameraDevice cameraDevice, Surface surface, CameraCaptureSession.StateCallback callback)
    {
        var outputConfig = new OutputConfiguration(surface);
        var sessionConfig = new SessionConfiguration(
            (int)SessionType.Regular,
            [outputConfig],
            _cameraExecutor,
            callback
        );

        cameraDevice.CreateCaptureSession(sessionConfig);
    }

    public void OpenCamera(CameraManager cameraManager, string cameraId, CameraDevice.StateCallback callback)
    {
        cameraManager.OpenCamera(cameraId, _cameraExecutor, callback);
    }

    public void Dispose()
    {
        _cameraExecutor.Dispose();
    }

    private class HandlerExecutor(Handler handler) : Java.Lang.Object, IExecutor
    {
        public void Execute(Java.Lang.IRunnable? command)
        {
            if (command is null)
                return;
            handler.Post(command);
        }
    }
}

[SupportedOSPlatform("android21.0")]
[UnsupportedOSPlatform("android30.0")]
internal sealed class AndroidCameraExecutor21 : IAndroidCameraExecutor
{
    private readonly Handler _handler;

    private AndroidCameraExecutor21(Handler handler)
    {
        _handler = handler;
    }

    public static AndroidCameraExecutor21 Create(Handler handler)
    {
        return new AndroidCameraExecutor21(handler);
    }

    public void CreateCaptureSession(CameraDevice cameraDevice, Surface surface, CameraCaptureSession.StateCallback callback)
    {
        cameraDevice.CreateCaptureSession([surface], callback, _handler);
    }

    public void OpenCamera(CameraManager cameraManager, string cameraId, CameraDevice.StateCallback callback)
    {
        cameraManager.OpenCamera(cameraId, callback, _handler);
    }

    public void Dispose()
    {
    }
}
