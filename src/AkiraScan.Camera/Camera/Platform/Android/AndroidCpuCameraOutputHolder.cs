using AkiraScan.Camera.Cpu;
using AkiraScan.Camera.Utils;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using System;

namespace AkiraScan.Camera.Platform.Android;

internal sealed class AndroidCpuCameraOutputHolder: CpuCameraOutputHolder, IAndroidCameraOutputHolder
{
    private readonly ImageAvailableListener _listener;
    private readonly ImageReader _reader;
    private readonly Surface _surface;

    public Surface Surface => _surface;

    private AndroidCpuCameraOutputHolder(int width, int height, CameraColor color, ImageReader reader, Handler backgroundHandler): base(width, height, color)
    {
        _listener = new(_output);
        _reader = reader;
        _surface = reader.Surface ?? throw new InvalidOperationException("get surface failed");

        _reader.SetOnImageAvailableListener(_listener, backgroundHandler);
    }

    public static AndroidCpuCameraOutputHolder Create(int width, int height, CameraColor color, Handler backgroundHandler)
    {
        var reader = ImageReader.NewInstance(width, height, ImageFormatType.Yuv420888, 2);

        return new AndroidCpuCameraOutputHolder(width, height, color, reader, backgroundHandler);
    }

    public event Action? AfterFrameUpdate
    {
        add => _listener.AfterFrameUpdate += value;
        remove => _listener.AfterFrameUpdate -= value;
    }

    public override void OnDispose()
    {
        _reader.SetOnImageAvailableListener(null, null);
        _reader.Dispose();
        _listener.Dispose();
    }

    private sealed class ImageAvailableListener(CpuCameraOutput cpuOutput) : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        public event Action? AfterFrameUpdate;

        public async void OnImageAvailable(ImageReader? reader)
        {
            Image? image = null;
            try
            {
                image = reader?.AcquireLatestImage();
                if (image is null) return;

                var planes = image.GetPlanes();
                if (planes is not null && planes.Length >= 3)
                {
                    var yBuffer = planes[0].Buffer;
                    var uBuffer = planes[1].Buffer;
                    var vBuffer = planes[2].Buffer;

                    if (yBuffer is null || uBuffer is null || vBuffer is null) return;

                    using (var l = await cpuOutput.UpdateLockAsync())
                    {
                        unsafe
                        {
                            var yPointer = (byte*)yBuffer.GetDirectBufferAddress();
                            var uPointer = (byte*)uBuffer.GetDirectBufferAddress();
                            var vPointer = (byte*)vBuffer.GetDirectBufferAddress();
                            fixed (byte* output = cpuOutput.BufferArray)
                            {
                                Yuv.Yuv420ToRgba(yPointer, planes[0].RowStride, uPointer, planes[1].RowStride, planes[1].PixelStride, vPointer, planes[2].RowStride, planes[2].PixelStride, output, cpuOutput.Width, cpuOutput.Height);
                            }
                        }
                    }

                    AfterFrameUpdate?.Invoke();
                }
            }
            finally
            {
                image?.Close();
            }
        }
    }
}
