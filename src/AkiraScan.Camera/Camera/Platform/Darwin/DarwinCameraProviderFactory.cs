using AVFoundation;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Darwin;

[SupportedOSPlatform("tvos17.0")]
[SupportedOSPlatform("ios")]
[SupportedOSPlatform("macos")]
internal sealed class DarwinCameraProviderFactory : ICameraProviderFactory
{
    public async Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        var discoverySession = AVCaptureDeviceDiscoverySession.Create(
            [AVCaptureDeviceType.BuiltInWideAngleCamera],
            AVMediaTypes.Video,
            AVCaptureDevicePosition.Unspecified
        );

        return discoverySession.Devices
            .Select(x => new CameraDescriptor
            {
                UniqueId = x.UniqueID,
                Facing = x.Position switch
                {
                    AVCaptureDevicePosition.Unspecified => CameraFacing.Unspecified,
                    AVCaptureDevicePosition.Back => CameraFacing.Back,
                    AVCaptureDevicePosition.Front => CameraFacing.Front,
                    _ => throw new NotSupportedException()
                }
            })
            .ToArray();
    }

    public async Task<ICameraProvider> OpenCameraAsync(CameraParameters? param)
    {
        param ??= new CameraParameters();
        var gpus = param.PreferGpuLists?
            .Select(x => x as ModernGpuDevice)?
            .Where(x => x is not null && x.Gpu == CameraGpu.Metal)?
            .OfType<ModernGpuDevice>()?
            .ToArray() ?? [];

        var videoDevice = (param.CameraUniqueId is not null ? AVCaptureDevice.DeviceWithUniqueID(param.CameraUniqueId) : AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video)) ?? throw CameraException.NoValidCameraSource();

        return await AVFoundationCameraProvider.CreateAsync(videoDevice, gpus);
    }
}
