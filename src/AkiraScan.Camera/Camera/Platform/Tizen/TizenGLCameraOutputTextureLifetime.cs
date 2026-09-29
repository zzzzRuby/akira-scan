using AkiraScan.Camera.GL;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.OES;
using System;

namespace AkiraScan.Camera.Platform.Tizen;

internal sealed class TizenGLCameraOutputTextureLifetime : IGLCameraOutputTextureLifetime
{
    private readonly IntPtr _eglImage;
    private readonly IntPtr _eglDisplay;
    private readonly TbmSurface _tbmSurface;

    private TizenGLCameraOutputTextureLifetime(IntPtr eglDisplay, IntPtr eglImage, TbmSurface tbmSurface)
    {
        _eglImage = eglImage;
        _eglDisplay = eglDisplay;
        _tbmSurface = tbmSurface;
    }

    public static IGLCameraOutputTextureLifetime? BindToGLTexture(GLContext gl, int width, int height)
    {
        var egl = gl.EGL;
        var oes = gl.Oes;

        if (egl is not null && oes is not null)
        {
            TbmSurface tbmSurface;
            try
            {
                tbmSurface = TbmSurface.Create(width, height, TbmFormat.Rgba8888);
            }
            catch (TbmException)
            {
                return null;
            }

            var eglDisplay = egl.GetCurrentDisplay();

            var eglImage = egl.CreateImage(
                eglDisplay,
                IntPtr.Zero,
                (uint)EGLEnum.NativeSurfaceTIZEN,
                tbmSurface.DangerousHandle,
                [(uint)EGLEnum.ImagePreserved, (uint)EGLEnum.True, (uint)EGLEnum.None]);

            if (eglImage == IntPtr.Zero)
            {
                tbmSurface.Dispose();
                return null;
            }

            oes.EglimageTargetTexture2D((OES)TextureTarget.Texture2D, eglImage);

            return new TizenGLCameraOutputTextureLifetime(eglDisplay, eglImage, tbmSurface);
        }

        return null;
    }

    public void GLRelease(GLContext gl)
    {
        if (_eglImage != IntPtr.Zero)
            gl.EGL?.DestroyImage(_eglDisplay, _eglImage);
    }

    public void Dispose()
    {
        _tbmSurface.Dispose();
    }
}