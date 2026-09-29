using System;

namespace AkiraScan.Camera.Utils;

internal static class Yuv
{
    public static void YuyvToBgra(ReadOnlySpan<byte> src, Span<byte> dst, int width, int height)
    {
        int srcStride = width * 2;
        int dstStride = width * 4;

        for (int y = 0; y < height; y++)
        {
            var srcRow = src.Slice(y * srcStride, srcStride);
            var dstRow = dst.Slice(y * dstStride, dstStride);

            for (int x = 0; x < width; x += 2)
            {
                int i = x * 2;
                byte y0 = srcRow[i];
                byte u = srcRow[i + 1];
                byte y1 = srcRow[i + 2];
                byte v = srcRow[i + 3];

                WriteBgra(y0, u, v, dstRow.Slice(x * 4, 4));
                WriteBgra(y1, u, v, dstRow.Slice((x + 1) * 4, 4));
            }
        }
    }

    public static unsafe void Yuv420ToRgba(
        byte* yPlane, int yRowStride,
        byte* uPlane, int uRowStride, int uPixelStride,
        byte* vPlane, int vRowStride, int vPixelStride,
        byte* rgba, int width, int height)
    {
        int rgbaStride = width * 4;

        for (int y = 0; y < height; y++)
        {
            byte* yRow = yPlane + y * yRowStride;
            byte* uRow = uPlane + (y >> 1) * uRowStride;
            byte* vRow = vPlane + (y >> 1) * vRowStride;
            byte* rgbaRow = rgba + y * rgbaStride;

            for (int x = 0; x < width; x++)
            {
                int uvX = x >> 1;
                var (r, g, b) = YuvToRgb(yRow[x], uRow[uvX * uPixelStride], vRow[uvX * vPixelStride]);

                rgbaRow[x * 4] = r;
                rgbaRow[x * 4 + 1] = g;
                rgbaRow[x * 4 + 2] = b;
                rgbaRow[x * 4 + 3] = 255;
            }
        }
    }

    private static void WriteBgra(byte y, byte u, byte v, Span<byte> dst)
    {
        var (r, g, b) = YuvToRgb(y, u, v);
        dst[0] = b;
        dst[1] = g;
        dst[2] = r;
        dst[3] = 255;
    }

    private static (byte r, byte g, byte b) YuvToRgb(byte y, byte u, byte v)
    {
        int c = y - 16;
        int d = u - 128;
        int e = v - 128;

        int r = (298 * c + 409 * e + 128) >> 8;
        int g = (298 * c - 100 * d - 208 * e + 128) >> 8;
        int b = (298 * c + 516 * d + 128) >> 8;

        return (
            (byte)Math.Clamp(r, 0, 255),
            (byte)Math.Clamp(g, 0, 255),
            (byte)Math.Clamp(b, 0, 255)
        );
    }
}