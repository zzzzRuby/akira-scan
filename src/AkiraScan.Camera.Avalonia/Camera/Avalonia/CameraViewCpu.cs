using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using System;

namespace AkiraScan.Camera.Avalonia;

internal class CameraViewCpuCompositionCustomVisualHandler(WriteableBitmap bitmap) : CompositionCustomVisualHandler
{
    internal class SizeMessage(double width, double height)
    {
        public double Width { get; } = width;
        public double Height { get; } = height;
    }

    internal class RefreshMessage
    {
    }

    private Size _controlSize;

    public override void OnMessage(object message)
    {
        if (message is SizeMessage sizeMsg)
        {
            _controlSize = new Size(sizeMsg.Width, sizeMsg.Height);
            Invalidate();
        }
        else if (message is RefreshMessage)
        {
            Invalidate();
        }
    }

    public override void OnRender(ImmediateDrawingContext drawingContext)
    {
        drawingContext.DrawBitmap(bitmap, new Rect(0, 0, _controlSize.Width, _controlSize.Height));
    }
}

public sealed class CameraViewCpu: ICameraView
{
    private readonly WriteableBitmap _bitmap;
    private readonly ICpuCameraOutput _output;
    private readonly CompositionCustomVisual _visual;

    public CompositionContainerVisual Visual => _visual;

    private CameraViewCpu(WriteableBitmap bitmap, ICpuCameraOutput output, CompositionCustomVisual visual)
    {
        _bitmap = bitmap;
        _output = output;
        _visual = visual;
    }

    public static CameraViewCpu Setup(Compositor compositor, ICpuCameraOutput cpuOutput)
    {
        var pixelFormat = cpuOutput.Color switch
        {
            CameraColor.Rgba8888 => PixelFormat.Rgba8888,
            CameraColor.Bgra8888 => PixelFormat.Bgra8888,
            _ => throw new NotSupportedException(),
        };

        var bitmap = new WriteableBitmap(new PixelSize(cpuOutput.Width, cpuOutput.Height), new Vector(96, 96), format: pixelFormat);
        var handler = new CameraViewCpuCompositionCustomVisualHandler(bitmap);
        var customVisual = compositor.CreateCustomVisual(handler);

        return new CameraViewCpu(bitmap, cpuOutput, customVisual);
    }

    public async void OnAfterFrameUpdate()
    {
        using (var l = await _output.PresentLockAsync())
        using (var lockedBitmap = _bitmap.Lock())
        {
            var backBuffer = lockedBitmap.Address;

            unsafe
            {
                fixed (byte* cpuBuffer = _output.Buffer.Span)
                {
                    Buffer.MemoryCopy(cpuBuffer, (byte*)backBuffer, lockedBitmap.RowBytes * lockedBitmap.Size.Height, _output.Buffer.Length);
                }
            }
        }

        Dispatcher.UIThread.Post(async () =>
        {
            _visual.SendHandlerMessage(new CameraViewCpuCompositionCustomVisualHandler.RefreshMessage());
        });
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }

    public void OnMeasure(double width, double height)
    {
        _visual.SendHandlerMessage(new CameraViewCpuCompositionCustomVisualHandler.SizeMessage(width, height));
    }
}