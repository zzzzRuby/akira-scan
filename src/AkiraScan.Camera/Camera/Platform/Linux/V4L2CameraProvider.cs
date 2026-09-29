using System;
using System.Threading.Tasks;
using static AkiraScan.Camera.Platform.Linux.V4L2PInvoke;

namespace AkiraScan.Camera.Platform.Linux;

internal sealed class V4L2CameraProvider : ICameraProvider
{
    private readonly V4L2Capture _device;
    private readonly IV4L2CameraOutputHolder _output;

    public event Action? AfterFrameUpdate;

    public ICameraOutput Output => _output.GetOutput();

    public IGpuDevice? Gpu => _output.Gpu;

    private V4L2CameraProvider(
        V4L2Capture device,
        IV4L2CameraOutputHolder output)
    {
        _device = device;
        _output = output;
        _device.FrameArrived += OnFrameArrived;
    }

    private async void OnFrameArrived(object? sender, V4L2FrameEventArgs e)
    {
        await _output.UpdateFrameAsync(e.Frame);
        e.Frame.Dispose();
        AfterFrameUpdate?.Invoke();
    }

    public static async Task<V4L2CameraProvider> CreateAsync(string devicePath, CameraColor color)
    {
        var capture = new V4L2Capture(devicePath, V4L2BufferMode.Mmap, bufferCount: 4);

        int width = 1920, height = 1080;
        capture.SetFormat(new V4L2VideoFormat
        {
            Width = (uint)width,
            Height = (uint)height,
            PixelFormat = V4L2_PIX_FMT_MJPEG,
            Field = v4l2_field.V4L2_FIELD_ANY,
            Colorspace = (uint)v4l2_colorspace.V4L2_COLORSPACE_SRGB,
        });
        capture.InitBuffers();

        IV4L2CameraOutputHolder output;
        //if (gpu != null && actualFormat == targetFourcc)
        //{
        //    output = V4L2VulkanCameraOutputHolder.Create(gpu, width, height, color, actualFormat);
        //}
        //else
        //{
            output = new V4L2CpuCameraOutputHolder(width, height, color);
        //}

        return new V4L2CameraProvider(capture, output);
    }

    public Task StartCameraAsync()
    {
        _device.StartCaptureLoop();
        return Task.CompletedTask;
    }

    public Task StopCameraAsync()
    {
        _device.StopCaptureLoop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _device.FrameArrived -= OnFrameArrived;
        _output.Dispose();
        _device.Dispose();
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