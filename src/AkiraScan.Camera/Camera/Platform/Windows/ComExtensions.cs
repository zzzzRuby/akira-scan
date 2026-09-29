using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Media.MediaFoundation;
using Windows.Win32.Security;
using Windows.Win32.System.Com;

namespace AkiraScan.Camera.Platform.Windows;

internal static class ComExtensions
{
    [SupportedOSPlatform("windows6.1")]
    public unsafe static HRESULT EnumAdapters1(this ComPtr<IDXGIFactory1> factory, uint index, out ComPtr<IDXGIAdapter1>? adapter)
    {
        IDXGIAdapter1* result = null;
        HRESULT hr = factory.Ptr->EnumAdapters1(index, &result);
        adapter = result != null && hr.Succeeded ? ComPtr<IDXGIAdapter1>.Attach(result) : null;
        return hr;
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static DXGI_ADAPTER_DESC1 GetDesc1(this ComPtr<IDXGIAdapter1> device)
    {
        return device.Ptr->GetDesc1();
    }

    public unsafe static void AcquireSync(this ComPtr<IDXGIKeyedMutex> mutex, ulong index, uint time)
    {
        mutex.Ptr->AcquireSync(index, time);
    }

    public unsafe static void ReleaseSync(this ComPtr<IDXGIKeyedMutex> mutex, ulong index)
    {
        mutex.Ptr->ReleaseSync(index);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static ComPtr<ID3D11DeviceContext> GetImmediateContext(this ComPtr<ID3D11Device> device)
    {
        ID3D11DeviceContext* context = null;
        device.Ptr->GetImmediateContext(&context);
        return ComPtr<ID3D11DeviceContext>.Attach(context);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static ComPtr<ID3D11Texture2D> CreateTexture2D(this ComPtr<ID3D11Device> device, in D3D11_TEXTURE2D_DESC desc, D3D11_SUBRESOURCE_DATA? subDesc)
    {
        ID3D11Texture2D* result = null;
        device.Ptr->CreateTexture2D(desc, subDesc, &result);
        return ComPtr<ID3D11Texture2D>.Attach(result);
    }

    [SupportedOSPlatform("windows8.0")]
    public unsafe static SafeFileHandle CreateSharedHandle(this ComPtr<IDXGIResource1> resource, SECURITY_ATTRIBUTES? attributes, GENERIC_ACCESS_RIGHTS access, string? name)
    {
        resource.Ptr->CreateSharedHandle(attributes, (uint)access, name, out var result);
        return result;
    }

    public unsafe static void SetMultithreadProtected(this ComPtr<ID3D11Multithread> multithread, BOOL value)
    {
        multithread.Ptr->SetMultithreadProtected(value);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static void CopyResource(this ComPtr<ID3D11DeviceContext> context, ComPtr<ID3D11Texture2D> dest, ComPtr<ID3D11Texture2D> src)
    {
        context.Ptr->CopyResource((ID3D11Resource*)dest.Ptr, (ID3D11Resource*)src.Ptr);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static D3D11_MAPPED_SUBRESOURCE Map(this ComPtr<ID3D11DeviceContext> context, ComPtr<ID3D11Texture2D> resource, uint subresource, D3D11_MAP mapType, uint mapFlags)
    {
        D3D11_MAPPED_SUBRESOURCE mapped = default;
        context.Ptr->Map((ID3D11Resource*)resource.Ptr, subresource, mapType, mapFlags, &mapped);
        return mapped;
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static void Unmap(this ComPtr<ID3D11DeviceContext> context, ComPtr<ID3D11Texture2D> resource, uint subresource)
    {
        context.Ptr->Unmap((ID3D11Resource*)resource.Ptr, subresource);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void SetGUID(this ComPtr<IMFAttributes> attributes, in Guid guid, in Guid value)
    {
        attributes.Ptr->SetGUID(guid, value);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void SetString(this ComPtr<IMFAttributes> attributes, in Guid guid, string value)
    {
        attributes.Ptr->SetString(guid, value);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void SetUINT32(this ComPtr<IMFAttributes> attributes, in Guid guid, uint value)
    {
        attributes.Ptr->SetUINT32(guid, value);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void SetUnknown<T>(this ComPtr<IMFAttributes> attributes, in Guid guid, ComPtr<T> value) where T : unmanaged, IComIID
    {
        attributes.Ptr->SetUnknown(guid, (IUnknown*)value.Ptr);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void SetUnknown(this ComPtr<IMFAttributes> attributes, in Guid guid, IntPtr value)
    {
        attributes.Ptr->SetUnknown(guid, (IUnknown*)value);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void SetGUID(this ComPtr<IMFMediaType> mediaType, in Guid guid, in Guid value)
    {
        mediaType.Ptr->SetGUID(guid, value);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static ulong GetUINT64(this ComPtr<IMFMediaType> mediaType, in Guid guid)
    {
        mediaType.Ptr->GetUINT64(guid, out var result);
        return result;
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static void SetCurrentMediaType(this ComPtr<IMFSourceReader> reader, MF_SOURCE_READER_CONSTANTS index, ComPtr<IMFMediaType> mediaType)
    {
        reader.Ptr->SetCurrentMediaType((uint)index, mediaType.Ptr);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static ComPtr<IMFMediaType> GetCurrentMediaType(this ComPtr<IMFSourceReader> reader, MF_SOURCE_READER_CONSTANTS index)
    {
        IMFMediaType* result = null;
        reader.Ptr->GetCurrentMediaType((uint)index, &result);
        return ComPtr<IMFMediaType>.Attach(result);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static void ReadSampleAsyncMode(this ComPtr<IMFSourceReader> reader, MF_SOURCE_READER_CONSTANTS dwStreamIndex, MF_SOURCE_READER_CONTROL_FLAG dwControlFlags)
    {
        reader.Ptr->ReadSample((uint)dwStreamIndex, (uint)dwControlFlags);
    }

    [SupportedOSPlatform("windows6.1")]
    public unsafe static void Flush(this ComPtr<IMFSourceReader> reader, MF_SOURCE_READER_CONSTANTS dwStreamIndex)
    {
        reader.Ptr->Flush((uint)dwStreamIndex);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static ComPtr<IMFMediaBuffer> ConvertToContiguousBuffer(this ComPtr<IMFSample> sample)
    {
        IMFMediaBuffer* result = null;
        sample.Ptr->ConvertToContiguousBuffer(&result);
        return ComPtr<IMFMediaBuffer>.Attach(result);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static ComPtr<IMFMediaBuffer> GetBufferByIndex(this ComPtr<IMFSample> sample, uint index)
    {
        IMFMediaBuffer* result = null;
        sample.Ptr->GetBufferByIndex(index, &result);
        return ComPtr<IMFMediaBuffer>.Attach(result);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void Lock(this ComPtr<IMFMediaBuffer> buffer, out IntPtr ptr, out uint maxLength, out uint currentLength)
    {
        buffer.Ptr->Lock(out var temp, out maxLength, out currentLength);
        ptr = (IntPtr)temp;
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void Unlock(this ComPtr<IMFMediaBuffer> buffer)
    {
        buffer.Ptr->Unlock();
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void Lock2D(this ComPtr<IMF2DBuffer> buffer, out IntPtr ptr, out int pitch)
    {
        buffer.Ptr->Lock2D(out var temp, out pitch);
        ptr = (IntPtr)temp;
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe static void Unlock2D(this ComPtr<IMF2DBuffer> buffer)
    {
        buffer.Ptr->Unlock2D();
    }

    [SupportedOSPlatform("windows8.0")]
    public unsafe static ComPtr<T> GetResource<T>(this ComPtr<IMFDXGIBuffer> dxgiBuffer) where T: unmanaged, IComIID
    {
        dxgiBuffer.Ptr->GetResource(T.Guid, out var result);
        return ComPtr<T>.Attach((T*)result);
    }

    [SupportedOSPlatform("windows8.0")]
    public unsafe static void ResetDevice<T>(this ComPtr<IMFDXGIDeviceManager> manager, ComPtr<T> device, uint resetToken) where T: unmanaged, IComIID
    {
        manager.Ptr->ResetDevice((IUnknown*)device.Ptr, resetToken);
    }
}
