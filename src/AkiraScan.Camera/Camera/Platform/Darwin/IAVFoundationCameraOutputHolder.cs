using CoreVideo;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Darwin;

internal interface IAVFoundationCameraOutputHolder: ICameraOutputHolder
{
    Task Process(CVPixelBuffer pixels);
}
