using AkiraScan.Camera.Cpu;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.Media.MediaFoundation;

namespace AkiraScan.Camera.Platform.Windows;

[SupportedOSPlatform("windows8.0")]
internal sealed class MFD3D11CameraOutput : IGpuCameraOutput, IDisposableCameraOutput
{
    private readonly ComPtr<ID3D11Texture2D> _texture;
    private readonly ComPtr<ID3D11Texture2D> _stagedTexture;
    private readonly SafeFileHandle _sharedHandle;
    private readonly MFD3D11CameraOutputLock _lock;
    private readonly CameraColor _color;
    private readonly int _width, _height;

    public nint Handle => _sharedHandle.DangerousGetHandle();
    public int Width => _width;
    public int Height => _height;
    public CameraColor Color => _color;
    public IGpuCameraOutputLock Lock => _lock;
    public CameraGpu Gpu => CameraGpu.D3D11;
    public CameraRotate Rotate { get; set; } = CameraRotate.None;

    private MFD3D11CameraOutput(
        ComPtr<ID3D11Texture2D> texture,
        ComPtr<ID3D11Texture2D> stagedTexture,
        CameraColor color,
        int width,
        int height)
    {
        _texture = texture;
        _stagedTexture = stagedTexture;
        _color = color;
        _width = width;
        _height = height;

        var keyedMutex = _texture.QueryInterface<IDXGIKeyedMutex>();

        _lock = new MFD3D11CameraOutputLock(keyedMutex);

        var dxgiResource = texture.QueryInterface<IDXGIResource1>();

        _sharedHandle = dxgiResource.CreateSharedHandle(null, GENERIC_ACCESS_RIGHTS.GENERIC_READ, null);

        dxgiResource.Dispose();
    }

    public static MFD3D11CameraOutput Create(ComPtr<ID3D11Device> device, int width, int height, CameraColor color)
    {
        var sharedDesc = new D3D11_TEXTURE2D_DESC
        {
            Width = (uint)width,
            Height = (uint)height,
            Format = color switch
            {
                CameraColor.Bgra8888 => DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                CameraColor.Rgba8888 => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
                _ => throw new NotSupportedException()
            },
            BindFlags = D3D11_BIND_FLAG.D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_FLAG.D3D11_BIND_RENDER_TARGET,
            Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT,
            MiscFlags = D3D11_RESOURCE_MISC_FLAG.D3D11_RESOURCE_MISC_SHARED_NTHANDLE | D3D11_RESOURCE_MISC_FLAG.D3D11_RESOURCE_MISC_SHARED_KEYEDMUTEX,
            SampleDesc = new DXGI_SAMPLE_DESC { Count = 1, Quality = 0 },
            MipLevels = 1,
            ArraySize = 1,
            CPUAccessFlags = 0,
        };

        var sharedTexture = device.CreateTexture2D(sharedDesc, null);

        var stagingDesc = new D3D11_TEXTURE2D_DESC
        {
            Width = sharedDesc.Width,
            Height = sharedDesc.Height,
            Format = sharedDesc.Format,
            BindFlags = 0,
            Usage = D3D11_USAGE.D3D11_USAGE_STAGING,
            MiscFlags = 0,
            SampleDesc = new DXGI_SAMPLE_DESC { Count = 1, Quality = 0 },
            MipLevels = 1,
            ArraySize = 1,
            CPUAccessFlags = D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_READ,
        };

        var stagingTexture = device.CreateTexture2D(stagingDesc, null);

        return new MFD3D11CameraOutput(sharedTexture, stagingTexture, color, width, height);
    }

    public void Dispose()
    {
        _sharedHandle.Dispose();
        _lock.Dispose();
        _texture.Dispose();
        _stagedTexture.Dispose();
    }

    public void FrameUpdate(ComPtr<IMFSample> pSample, ComPtr<ID3D11Device> device)
    {
        using var mediaBuffer = pSample.GetBufferByIndex(0);
        using var dxgiBuffer = mediaBuffer.QueryInterface<IMFDXGIBuffer>();
        using var sourceTexture = dxgiBuffer.GetResource<ID3D11Texture2D>();

        _lock.AcquireAtRender();
        try
        {
            using var context = device.GetImmediateContext();
            context.CopyResource(_texture, sourceTexture);
            context.CopyResource(_stagedTexture, sourceTexture);
        }
        finally
        {
            _lock.ReleaseAtRender();
        }
    }

    public void CopyTo(CpuCameraOutput output, ComPtr<ID3D11Device> device)
    {
        using var context = device.GetImmediateContext();

        context.CopyResource(_stagedTexture, _texture);

        var mapped = context.Map(_stagedTexture, 0, D3D11_MAP.D3D11_MAP_READ, 0);
        try
        {
            unsafe
            {
                fixed (byte* dest = output.BufferArray)
                {
                    for (int i = 0; i < Height; i++)
                    {
                        Buffer.MemoryCopy((byte*)mapped.pData + i * mapped.RowPitch, dest + i * output.Stride, output.Stride, mapped.RowPitch);
                    }
                }
            }
        }
        finally
        {
            context.Unmap(_stagedTexture, 0);
        }
    }

    public void CopyTo(MFD3D11CameraOutput output, ComPtr<ID3D11Device> device)
    {
        using var context = device.GetImmediateContext();
        context.CopyResource(output._texture, _texture);
    }
}
