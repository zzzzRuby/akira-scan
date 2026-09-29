using AkiraScan.Camera.Cpu;
using CoreVideo;
using Foundation;
using IOSurface;
using Metal;
using ObjCRuntime;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AkiraScan.Camera.Platform.Darwin;

[SupportedOSPlatform("ios14.0")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("tvos17.0")]
internal sealed class AVFoundationMetalCameraOutput : IGpuCameraOutput, IDisposableCameraOutput
{
    private readonly int _width;
    private readonly int _height;
    private readonly CameraColor _color;
    private readonly IOSurface.IOSurface _iosurface;
    private readonly IMTLTexture _texture;
    private readonly AVFoundationMetalCameraOutputLock _lock;
    private readonly CVMetalTextureCache _textureCache;

    public int Width => _width;
    public int Height => _height;
    public CameraColor Color => _color;

    public nint Handle => _iosurface.GetHandle();

    public IGpuCameraOutputLock Lock => _lock;

    public CameraGpu Gpu => CameraGpu.Metal;

    public CameraRotate Rotate { get; set; } = CameraRotate.None;

    private AVFoundationMetalCameraOutput(int width, int height, CameraColor color, IOSurface.IOSurface iosurface, IMTLTexture texture, AVFoundationMetalCameraOutputLock l, CVMetalTextureCache textureCache)
    {
        _width = width;
        _height = height;
        _color = color;
        _iosurface = iosurface;
        _texture = texture;
        _lock = l;
        _textureCache = textureCache;
    }

    public static AVFoundationMetalCameraOutput Create(IMTLDevice device, int width, int height, CameraColor color)
    {
        var copyEvent = device.CreateSharedEvent() ?? throw new InvalidOperationException("Failed to create Metal shared event");
        copyEvent.SignaledValue = 0;
        var presentEvent = device.CreateSharedEvent() ?? throw new InvalidOperationException("Failed to create Metal shared event");
        presentEvent.SignaledValue = 0;

        var iosurface = new IOSurface.IOSurface(new IOSurfaceOptions
        {
            Width = width,
            Height = height,
            PixelFormat = (uint)GetIOSurfacePixelFormat(color),
            BytesPerRow = width * 4,
        });

        var descriptor = new MTLTextureDescriptor
        {
            TextureType = MTLTextureType.k2D,
            PixelFormat = GetMetalPixelFormat(color),
            Width = (nuint)width,
            Height = (nuint)height,
            Depth = 1,
            MipmapLevelCount = 1,
            SampleCount = 1,
            ArrayLength = 1,
            StorageMode = MTLStorageMode.Shared,
            Usage = MTLTextureUsage.ShaderRead | MTLTextureUsage.RenderTarget
        };

        var texture = device.CreateTexture(descriptor, iosurface, 0) ?? throw new InvalidOperationException("Failed to create Metal texture from IOSurface");

        var l = new AVFoundationMetalCameraOutputLock(copyEvent, presentEvent);

        var textureCache = new CVMetalTextureCache(device);

        return new AVFoundationMetalCameraOutput(width, height, color, iosurface, texture, l, textureCache);
    }


    public void Process(CVPixelBuffer pixels, IMTLCommandQueue commandQueue)
    {
        var commandBuffer = commandQueue.CreateCommandBuffer(new MTLCommandBufferDescriptor());
        if (commandBuffer == null) return;

        _lock.AcquireAtRender(commandBuffer);
        try
        {
            var pixelFormat = GetMetalPixelFormat(_color);

            using var cache = _textureCache.TextureFromImage(
                pixels,
                pixelFormat,
                _width,
                _height,
                0,
                out var errCode);

            if (errCode == CVReturn.Success && cache is not null && cache.Texture is IMTLTexture cacheTexture)
            {
                var blitEncoder = commandBuffer.CreateBlitCommandEncoder(new MTLBlitPassDescriptor());
                blitEncoder.Copy(cacheTexture, _texture);
                blitEncoder.EndEncoding();
            }
        }
        finally
        {
            _lock.ReleaseAtRender(commandBuffer);
            commandBuffer.Commit();
        }
    }

    private static MTLPixelFormat GetMetalPixelFormat(CameraColor color)
    {
        return color switch
        {
            CameraColor.Rgba8888 => MTLPixelFormat.RGBA8Unorm,
            CameraColor.Bgra8888 => MTLPixelFormat.BGRA8Unorm,
            _ => throw new NotSupportedException($"Camera color {color} is not supported for Metal output")
        };
    }

    private static CVPixelFormatType GetIOSurfacePixelFormat(CameraColor color)
    {
        return color switch
        {
            CameraColor.Rgba8888 => CVPixelFormatType.CV32RGBA,
            CameraColor.Bgra8888 => CVPixelFormatType.CV32BGRA,
            _ => throw new NotSupportedException($"Camera color {color} is not supported")
        };
    }

    public void Dispose()
    {
        _textureCache.Dispose();
        _texture.Dispose();
        _lock.Dispose();
        _iosurface.Dispose();
    }

    public void CopyTo(CpuCameraOutput output)
    {
        var seed = 0;
        _iosurface.Lock(IOSurfaceLockOptions.ReadOnly, ref seed);
        try
        {
            var baseAddress = _iosurface.BaseAddress;
            var allocSize = _iosurface.AllocationSize;

            Marshal.Copy(baseAddress, output.BufferArray, 0, (int)allocSize);
        }
        finally
        {
            _iosurface.Unlock(IOSurfaceLockOptions.ReadOnly);
        }
    }

    public void CopyTo(AVFoundationMetalCameraOutput output, IMTLCommandQueue commandQueue)
    {
        var commandBuffer = commandQueue.CreateCommandBuffer(new MTLCommandBufferDescriptor()) ?? throw new InvalidOperationException("Create MTLCommandBuffer failed.");

        try
        {
            var blitEncoder = commandBuffer.CreateBlitCommandEncoder(new MTLBlitPassDescriptor());
            blitEncoder.Copy(_texture, output._texture);
            blitEncoder.EndEncoding();
        }
        finally
        {
            commandBuffer.Commit();
        }
    }
}