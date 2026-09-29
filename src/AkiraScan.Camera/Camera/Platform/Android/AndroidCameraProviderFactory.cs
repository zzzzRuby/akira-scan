using AkiraScan.Camera.GL;
using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Android;

internal sealed class AndroidCameraProviderFactory : ICameraProviderFactory
{
    public async Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        if (Application.Context.GetSystemService(Context.CameraService) is not CameraManager cameraManager)
            return [];

        var result = new List<CameraDescriptor>();
        foreach (var id in cameraManager.GetCameraIdList())
        {
            var chars = cameraManager.GetCameraCharacteristics(id);
            var facing = chars.Get(CameraCharacteristics.LensFacing);
            var facingVal = LensFacing.External;

            result.Add(new CameraDescriptor
            {
                UniqueId = id,
                Facing = facingVal switch
                {
                    LensFacing.External => CameraFacing.Unspecified,
                    LensFacing.Back => CameraFacing.Back,
                    LensFacing.Front => CameraFacing.Front,
                    _ => throw new NotSupportedException(),
                }
            });
        }
        return [.. result];
    }

    private static string DefaultCamera(CameraManager cameraManager)
    {
        var list = cameraManager.GetCameraIdList();
        if (list.Length == 0) throw CameraException.NoValidCameraSource();
        return list[0];
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

        var cameraManager = Application.Context.GetSystemService(Context.CameraService) as CameraManager ?? throw CameraException.NoValidCameraSource();
        var cameraId = param.CameraUniqueId ?? DefaultCamera(cameraManager);
        var characteristics = cameraManager.GetCameraCharacteristics(cameraId);

        return await AndroidCameraProvider.CreateAsync(cameraManager, cameraId, characteristics, gl);
    }
}
