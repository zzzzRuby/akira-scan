using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Browser;

[SupportedOSPlatform("browser")]
internal static partial class BrowserCameraInterop
{
    private const string JsModule = "akirascan_camera";

    [JSImport("enumerateDevices", JsModule)]
    internal static partial Task<string> EnumerateDevices();

    [JSImport("startCamera", JsModule)]
    internal static partial Task<string> StartCamera(string? deviceId, int width, int height);

    [JSImport("stopCamera", JsModule)]
    internal static partial void StopCamera(string cameraId);

    [JSImport("getWidth", JsModule)]
    internal static partial int GetWidth(string cameraId);

    [JSImport("getHeight", JsModule)]
    internal static partial int GetHeight(string cameraId);

    [JSImport("readFrame", JsModule)]
    internal static partial byte[] ReadFrame(string cameraId);

    [JSImport("startFrameLoop", JsModule)]
    internal static partial void StartFrameLoop(string cameraId, [JSMarshalAs<JSType.Function>] Action callback);

    [JSImport("stopFrameLoop", JsModule)]
    internal static partial void StopFrameLoop(string cameraId);

    private static readonly Lazy<Task> _initializer = new(async () =>
    {
        await JSHost.ImportAsync(JsModule, "../_content/AkiraScan.Camera/interop.js");
    });
    
    public static async Task Initialize()
    {
        await _initializer.Value;
    }
}