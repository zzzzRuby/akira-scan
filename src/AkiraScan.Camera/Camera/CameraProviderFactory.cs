using AkiraScan.Camera.Cpu;
using System;
using System.Runtime.Versioning;

namespace AkiraScan.Camera;

public static class CameraProviderFactory
{
    public static ICameraProviderFactory Create()
    {
#if ANDROID
        return new Platform.Android.AndroidCameraProviderFactory();
#elif IOS || MACOS || TVOS || MACCATALYST
        return new Platform.Darwin.DarwinCameraProviderFactory();
#elif TIZEN
        return new Platform.Tizen.TizenCameraProviderFactory();
#else
        if (OperatingSystem.IsWindowsVersionAtLeast(8))
        {
            return new Platform.Windows.WindowsCameraProviderFactory();
        }
        else if (OperatingSystem.IsLinux())
        {
            return new Platform.Linux.LinuxCameraProviderFactory();
        }
        else if (OperatingSystem.IsBrowser())
        {
            return new Platform.Browser.BrowserCameraProviderFactory();
        }
        else
        {
            return new MockCameraProviderFactory();
        }
#endif
    }

    [SupportedOSPlatformGuard("tizen")]
    internal static bool IsTizen => OperatingSystem.IsOSPlatform("TIZEN");

    public static IDisposableCameraOutput CreateCompatibleCpuOutput(this ICameraProvider provider)
    {
        return provider.Output.CreateCompatibleCpuOutput();
    }

    internal static CpuCameraOutput CreateCompatibleCpuOutput(this ICameraOutput output)
    {
        return new CpuCameraOutput(output.Width, output.Height, output.Color);
    }
}
