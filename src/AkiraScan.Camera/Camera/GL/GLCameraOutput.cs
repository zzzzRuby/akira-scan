using AkiraScan.Camera.Cpu;
using AkiraScan.Camera.Utils;
using Silk.NET.OpenGLES;
using System;
using System.Threading;

namespace AkiraScan.Camera.GL;

internal sealed class GLCameraOutput : IGLCameraOutput, IDisposableCameraOutput
{
    private readonly Arc<GLContext> _glDevice;
    private readonly IGLCameraOutputTextureLifetime? _textureLifetime;
    private readonly int _width, _height;
    private readonly CameraColor _color;
    private bool _disposed;

    private readonly uint _texture;
    private readonly uint _fbo;
    private readonly uint _pbo;

    public int Width => _width;
    public int Height => _height;
    public CameraColor Color => _color;
    public CameraRotate Rotate => CameraRotate.None;
    public int GLTexture => unchecked((int)_texture);

    public SemaphoreSlim RenderSemaphore { get; } = new(1);
    public SemaphoreSlim PresentSemaphore { get; } = new(0);

    private GLCameraOutput(
        int width, int height, CameraColor color,
        Arc<GLContext> glDevice, IGLCameraOutputTextureLifetime? lifetime, uint texture, uint fbo, uint pbo)
    {
        _width = width;
        _height = height;
        _color = color;
        _glDevice = glDevice;
        _textureLifetime = lifetime;
        _texture = texture;
        _fbo = fbo;
        _pbo = pbo;
    }

    public static unsafe GLCameraOutput Create<T>(
        int width, int height, CameraColor color,
        Arc<GLContext> device) where T: class, IGLCameraOutputTextureLifetime
    {
        uint fboId = 0;
        uint pboId = 0;
        uint textureId = 0;
        int bufferSize = width * height * 4;
        IGLCameraOutputTextureLifetime? textureLifetime = null;

        using (device.Inner.MakeCurrent())
        {
            var gl = device.Inner.Interface;

            try
            {
                textureId = gl.GenTexture();
                gl.BindTexture(TextureTarget.Texture2D, textureId);

                textureLifetime = T.BindToGLTexture(device.Inner, width, height);
                if (textureLifetime is null)
                {
                    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                        (uint)width, (uint)height, 0,
                        PixelFormat.Rgba, PixelType.UnsignedByte, null);
                }

                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
                gl.BindTexture(TextureTarget.Texture2D, 0);

                fboId = gl.GenFramebuffer();
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, fboId);
                gl.FramebufferTexture2D(
                    FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D,
                    textureId, 0);

                pboId = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.PixelPackBuffer, pboId);
                gl.BufferData(
                    BufferTargetARB.PixelPackBuffer,
                    (nuint)bufferSize,
                    null,
                    BufferUsageARB.StreamRead);

                gl.BindBuffer(BufferTargetARB.PixelPackBuffer, 0);
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            catch
            {
                if (pboId != 0) gl.DeleteBuffers([pboId]);
                if (fboId != 0) gl.DeleteFramebuffers([fboId]);
                if (textureId != 0) gl.DeleteTextures([textureId]);
                textureLifetime?.GLRelease(device.Inner);
                textureLifetime?.Dispose();
                
                throw;
            }
        }

        return new GLCameraOutput(width, height, color, device.Clone(), textureLifetime, textureId, fboId, pboId);
    }

    public void CopyTo(GLCameraOutput output)
    {
        using (_glDevice.Inner.MakeCurrent())
        {
            var gl = _glDevice.Inner.Interface;

            gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _fbo);
            gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, output._fbo);

            gl.BlitFramebuffer(
                0, 0, _width, _height,
                0, 0, output._width, output._height,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);

            gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            gl.Flush();
        }
    }

    public unsafe void CopyTo(CpuCameraOutput output)
    {
        byte[] pixels = output.BufferArray;

        using (_glDevice.Inner.MakeCurrent())
        {
            var gl = _glDevice.Inner.Interface;

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            gl.BindBuffer(BufferTargetARB.PixelPackBuffer, _pbo);

            gl.ReadPixels(0, 0, (uint)_width, (uint)_height,
                PixelFormat.Rgba, PixelType.UnsignedByte, null);

            nint syncFence = gl.FenceSync(
                SyncCondition.SyncGpuCommandsComplete,
                SyncBehaviorFlags.None);
            gl.Flush();

            GLEnum waitResult = gl.ClientWaitSync(
                syncFence, SyncObjectMask.Bit, 1_000_000_000L);
            gl.DeleteSync(syncFence);

            if (waitResult is GLEnum.AlreadySignaled or GLEnum.ConditionSatisfied)
            {
                var mapped = gl.MapBufferRange(
                    BufferTargetARB.PixelPackBuffer,
                    0, (nuint)pixels.Length,
                    MapBufferAccessMask.ReadBit);

                fixed (byte* pixelPtr = pixels)
                {
                    System.Buffer.MemoryCopy(
                        mapped, pixelPtr, pixels.Length, pixels.Length);
                }

                gl.UnmapBuffer(BufferTargetARB.PixelPackBuffer);
            }

            gl.BindBuffer(BufferTargetARB.PixelPackBuffer, 0);
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        using (_glDevice.Inner.MakeCurrent())
        {
            var gl = _glDevice.Inner.Interface;
            gl.DeleteFramebuffers([_fbo]);
            gl.DeleteBuffers([_pbo]);
            gl.DeleteTextures([_texture]);
            _textureLifetime?.GLRelease(_glDevice.Inner);
        }

        _textureLifetime?.Dispose();

        RenderSemaphore.Dispose();
        PresentSemaphore.Dispose();
        _glDevice.Dispose();

        _disposed = true;
    }

    ~GLCameraOutput()
    {
        Dispose(false);
    }
}