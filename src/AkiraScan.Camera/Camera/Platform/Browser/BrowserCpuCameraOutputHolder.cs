using AkiraScan.Camera.Cpu;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Browser;

[SupportedOSPlatform("browser")]
internal sealed class BrowserCpuCameraOutputHolder(int width, int height) : CpuCameraOutputHolder(width, height, CameraColor.Rgba8888)
{
    public async Task UpdateFrame(string cameraId)
    {
        try
        {
            var frameData = BrowserCameraInterop.ReadFrame(cameraId);
            if (frameData == null || frameData.Length == 0)
                return;

            using var l = await _output.UpdateLockAsync();

            int expectedLength = _output.Width * _output.Height * 4;
            if (frameData.Length < expectedLength)
                return;

            frameData.CopyTo(_output.BufferArray);
        }
        catch
        {

        }
    }
}
