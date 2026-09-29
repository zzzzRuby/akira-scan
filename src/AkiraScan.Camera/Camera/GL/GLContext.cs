using System;

namespace AkiraScan.Camera.GL;

internal sealed class GLContext : IDisposable
{
    private readonly Silk.NET.OpenGLES.Extensions.OES.OesEglImage? _oes;
    private readonly EGL? _egl;
    private readonly Silk.NET.OpenGLES.GL _interface;
    private readonly IGLDevice _device;

    private GLContext(IGLDevice device, Silk.NET.OpenGLES.GL i, Silk.NET.OpenGLES.Extensions.OES.OesEglImage? oes, EGL? egl)
    {
        _device = device;
        _interface = i;
        _oes = oes;
        _egl = egl;
    }

    public static GLContext? Create(IGLDevice device)
    {
        using (device.MakeCurrent())
        {
            var i = Silk.NET.OpenGLES.GL.GetApi(device.GetProcAddress);
            i.TryGetExtension(out Silk.NET.OpenGLES.Extensions.OES.OesEglImage? oes);

            var egl = EGL.TryCreate(device);

            return new GLContext(device, i, oes, egl);
        }
    }

    public void Dispose()
    {
        _oes?.Dispose();
        _interface.Dispose();
        _device.Dispose();
    }

    public IDisposable MakeCurrent()
    {
        return _device.MakeCurrent();

    }
    public IGLDevice Device => _device;
    public Silk.NET.OpenGLES.Extensions.OES.OesEglImage? Oes => _oes;
    public Silk.NET.OpenGLES.GL Interface => _interface;
    public EGL? EGL => _egl;
}
