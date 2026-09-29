using AkiraScan.Camera.Cpu;
using AkiraScan.Camera.Utils;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.OES;
using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera.GL;

internal abstract class GLOesCameraOutputHolder<T> : ICameraOutputHolder where T: class, IGLCameraOutputTextureLifetime
{
    protected readonly GLCameraOutput _output;
    private readonly uint _fboId;
    private readonly uint _shaderProgram;
    private readonly uint _vboId;
    protected readonly uint _oesTexId;

    private readonly int _vTextureHandle;
    private readonly int _vMatrixHandle;
    private bool _isDisposed;

    protected readonly Arc<GLContext> _gl;

    protected GLOesCameraOutputHolder(GLContext device, int width, int height, CameraColor color)
    {
        uint vboId = 0;
        uint fboId = 0;
        uint program = 0;
        uint oesTexId = 0;
        int vTextureHandle = 0;
        int vMatrixHandle = 0;

        using (var ctx = device.MakeCurrent())
        {
            var gl = device.Interface;
            try
            {
                oesTexId = gl.GenTexture();
                gl.BindTexture((TextureTarget)OES.TextureExternalOes, oesTexId);
                gl.TexParameter((TextureTarget)OES.TextureExternalOes, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                gl.TexParameter((TextureTarget)OES.TextureExternalOes, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                gl.TexParameter((TextureTarget)OES.TextureExternalOes, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                gl.TexParameter((TextureTarget)OES.TextureExternalOes, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                float[] vertexData =
                [
                    -1.0f, -1.0f,   0.0f, 0.0f,
                     1.0f, -1.0f,   1.0f, 0.0f,
                    -1.0f,  1.0f,   0.0f, 1.0f,
                     1.0f,  1.0f,   1.0f, 1.0f
                ];

                vboId = gl.GenBuffer();

                int sizeInBytes = vertexData.Length * sizeof(float);
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vboId);
                gl.BufferData(BufferTargetARB.ArrayBuffer, vertexData, BufferUsageARB.StaticDraw);
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

                fboId = gl.GenFramebuffer();

                program = LinkProgram(gl, VertexShaderSource, FragmentShaderSource);

                int posHandle = gl.GetAttribLocation(program, "vPosition");
                int coordHandle = gl.GetAttribLocation(program, "vCoordinate");

                uint stride = 4 * sizeof(float);

                gl.EnableVertexAttribArray(unchecked((uint)posHandle));
                gl.VertexAttribPointer(unchecked((uint)posHandle), 2, VertexAttribPointerType.Float, false, stride, 0);

                gl.EnableVertexAttribArray(unchecked((uint)coordHandle));
                gl.VertexAttribPointer(unchecked((uint)coordHandle), 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

                gl.BindBuffer(BufferTargetARB.PixelPackBuffer, 0);
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                gl.UseProgram(_shaderProgram);
                vTextureHandle = gl.GetUniformLocation(_shaderProgram, "vTexture");
                vMatrixHandle = gl.GetUniformLocation(_shaderProgram, "vMatrix");
            }
            catch
            {
                if (vboId != 0) gl.DeleteBuffers([vboId]);
                if (fboId != 0) gl.DeleteFramebuffers([fboId]);
                if (program != 0) gl.DeleteProgram(program);
                if (oesTexId != 0) gl.DeleteTextures([oesTexId]);
                throw;
            }
        }

        var arcDevice = new Arc<GLContext>(device);
        var output = GLCameraOutput.Create<T>(width, height, color, arcDevice);

        _output = output;

        _fboId = fboId;
        _shaderProgram = program;
        _vboId = vboId;
        _oesTexId = oesTexId;
        _vTextureHandle = vTextureHandle;
        _vMatrixHandle = vMatrixHandle;

        _gl = arcDevice;
    }

    public abstract float[] GetTransformMatrix();

    public void ProcessAndCopyFrame()
    {
        using (_gl.Inner.MakeCurrent())
        {
            float[] transformMatrix = GetTransformMatrix();

            var gl = _gl.Inner.Interface;
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fboId);
            gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, unchecked((uint)_output.GLTexture), 0);
            gl.Viewport(0, 0, (uint)_output.Width, (uint)_output.Height);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            gl.UseProgram(_shaderProgram);

            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture((TextureTarget)OES.TextureExternalOes, _oesTexId);
            gl.Uniform1(_vTextureHandle, 0);
            gl.UniformMatrix4(_vMatrixHandle, 1, false, transformMatrix);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboId);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            gl.DisableVertexAttribArray(0);
            gl.DisableVertexAttribArray(1);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindTexture((TextureTarget)OES.TextureExternalOes, 0);

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gl.Flush();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose(bool disposing) { }

    protected virtual void OnGLRelease() { }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        using (_gl.Inner.MakeCurrent())
        {
            var gl = _gl.Inner.Interface;
            gl.DeleteBuffers([_vboId]);
            gl.DeleteFramebuffers([_fboId]);
            gl.DeleteProgram(_shaderProgram);
            gl.DeleteBuffers([_oesTexId]);

            OnGLRelease();
        }

        OnDispose(disposing);

        _gl.Dispose();

        _isDisposed = true;
    }

    ~GLOesCameraOutputHolder()
    {
        Dispose(false);
    }

    private const string VertexShaderSource = @"#version 300 es
        in vec4 vPosition;
        in vec2 vCoordinate;   
        out vec2 vTexCoordinate;
        uniform mat4 vMatrix;
        void main() {
            gl_Position = vPosition;
            vTexCoordinate = (vMatrix * vec4(vCoordinate, 0.0, 1.0)).xy;
        }";

    private const string FragmentShaderSource = @"#version 300 es
        #extension GL_OES_EGL_image_external_essl3 : require
        precision highp float;
        in vec2 vTexCoordinate;
        out vec4 fragColor;
        uniform samplerExternalOES vTexture;
        void main() {
            fragColor = texture(vTexture, vTexCoordinate);
        }";

    public IGpuDevice? Gpu => _gl.Inner.Device;

    private static uint LinkProgram(Silk.NET.OpenGLES.GL gl, string vertSrc, string fragSrc)
    {
        uint vertexShader = CompileShader(gl, ShaderType.VertexShader, vertSrc);
        uint fragmentShader = CompileShader(gl, ShaderType.FragmentShader, fragSrc);

        uint program = gl.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);

        int[] linkStatus = new int[1];
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, linkStatus);
        if (linkStatus[0] == 0)
        {
            var info = gl.GetProgramInfoLog(program);
            gl.DeleteProgram(program);
            throw new InvalidOperationException($"Shader 链接失败: {info}");
        }

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        return program;
    }

    private static uint CompileShader(Silk.NET.OpenGLES.GL gl, ShaderType type, string source)
    {
        uint shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        int[] compiled = new int[1];
        gl.GetShader(shader, ShaderParameterName.CompileStatus, compiled);
        if (compiled[0] == 0)
        {
            var info = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader);
            throw new InvalidOperationException($"Shader 编译失败 ({type}): {info}");
        }
        return shader;
    }

    public ICameraOutput GetOutput()
    {
        return _output;
    }

    public IDisposableCameraOutput CreateOutput()
    {
        return GLCameraOutput.Create<T>(_output.Width, _output.Height, _output.Color, _gl);
    }

    public async Task CopyTo(IDisposableCameraOutput output)
    {
        if (output is CpuCameraOutput cpuOutput)
        {
            _output.CopyTo(cpuOutput);
        }
        else if (output is GLCameraOutput glOutput)
        {
            _output.CopyTo(glOutput);
        }
    }
}