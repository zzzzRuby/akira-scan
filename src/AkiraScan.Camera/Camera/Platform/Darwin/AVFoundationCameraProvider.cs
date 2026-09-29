using AVFoundation;
using CoreFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using Metal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Darwin;

internal sealed class AVFoundationCameraProvider : ICameraProvider
{
    private readonly AVCaptureSession _captureSession;
    private readonly AVFoundationCameraFrameDelegate _delegate;
    private readonly IAVFoundationCameraOutputHolder _output;

    public event Action? AfterFrameUpdate
    {
        add => _delegate.OnUpdate += value;
        remove => _delegate.OnUpdate -= value;
    }

    public ICameraOutput Output => _output.GetOutput();

    public IGpuDevice? Gpu => _output.Gpu;

    private AVFoundationCameraProvider(AVCaptureSession captureSession, AVFoundationCameraFrameDelegate frameDelegate, IAVFoundationCameraOutputHolder output)
    {
        _captureSession = captureSession;
        _delegate = frameDelegate;
        _output = output;
    }

    public static async Task<AVFoundationCameraProvider> CreateAsync(AVCaptureDevice videoDevice, ModernGpuDevice[] gpus)
    {
        var captureSession = new AVCaptureSession();
        captureSession.BeginConfiguration();

        var formatDescription = videoDevice.ActiveFormat.FormatDescription as CMVideoFormatDescription ?? throw CameraException.NoValidCameraSource();

        var dimensions = formatDescription.Dimensions;

        var videoInput = AVCaptureDeviceInput.FromDevice(videoDevice, out var error);
        if (error is not null)
            throw new CameraException(error.ToString());
        if (videoInput is null)
            throw CameraException.NoValidCameraSource();
        if (!captureSession.CanAddInput(videoInput))
            throw CameraException.NoValidCameraSource();
        captureSession.AddInput(videoInput);

        var videoOutput = new AVCaptureVideoDataOutput
        {
            WeakVideoSettings = new NSDictionary(
                CVPixelBuffer.PixelFormatTypeKey,
                NSNumber.FromInt32((int)CVPixelFormatType.CV32BGRA)
            )
        };

        MetalGpu? targetDevice = null;
        if (gpus.Length != 0)
        {
            var gpuLuids = gpus
                .Select(x => (x.DeviceLuid, x))
                .Where(x => x.DeviceLuid is not null)
                .OfType<(byte[], ModernGpuDevice)>()
                .Select(x =>
                {
                    var bytes = (byte[])x.Item1.Clone();
                    bytes.AsSpan().Reverse();
                    return (BitConverter.ToUInt64(bytes, 0), x.Item2);
                })
                .ToArray();

            IMTLDevice[] gpuDevices;

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsIOSVersionAtLeast(18) || OperatingSystem.IsTvOSVersionAtLeast(18))
            {
                gpuDevices = MTLDevice.GetAllDevices() ?? [];
            }
            else if (OperatingSystem.IsIOS() || OperatingSystem.IsTvOS())
            {
                var defaultDevice = MTLDevice.SystemDefault;
                gpuDevices = defaultDevice is not null ? [defaultDevice] : [];
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                gpuDevices = MTLDevice.GetAllDevices() ?? [];
            }
            else
            {
                gpuDevices = [];
            }

            foreach(var gpuLuid in gpuLuids)
            {
                var device = gpuDevices.FirstOrDefault(x => x.RegistryId == gpuLuid.Item1);
                if (device is not null)
                {
                    targetDevice = new MetalGpu(gpuLuid.Item2, device);
                    break;
                }
            }
        }

        IAVFoundationCameraOutputHolder output = targetDevice is not null ? 
            AVFoundationMetalCameraOutputHolder.Create(targetDevice, dimensions.Width, dimensions.Height, CameraColor.Bgra8888) :
            new AVFoundationCpuCameraOutputHolder(dimensions.Width, dimensions.Height, CameraColor.Bgra8888);

        var frameDelegate = new AVFoundationCameraFrameDelegate(output);
        var queue = new DispatchQueue("camera_frame_queue");
        videoOutput.SetSampleBufferDelegate(frameDelegate, queue);

        if (captureSession.CanAddOutput(videoOutput))
        {
            captureSession.AddOutput(videoOutput);
        }

        captureSession.CommitConfiguration();

        return new AVFoundationCameraProvider(captureSession, frameDelegate, output);
    }

    public async Task StartCameraAsync()
    {
        _captureSession.StartRunning();
    }

    public async Task StopCameraAsync()
    {
        _captureSession.StopRunning();
    }

    public void Dispose()
    {
        _captureSession.Dispose();
        _delegate.Dispose();
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