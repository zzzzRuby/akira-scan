using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dxgi;

namespace AkiraScan.Camera.Platform.Windows;

[SupportedOSPlatform("windows8.0")]
internal sealed class WindowsCameraProviderFactory : ICameraProviderFactory
{
    static WindowsCameraProviderFactory()
    {
        var wrappers = new StrategyBasedComWrappers();
        ComWrappers.RegisterForMarshalling(wrappers);
    }

    private static D3D11Gpu? SelectGpu(ModernGpuDevice[] gpus)
    {
        D3D11Gpu? targetDevice = null;
        if (gpus.Length != 0)
        {
            var gpuLuids = gpus
                .Select(x => (x.DeviceLuid, x))
                .Where(x => x.DeviceLuid is not null)
                .OfType<(byte[], ModernGpuDevice)>()
                .Select(x =>
                {
                    var id = BitConverter.ToUInt64(x.Item1);
                    var luid = new LUID
                    {
                        LowPart = (uint)(id & 0xffffffff),
                        HighPart = unchecked((int)(id >> 32))
                    };
                    return (luid, x.Item2);
                })
                .ToArray();

            var lists = new List<ComPtr<IDXGIAdapter1>>();

            using var factory = ComFactory.CreateDXGIFactory1();

            uint adapterIndex = 0;
            while (true)
            {
                var hr = factory.EnumAdapters1(adapterIndex, out var adapterTemp);
                if (hr.Value < 0) break;

                if (adapterTemp != null)
                {
                    lists.Add(adapterTemp);
                    adapterIndex++;
                }
                else
                {
                    break;
                }
            }

            foreach (var (gpuLuid, modernDevice) in gpuLuids)
            {
                int device = -1;
                for (int i = 0; i < lists.Count; i++)
                {
                    var desc = lists[i].GetDesc1();
                    if (desc.AdapterLuid.LowPart == gpuLuid.LowPart && desc.AdapterLuid.HighPart == gpuLuid.HighPart)
                    {
                        device = i;
                        break;
                    }
                }

                if (device != -1)
                {
                    targetDevice = new D3D11Gpu(modernDevice, lists[device]);
                    for (int i = 0; i < lists.Count; i++)
                    {
                        if (i != device)
                        {
                            lists[i].Dispose();
                        }
                    }
                    break;
                }
            }
        }

        return targetDevice;
    }

    public unsafe Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        var ids = new List<CameraDescriptor>();

        using var attributes = ComFactory.MFCreateAttributes(1);

        attributes.SetGUID(PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);

        using var devices = ComFactory.MFEnumDeviceSources(attributes);

        for (int i = 0; i < devices.Count; i++)
        {
            devices[i]->GetAllocatedString(PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, out PWSTR symbolicLink, out uint len);

            try
            {
                if (symbolicLink != null)
                {
                    ids.Add(new CameraDescriptor { UniqueId = new string(symbolicLink, 0, unchecked((int)len)), Facing = CameraFacing.Unspecified });
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem((IntPtr)symbolicLink.Value);
            }
        }

        return Task.FromResult<CameraDescriptor[]>([.. ids]);
    }

    public async Task<ICameraProvider> OpenCameraAsync(CameraParameters? param)
    {
        param ??= new CameraParameters();

        var uniqueId = param.CameraUniqueId ?? (await EnumCamerasAsync()).FirstOrDefault()?.UniqueId ?? throw CameraException.NoValidCameraSource();
        var gpus = param.PreferGpuLists?
            .Select(x => x as ModernGpuDevice)?
            .Where(x => x is not null && x.Gpu == CameraGpu.D3D11)?
            .OfType<ModernGpuDevice>()?
            .ToArray() ?? [];

        var gpu = SelectGpu(gpus);

        return await MFCameraProvider.CreateAsync(uniqueId, gpu);
    }
}
