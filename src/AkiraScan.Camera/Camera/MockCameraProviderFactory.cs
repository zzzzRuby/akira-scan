using System.Threading.Tasks;

namespace AkiraScan.Camera;

public sealed class MockCameraProviderFactory : ICameraProviderFactory
{
    private const string DEFAULT_ID = "Mock";

    public async Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        return [
            new CameraDescriptor
            {
                UniqueId = DEFAULT_ID,
                Facing = CameraFacing.Unspecified,
            }
        ];
    }

    public async Task<ICameraProvider> OpenCameraAsync(CameraParameters? param)
    {
        var uniqueId = param?.CameraUniqueId ?? DEFAULT_ID;
        if (uniqueId != DEFAULT_ID)
            throw CameraException.NoValidCameraSource();
        return await MockCameraProvider.CreateAsync();
    }
}