using AVFoundation;
using CoreMedia;
using CoreVideo;
using System;

namespace AkiraScan.Camera.Platform.Darwin;

internal sealed class AVFoundationCameraFrameDelegate(IAVFoundationCameraOutputHolder output) : AVCaptureVideoDataOutputSampleBufferDelegate
{
    public event Action? OnUpdate;

    public override async void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
    {
        using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
        {
            if (pixelBuffer is not null)
            {
                await output.Process(pixelBuffer);

                OnUpdate?.Invoke();
            }
        }

        sampleBuffer.Dispose();
    }
}
