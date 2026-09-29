using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct3D;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Media.MediaFoundation;

namespace AkiraScan.Camera.Platform.Windows;

[Guid("DEEC8D99-FA1D-4D82-84C2-2C8969944867"), GeneratedComInterface]
[SupportedOSPlatform("windows6.1")]
internal partial interface IMFSourceReaderCallbackPolyfill
{
    [PreserveSig]
    int OnReadSample(int hrStatus, uint dwStreamIndex, uint dwStreamFlags, long llTimestamp, IntPtr pSample);

    [PreserveSig]
    int OnFlush(uint dwStreamIndex);

    [PreserveSig]
    int OnEvent(uint dwStreamIndex, IntPtr pEvent);
}

[SupportedOSPlatform("windows8.0")]
internal sealed partial class MFCameraProvider : ICameraProvider
{
    [GeneratedComClass]
    public sealed partial class MFCameraProviderCallback : IMFSourceReaderCallbackPolyfill
    {
        private volatile TaskCompletionSource? _flushTcs;

        public event Action? AfterFrameUpdate;

        private IMFCameraOutputHolder? _output;
        private ComPtr<IMFSourceReader>? _sourceReader;

        public ComPtr<IMFSourceReader>? SourceReader
        {
            get => Interlocked.CompareExchange(ref _sourceReader, null, null);
            set => Interlocked.Exchange(ref _sourceReader, value);
        }

        public void Initialize(IMFCameraOutputHolder output)
        {
            _output = output;
        }

        [PreserveSig]
        public unsafe int OnReadSample(int hrStatus, uint dwStreamIndex, uint dwStreamFlags, long llTimestamp, IntPtr pSample)
        {
            var hr = new HRESULT(hrStatus);
            if (hr.Value < 0)
            {
                RequestNextFrame();
                return HRESULT.S_OK;
            }

            try
            {
                if (pSample != IntPtr.Zero && _output is not null)
                {
                    _ = OnReadSampleAsync(ComPtr<IMFSample>.Borrow((IMFSample*)pSample));
                }
            }
            finally
            {
                RequestNextFrame();
            }

            return HRESULT.S_OK;
        }

        private async Task OnReadSampleAsync(ComPtr<IMFSample> pSample)
        {
            try
            {
                await _output!.UpdateFrame(pSample);
                AfterFrameUpdate?.Invoke();
            }
            finally
            {
                pSample.Dispose();
            }
        }

        [PreserveSig]
        public int OnEvent(uint dwStreamIndex, IntPtr pEvent)
        {
            return HRESULT.S_OK;
        }

        public Task WaitForFlushAsync()
        {
            var tcs = new TaskCompletionSource();
            _flushTcs = tcs;
            return tcs.Task;
        }

        [PreserveSig]
        public int OnFlush(uint dwStreamIndex)
        {
            _flushTcs?.TrySetResult();
            return HRESULT.S_OK;
        }

        private void RequestNextFrame()
        {
            var reader = SourceReader;
            reader?.ReadSampleAsyncMode(MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM, 0);
        }
    }

    private readonly IMFCameraOutputHolder _output;
    private readonly MFCameraProviderCallback _callback;
    private readonly ComPtr<IMFSourceReader> _sourceReader;

    public ICameraOutput Output => _output.GetOutput();
    public IGpuDevice? Gpu => _output.Gpu;

    public event Action? AfterFrameUpdate
    {
        add => _callback.AfterFrameUpdate += value;
        remove => _callback.AfterFrameUpdate -= value;
    }

    private MFCameraProvider(
        IMFCameraOutputHolder output,
        MFCameraProviderCallback callback,
        ComPtr<IMFSourceReader> sourceReader)
    {
        _output = output;
        _callback = callback;
        _sourceReader = sourceReader;
    }

    public static Task<MFCameraProvider> CreateAsync(string sourceGroupId, D3D11Gpu? adapter)
    {
        using var deviceAttributes = ComFactory.MFCreateAttributes(2);

        deviceAttributes.SetGUID(PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
        deviceAttributes.SetString(PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, sourceGroupId);

        using var mediaSource = ComFactory.MFCreateDeviceSource(deviceAttributes);

        using var attributes = ComFactory.MFCreateAttributes(3);

        var callback = new MFCameraProviderCallback();
        var wrappers = new StrategyBasedComWrappers();
        nint callbackUnk = wrappers.GetOrCreateComInterfaceForObject(
            callback,
            CreateComInterfaceFlags.None);

        attributes.SetUnknown(PInvoke.MF_SOURCE_READER_ASYNC_CALLBACK, callbackUnk);
        attributes.SetUINT32(PInvoke.MF_SOURCE_READER_ENABLE_ADVANCED_VIDEO_PROCESSING, 1);

        (ComPtr<ID3D11Device>, ModernGpuDevice)? device = null;

        if (adapter is not null)
        {
            var featureLevels = new[] { D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0 };

            var outDevice = ComFactory.D3D11CreateDevice(
                adapter.DXGI,
                D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_UNKNOWN,
                default,
                D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT | D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_VIDEO_SUPPORT,
                featureLevels);

            var multithread = outDevice.QueryInterface<ID3D11Multithread>();
            multithread.SetMultithreadProtected(new BOOL(1));
            multithread.Dispose();

            using var dxgiManager = ComFactory.MFCreateDXGIDeviceManager(out var resetToken);
            dxgiManager.ResetDevice(outDevice, resetToken);
            attributes.SetUnknown(PInvoke.MF_SOURCE_READER_D3D_MANAGER, dxgiManager);

            device = (outDevice, adapter.Device);
        }

        var reader = ComFactory.MFCreateSourceReaderFromMediaSource(mediaSource, attributes);

        using var partialType = ComFactory.MFCreateMediaType();

        partialType.SetGUID(PInvoke.MF_MT_MAJOR_TYPE, PInvoke.MFMediaType_Video);
        partialType.SetGUID(PInvoke.MF_MT_SUBTYPE, PInvoke.MFVideoFormat_ARGB32);

        reader.SetCurrentMediaType(MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM, partialType);

        using var finalType = reader.GetCurrentMediaType(MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM);

        ulong frameSize = finalType.GetUINT64(PInvoke.MF_MT_FRAME_SIZE);

        var width = (int)(frameSize >> 32);
        var height = (int)(frameSize & 0xFFFFFFFF);

        IMFCameraOutputHolder output = device != null
            ? MFD3D11CameraOutputHolder.Create(device.Value.Item2, device.Value.Item1, width, height, CameraColor.Bgra8888)
            : new MFCpuCameraOutputHolder(width, height, CameraColor.Bgra8888);

        var provider = new MFCameraProvider(output, callback, reader);
        callback.Initialize(output);

        return Task.FromResult(provider);
    }

    public Task StartCameraAsync()
    {
        _callback.SourceReader = _sourceReader;
        _sourceReader.ReadSampleAsyncMode(MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM, 0);
        return Task.CompletedTask;
    }

    public async Task StopCameraAsync()
    {
        _callback.SourceReader = null;

        var flushTask = _callback.WaitForFlushAsync();

        _sourceReader.Flush(MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM);

        await flushTask;
    }

    public void Dispose()
    {
        _callback.SourceReader = null;

        _sourceReader.Dispose();
        _output.Dispose();
    }

    public IDisposableCameraOutput CreateCompatibleOutput()
    {
        return _output.CreateOutput();
    }

    public Task CopyTo(IDisposableCameraOutput output)
    {
        return _output.CopyTo(output);
    }
}
