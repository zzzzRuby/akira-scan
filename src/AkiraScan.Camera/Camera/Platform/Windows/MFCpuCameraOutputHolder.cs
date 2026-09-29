using AkiraScan.Camera.Cpu;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Win32.Media.MediaFoundation;

namespace AkiraScan.Camera.Platform.Windows;

[SupportedOSPlatform("windows6.0.6000")]
internal sealed class MFCpuCameraOutputHolder(int width, int height, CameraColor color)
    : CpuCameraOutputHolder(width, height, color), IMFCameraOutputHolder
{
    public async Task UpdateFrame(ComPtr<IMFSample> videoFrame)
    {
        using var mediaBuffer = videoFrame.ConvertToContiguousBuffer();
        using var buffer2d = mediaBuffer.QueryInterfaceOrNull<IMF2DBuffer>();

        if (buffer2d is not null)
        {
            buffer2d.Lock2D(out var pSourceData, out var stride);
            var l = await _output.UpdateLockAsync();
            try
            {
                unsafe
                {
                    fixed (byte* pDestData = _output.BufferArray)
                    {
                        int width = _output.Width;
                        int height = _output.Height;

                        int bytesPerLine = width * 4;

                        int absStride = Math.Abs(stride);

                        if (bytesPerLine > absStride) bytesPerLine = absStride;

                        byte* pSrcRow = (byte*)pSourceData;
                        byte* pDestRow = pDestData;

                        if (stride < 0)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                Buffer.MemoryCopy(pSrcRow, pDestRow, bytesPerLine, bytesPerLine);
                                pDestRow += bytesPerLine;
                                pSrcRow -= absStride;
                            }
                        }
                        else
                        {
                            for (int y = 0; y < height; y++)
                            {
                                Buffer.MemoryCopy(pSrcRow, pDestRow, bytesPerLine, bytesPerLine);

                                pDestRow += bytesPerLine;
                                pSrcRow += absStride;
                            }
                        }
                    }
                }
            }
            finally
            {
                l.Dispose();
                buffer2d.Unlock2D();
            }
        }
        else
        {
            mediaBuffer.Lock(out var pSourceData, out var pMaxLength, out var pCurrentLength);
            var l = await _output.UpdateLockAsync();
            try
            {
                unsafe
                {
                    fixed (byte* pDestData = _output.BufferArray)
                    {
                        Buffer.MemoryCopy((void*)pSourceData, pDestData, _output.Buffer.Length, pCurrentLength);
                    }
                }
            }
            finally
            {
                l.Dispose();
                mediaBuffer.Unlock();
            }
        }
    }
}
