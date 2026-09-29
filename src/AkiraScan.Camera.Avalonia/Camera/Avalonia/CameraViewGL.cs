using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using System;

namespace AkiraScan.Camera.Avalonia;

internal sealed class AvaloniaGLDevice(IGlContext context) : IGLDevice
{
    public void Dispose()
    {
        context.Dispose();
    }

    public IDisposable MakeCurrent()
    {
        return context.MakeCurrent();
    }

    public IntPtr GetProcAddress(string name)
    {
        return context.GlInterface.GetProcAddress(name);
    }

    public IGlContext Inner => context;
}

internal sealed class CameraViewGL : ICameraView
{
    private ICompositionImportableOpenGlSharedTexture? _texture;
    private ICompositionImportedGpuImage? _importedTexture;
    private readonly int _program;
    private readonly int _vbo;
    private readonly int _fbo;
    private readonly int _cameraTextureUniform;

    private readonly CompositionDrawingSurface _surface;
    private readonly CompositionSurfaceVisual _visual;
    private readonly IGLCameraOutput _output;
    private readonly IGlContext _gl;
    private readonly IOpenGlTextureSharingRenderInterfaceContextFeature _glFeature;
    private readonly ICompositionGpuInterop _gpuInterop;

    private CameraViewGL(IOpenGlTextureSharingRenderInterfaceContextFeature glFeature, IGlContext gl, ICompositionGpuInterop gpuInterop, IGLCameraOutput output, CompositionSurfaceVisual visual, CompositionDrawingSurface surface, int program, int vbo, int fbo, int cameraTextureUniform)
    {
        _output = output;
        _visual = visual;
        _glFeature = glFeature;
        _gl = gl;
        _gpuInterop = gpuInterop;
        _surface = surface;
        _program = program;
        _vbo = vbo;
        _fbo = fbo;
        _cameraTextureUniform = cameraTextureUniform;
    }

    public CompositionContainerVisual Visual => _visual;

    public static CameraViewGL Setup(
        Compositor compositor,
        IGlContext gl,
        ICompositionGpuInterop gpuInterop,
        IOpenGlTextureSharingRenderInterfaceContextFeature glFeature,
        IGLCameraOutput output)
    {
        var surface = compositor.CreateDrawingSurface();

        var surfaceVisual = compositor.CreateSurfaceVisual();
        surfaceVisual.Surface = surface;

        int program = 0;
        int vbo = 0;
        int fbo = 0;
        int cameraTextureUniform = 0;
        using (gl.MakeCurrent())
        {
            program = CreateProgram(gl.GlInterface, VertexShaderSource, FragmentShaderSource);
            cameraTextureUniform = gl.GlInterface.GetUniformLocationString(program, "cameraTexture");

            vbo = gl.GlInterface.GenBuffer();
            gl.GlInterface.BindBuffer(GlConsts.GL_ARRAY_BUFFER, vbo);

            float[] vertices = [
                -1f, -1f,  0f, 0f,
                 3f, -1f,  2f, 0f,
                -1f,  3f,  0f, 2f
            ];
            unsafe { fixed (float* p = vertices) gl.GlInterface.BufferData(GlConsts.GL_ARRAY_BUFFER, sizeof(float) * 12, new IntPtr(p), GlConsts.GL_STATIC_DRAW); }

            fbo = gl.GlInterface.GenFramebuffer();
        }

        return new CameraViewGL(glFeature, gl, gpuInterop, output, surfaceVisual, surface, program, vbo, fbo, cameraTextureUniform);
    }

    public void OnAfterFrameUpdate()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (_importedTexture is not null && _texture is not null)
            {
                await _output.PresentSemaphore.WaitAsync();
                using (_gl.MakeCurrent())
                {
                    var gl = _gl.GlInterface;
                    var cameraImage = _output.GLTexture;

                    gl.BindFramebuffer(GlConsts.GL_FRAMEBUFFER, _fbo);
                    gl.FramebufferTexture2D(GlConsts.GL_FRAMEBUFFER, GlConsts.GL_COLOR_ATTACHMENT0, GlConsts.GL_TEXTURE_2D, _texture.TextureId, 0);
                    gl.Viewport(0, 0, _texture.Size.Width, _texture.Size.Height);
                    gl.Clear(GlConsts.GL_COLOR_BUFFER_BIT);

                    gl.UseProgram(_program);

                    gl.ActiveTexture(GlConsts.GL_TEXTURE0);
                    gl.BindTexture(GlConsts.GL_TEXTURE_2D, cameraImage);
                    gl.Uniform1i(_cameraTextureUniform, 0);

                    gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, _vbo);
                    gl.EnableVertexAttribArray(0);
                    gl.VertexAttribPointer(0, 2, GlConsts.GL_FLOAT, 0, 16, IntPtr.Zero);
                    gl.EnableVertexAttribArray(1);
                    gl.VertexAttribPointer(1, 2, GlConsts.GL_FLOAT, 0, 16, new IntPtr(8));
                    gl.DrawArrays(GlConsts.GL_TRIANGLES, 0, 3);
                }
                _output.RenderSemaphore.Release();

                await _surface.UpdateAsync(_importedTexture);
            }
        });
    }

    public void OnMeasure(double width, double height)
    {
        _texture?.Dispose();
        _texture = _glFeature.CreateSharedTextureForComposition(_gl, new PixelSize((int)width, (int)height));
        _importedTexture = _gpuInterop.ImportImage(_texture);
    }

    public void Dispose()
    {
        _texture?.Dispose();
        using (_gl.MakeCurrent())
        {
            _gl.GlInterface.DeleteProgram(_program);
            _gl.GlInterface.DeleteBuffer(_vbo);
            _gl.GlInterface.DeleteFramebuffer(_fbo);
        }
    }

    private const string VertexShaderSource = @"# version 100
            attribute vec2 position;
            attribute vec2 texCoord;
            varying vec2 vTexCoord;
            void main() { gl_Position = vec4(position, 0.0, 1.0); vTexCoord = texCoord; }";

    private const string FragmentShaderSource = @"# version 100
            precision mediump float;
            varying vec2 vTexCoord;
            uniform sampler2D cameraTexture;
            void main() { gl_FragColor = texture2D(cameraTexture, vTexCoord); }";

    private static int CreateProgram(GlInterface gl, string vss, string fss)
    {
        int p = gl.CreateProgram();
        int vs = gl.CreateShader(GlConsts.GL_VERTEX_SHADER);
        gl.ShaderSourceString(vs, vss);
        gl.CompileShader(vs);
        gl.AttachShader(p, vs);
        int fs = gl.CreateShader(GlConsts.GL_FRAGMENT_SHADER);
        gl.ShaderSourceString(fs, fss);
        gl.CompileShader(fs);
        gl.AttachShader(p, fs);
        gl.BindAttribLocationString(p, 0, "position");
        gl.BindAttribLocationString(p, 1, "texCoord");
        gl.LinkProgram(p);
        return p;
    }
}