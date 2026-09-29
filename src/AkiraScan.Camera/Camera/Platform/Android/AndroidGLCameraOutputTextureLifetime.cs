using AkiraScan.Camera.GL;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.OES;
using System;

namespace AkiraScan.Camera.Platform.Android;

internal sealed class AndroidGLCameraOutputTextureLifetime: IGLCameraOutputTextureLifetime
{
    private readonly IntPtr _eglImage;
    private readonly IntPtr _eglDisplay;
    private readonly AHardwareBuffer _hardwareBuffer;

    private AndroidGLCameraOutputTextureLifetime(IntPtr eglDisplay, IntPtr eglImage, AHardwareBuffer hardwareBuffer)
    {
        _eglImage = eglImage;
        _eglDisplay = eglDisplay;
        _hardwareBuffer = hardwareBuffer;
    }

    public static IGLCameraOutputTextureLifetime? BindToGLTexture(GLContext gl, int width, int height)
    {
        var egl = gl.EGL;
        var oes = gl.Oes;

        if (egl is not null && oes is not null && OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var desc = new AHardwareBuffer_Desc
            {
                width = (uint)width,
                height = (uint)height,
                format = (uint)AHardwareBuffer_Format.AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM,
                usage = (ulong)(AHardwareBuffer_UsageFlags.AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE | AHardwareBuffer_UsageFlags.AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT),
                layers = 1,
                stride = 0,
                rfu0 = 0,
                rfu1 = 0,
            };

            var hardwareBuffer = AHardwareBuffer.Allocate(desc);

            var eglDisplay = egl.GetCurrentDisplay();

            var clientBuffer = egl.GetNativeClientBufferANDROID(hardwareBuffer.NativeHandle);
            var eglImage = egl.CreateImage(
                eglDisplay,
                IntPtr.Zero,
                (uint)EGLEnum.NativeBufferANDROID,
                clientBuffer,
                [(uint)EGLEnum.ImagePreserved, (uint)EGLEnum.True, (uint)EGLEnum.None]);

            oes.EglimageTargetTexture2D((OES)TextureTarget.Texture2D, eglImage);

            return new AndroidGLCameraOutputTextureLifetime(eglDisplay, eglImage, hardwareBuffer);
        }
        else
        {
            return null;
        }
    }

    public void GLRelease(GLContext gl)
    {
        if (_eglImage != IntPtr.Zero) gl.EGL?.DestroyImage(_eglDisplay, _eglImage);
    }

    public void Dispose()
    {
        _hardwareBuffer.Dispose();
    }
}
