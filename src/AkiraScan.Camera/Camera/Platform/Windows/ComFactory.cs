using System;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct3D;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Media.MediaFoundation;

namespace AkiraScan.Camera.Platform.Windows;

internal static unsafe class ComFactory
{
    [SupportedOSPlatform("windows6.0.6000")]
    public static ComPtr<IMFAttributes> MFCreateAttributes(uint cInitialSize)
    {
        IMFAttributes* p = null;
        PInvoke.MFCreateAttributes(&p, cInitialSize).ThrowOnFailure();
        return ComPtr<IMFAttributes>.Attach(p);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public static ComPtr<IMFMediaType> MFCreateMediaType()
    {
        IMFMediaType* p = null;
        PInvoke.MFCreateMediaType(&p).ThrowOnFailure();
        return ComPtr<IMFMediaType>.Attach(p);
    }

    [SupportedOSPlatform("windows6.1")]
    public static ComPtr<IMFMediaSource> MFCreateDeviceSource(ComPtr<IMFAttributes> pAttributes)
    {
        IMFMediaSource* p = null;
        PInvoke.MFCreateDeviceSource(pAttributes.Ptr, &p).ThrowOnFailure();
        return ComPtr<IMFMediaSource>.Attach(p);;
    }

    [SupportedOSPlatform("windows6.1")]
    public static ComPtrArray<IMFActivate> MFEnumDeviceSources(ComPtr<IMFAttributes> pAttributes)
    {
        IMFActivate** devices = null;
        uint count = 0;
        PInvoke.MFEnumDeviceSources(pAttributes.Ptr, &devices, &count).ThrowOnFailure();
        return new ComPtrArray<IMFActivate>(devices, count);
    }

    [SupportedOSPlatform("windows8.0")]
    public static ComPtr<IMFDXGIDeviceManager> MFCreateDXGIDeviceManager(out uint resetToken)
    {
        uint token = 0;
        IMFDXGIDeviceManager* p = null;
        PInvoke.MFCreateDXGIDeviceManager(&token, &p).ThrowOnFailure();
        resetToken = token;
        return ComPtr<IMFDXGIDeviceManager>.Attach(p);
    }

    [SupportedOSPlatform("windows6.1")]
    public static ComPtr<IMFSourceReader> MFCreateSourceReaderFromMediaSource(
        ComPtr<IMFMediaSource> pMediaSource,
        ComPtr<IMFAttributes> pAttributes)
    {
        IMFSourceReader* p = null;
        PInvoke.MFCreateSourceReaderFromMediaSource(pMediaSource.Ptr, pAttributes.Ptr, &p).ThrowOnFailure();
        return ComPtr<IMFSourceReader>.Attach(p);
    }

    [SupportedOSPlatform("windows6.1")]
    public static ComPtr<IDXGIFactory1> CreateDXGIFactory1()
    {
        PInvoke.CreateDXGIFactory1<IDXGIFactory1>(out var ptr).ThrowOnFailure();
        return ComPtr<IDXGIFactory1>.Attach(ptr);
    }

    [SupportedOSPlatform("windows6.1")]
    public static ComPtr<ID3D11Device> D3D11CreateDevice(
        ComPtr<IDXGIAdapter1> pAdapter,
        D3D_DRIVER_TYPE DriverType,
        HMODULE Software,
        D3D11_CREATE_DEVICE_FLAG Flags,
        ReadOnlySpan<D3D_FEATURE_LEVEL> pFeatureLevels)
    {
        ID3D11Device* device = null;

        HRESULT hr;
        fixed (D3D_FEATURE_LEVEL* pLevels = pFeatureLevels)
        {
            hr = PInvoke.D3D11CreateDevice(
                (IDXGIAdapter*)pAdapter.Ptr, DriverType, Software, Flags,
                pLevels, (uint)pFeatureLevels.Length,
                PInvoke.D3D11_SDK_VERSION,
                &device, null, null);
        }
        hr.ThrowOnFailure();

        return ComPtr<ID3D11Device>.Attach(device);
    }
}