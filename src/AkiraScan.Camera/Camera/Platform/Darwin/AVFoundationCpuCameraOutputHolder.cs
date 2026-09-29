using AkiraScan.Camera.Cpu;
using CoreVideo;
using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Darwin;

internal sealed class AVFoundationCpuCameraOutputHolder(int width, int height, CameraColor color): CpuCameraOutputHolder(width, height, color), IAVFoundationCameraOutputHolder
{
    public async Task Process(CVPixelBuffer pixelBuffer)
    {
        pixelBuffer.Lock(CVPixelBufferLock.None);
        try
        {
            int height = (int)pixelBuffer.Height;
            int bytesPerRow = (int)pixelBuffer.BytesPerRow;
            IntPtr baseAddress = pixelBuffer.BaseAddress;

            using var l = await _output.UpdateLockAsync();
            unsafe
            {
                fixed(byte* output = _output.BufferArray)
                {
                    Buffer.MemoryCopy((byte*)baseAddress, output, _output.Buffer.Length, bytesPerRow * height);
                }
            }
        }
        finally
        {
            pixelBuffer.Unlock(CVPixelBufferLock.None);
        }
    }
}
