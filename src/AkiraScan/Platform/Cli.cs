using AkiraScan.Core;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace AkiraScan.Platform
{
    internal static class Cli
    {
        private static SKBitmap ConvertPixelFormat(string path, SKColorType targetColorType)
        {
            using var sourceBitmap = SKBitmap.Decode(path);
            var targetInfo = new SKImageInfo(
                sourceBitmap.Width,
                sourceBitmap.Height,
                targetColorType,
                sourceBitmap.AlphaType
            );

            SKBitmap targetBitmap = new SKBitmap(targetInfo);

            using (var canvas = new SKCanvas(targetBitmap))
            {
                canvas.DrawBitmap(sourceBitmap, 0, 0);
            }

            return targetBitmap;
        }

        public static void CliMain(string[] args)
        {
            using var image = ConvertPixelFormat("akira-scan.jpg", SKColorType.Rgba8888);
            using var scanner = Scanner.Create("akira-scan.onnx");
            var tensor = new DenseTensor<byte>(image.Bytes.AsMemory(), [1, image.Height, image.Width, 4]);
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var result = scanner.Scan<MahjongTile, MahjongTileClassConverter>(tensor, false);
            }
            watch.Stop();
            Console.WriteLine($"{watch.ElapsedMilliseconds / 1000.0}ms");
            /*
            using (var canvas = new SKCanvas(image))
            {
                foreach (var tile in result)
                {
                    canvas.DrawRect(new SKRect
                    {
                        Left = tile.Left,
                        Top = tile.Top,
                        Size = new SKSize(tile.Width, tile.Height)
                    }, new SKPaint
                    {
                        Color = new SKColor(255, 255, 255, 255),
                        IsStroke = true,
                    });
                }
            }

            using var outputImage = File.Create("akira-scan-2.png");
            image.Encode(outputImage, SKEncodedImageFormat.Png, 100);
            */
        }
    }
}