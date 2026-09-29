using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Browser;

[SupportedOSPlatform("browser")]
internal sealed class BrowserCameraProviderFactory : ICameraProviderFactory
{
    public async Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        await BrowserCameraInterop.Initialize();
        try
        {
            var json = await BrowserCameraInterop.EnumerateDevices();

            var infos = JsonSerializer.Deserialize(json, BrowserCameraInteropJsonContext.Default.BrowserCameraJsonInfoArray);

            return infos?
                .Select(info => new CameraDescriptor
                {
                    UniqueId = info.UniqueId,
                    Facing = CameraFacing.Unspecified
                })
                .ToArray() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ICameraProvider> OpenCameraAsync(CameraParameters? param)
    {
        await BrowserCameraInterop.Initialize();
        param ??= new CameraParameters();

        string? deviceId = param.CameraUniqueId;
        int width = 1920;
        int height = 1080;

        if (param.BackendArgs is not null)
        {
            if (param.BackendArgs.TryGetValue("width", out var w) && w is int wi)
                width = wi;
            if (param.BackendArgs.TryGetValue("height", out var h) && h is int hi)
                height = hi;
        }

        if (string.IsNullOrEmpty(deviceId))
        {
            var cameras = await EnumCamerasAsync();
            deviceId = cameras.FirstOrDefault()?.UniqueId;
        }

        return await BrowserCameraProvider.CreateAsync(deviceId, width, height);
    }
}