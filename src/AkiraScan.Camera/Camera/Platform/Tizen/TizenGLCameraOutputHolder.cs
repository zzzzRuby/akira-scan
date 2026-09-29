using AkiraScan.Camera.GL;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.OES;
using System;
using System.Collections.Generic;
using Tizen.Multimedia;

namespace AkiraScan.Camera.Platform.Tizen;

internal sealed class TizenGLCameraOutputHolder(GLContext device, OesEglImage oes, EGL egl, int width, int height, CameraColor color) : GLOesCameraOutputHolder<TizenGLCameraOutputTextureLifetime>(device, width, height, color), ITizenCameraOutputHolder
{
    public event Action? AfterFrameUpdate;

    private readonly Dictionary<IntPtr, IntPtr> _eglImages = [];

    private void OnPreview(object? sender, MediaPacketPreviewEventArgs e)
    {
        var tbm = e.Packet.TbmSurface;
        if (tbm == IntPtr.Zero) return;

        using (_gl.Inner.MakeCurrent())
        {
            if (!_eglImages.TryGetValue(tbm, out var image))
            {
                image = egl.CreateImage(
                    egl.GetCurrentDisplay(),
                    IntPtr.Zero,
                    (uint)EGLEnum.NativeSurfaceTIZEN,
                    tbm,
                    [(uint)EGLEnum.None]);
                _eglImages.Add(tbm, image);
            }

            _gl.Inner.Interface.BindTexture((TextureTarget)OES.TextureExternalOes, _oesTexId);
            oes.EglimageTargetTexture2D(OES.TextureExternalOes, image);
        }

        AfterFrameUpdate?.Invoke();
    }

    protected override void OnGLRelease()
    {
        foreach (var image in _eglImages.Values)
        {
            egl.DestroyImage(egl.GetCurrentDisplay(), image);
        }
        _eglImages.Clear();
    }

    public void SetupCamera(global::Tizen.Multimedia.Camera camera)
    {
        camera.MediaPacketPreview += OnPreview;
    }

    public void UnsetupCamera(global::Tizen.Multimedia.Camera camera)
    {
        camera.MediaPacketPreview -= OnPreview;
    }

    private static readonly float[] IdentityMatrix =
    [
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    ];

    public override float[] GetTransformMatrix()
    {
        return IdentityMatrix;
    }
}