using AkiraScan.Camera.GL;
using Android.Graphics;
using Android.OS;
using Android.Views;
using System;

namespace AkiraScan.Camera.Platform.Android;

internal sealed class AndroidGLCameraOutputHolder: GLOesCameraOutputHolder<AndroidGLCameraOutputTextureLifetime>, IAndroidCameraOutputHolder
{
    private readonly Handler _handler;
    private readonly Surface _surface;
    private readonly SurfaceTexture _surfaceTexture;

    public event Action? AfterFrameUpdate;

    private void FrameAvailable(object? sender, SurfaceTexture.FrameAvailableEventArgs? args)
    {
        _handler.Post(async () =>
        {
            await _output.RenderSemaphore.WaitAsync();
            ProcessAndCopyFrame();
            _output.PresentSemaphore.Release();
            AfterFrameUpdate?.Invoke();
        });
    }

    public AndroidGLCameraOutputHolder(GLContext gl, int width, int height, CameraColor color, Handler handler): base(gl, width, height, color)
    {
        _surfaceTexture = new SurfaceTexture(unchecked((int)_oesTexId));
        _surfaceTexture.SetDefaultBufferSize(width, height);

        _handler = handler;
        _surface = new Surface(_surfaceTexture);
    }

    public override float[] GetTransformMatrix()
    {
        _surfaceTexture.UpdateTexImage();
        float[] systemMatrix = new float[16];
        _surfaceTexture.GetTransformMatrix(systemMatrix);
        float[] counterMatrix =
        [
            0.0f,  1.0f, 0.0f, 0.0f,
               -1.0f,  0.0f, 0.0f, 0.0f,
                0.0f,  0.0f, 1.0f, 0.0f,
                1.0f,  0.0f, 0.0f, 1.0f
        ];

        static float[] Multiply4x4(float[] a, float[] b)
        {
            float[] r = new float[16];
            for (int c = 0; c < 4; c++)
                for (int row = 0; row < 4; row++)
                {
                    float s = 0;
                    for (int k = 0; k < 4; k++)
                        s += a[k * 4 + row] * b[c * 4 + k];
                    r[c * 4 + row] = s;
                }
            return r;
        }

        return Multiply4x4(counterMatrix, systemMatrix);
    }

    protected override void OnDispose(bool disposing)
    {
        _surfaceTexture.FrameAvailable -= FrameAvailable;
        _surfaceTexture.Release();
        _surfaceTexture.Dispose();

        _surface.Release();
        _surface.Dispose();
    }

    public Surface Surface => _surface;
}