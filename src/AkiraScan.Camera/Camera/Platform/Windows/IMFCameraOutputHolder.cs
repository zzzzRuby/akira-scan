using System.Threading.Tasks;
using Windows.Win32.Media.MediaFoundation;

namespace AkiraScan.Camera.Platform.Windows;

internal interface IMFCameraOutputHolder : ICameraOutputHolder
{
    Task UpdateFrame(ComPtr<IMFSample> videoFrame);
}
