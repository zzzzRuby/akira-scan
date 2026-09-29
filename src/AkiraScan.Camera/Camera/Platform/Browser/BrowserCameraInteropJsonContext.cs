using System.Text.Json.Serialization;

namespace AkiraScan.Camera.Platform.Browser;

internal sealed class BrowserCameraJsonInfo
{
    public required string UniqueId { get; set; }
    public required string Label { get; set; }
}

[JsonSerializable(typeof(BrowserCameraJsonInfo[]))]
internal sealed partial class BrowserCameraInteropJsonContext: JsonSerializerContext
{
}
