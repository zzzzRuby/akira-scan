using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Linux;

internal interface IV4L2CameraOutputHolder : ICameraOutputHolder
{
    Task UpdateFrameAsync(V4L2Frame frame);
}