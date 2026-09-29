using AkiraScan.Camera.GL;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Multimedia;

namespace AkiraScan.Camera.Platform.Tizen;

internal sealed class TizenCameraProviderFactory : ICameraProviderFactory
{
    public async Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        try
        {
            using var cdm = new CameraDeviceManager();

            return cdm.SupportedDevices
                .Select(x => new CameraDescriptor
                {
                    Facing = x.Device switch
                    {
                        CameraDevice.Rear => CameraFacing.Back,
                        CameraDevice.Front => CameraFacing.Front,
                        _ => CameraFacing.Unspecified,
                    },
                    UniqueId = x.Id,
                })
                .ToArray();
        }
        catch (NotSupportedException)
        {
            return [];
        }
    }

    public async Task<ICameraProvider> OpenCameraAsync(CameraParameters? param)
    {
        param ??= new CameraParameters();

        var gls = param.PreferGpuLists?
            .Select(x => x is IGLDevice device ? GLContext.Create(device) : null)?
            .Where(x => x is not null)?
            .OfType<GLContext>()?
            .ToArray() ?? [];

        GLContext? gl = null;
        if (gls.Length != 0)
        {
            gl = gls[0];
            foreach (var item in gls.Skip(1))
            {
                item.Dispose();
            }
        }

        using var cdm = new CameraDeviceManager();
        var targetId = param.CameraUniqueId;
        var descriptor = targetId is not null ?
            cdm.SupportedDevices.Where(d => d.Id == targetId).OfType<CameraDeviceInformation?>().FirstOrDefault() :
            cdm.SupportedDevices.OfType<CameraDeviceInformation?>().FirstOrDefault();
        return descriptor is null ? throw CameraException.NoValidCameraSource() : TizenCameraProvider.Create(descriptor.Value.Device, gl);
    }
}