using AkiraScan.Camera.Cpu;
using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Linux;

internal sealed class V4L2CpuCameraOutputHolder(int width, int height, CameraColor color) : CpuCameraOutputHolder(width, height, color), IV4L2CameraOutputHolder
{
    public async Task UpdateFrameAsync(V4L2Frame frame)
    {
        using var l = await _output.UpdateLockAsync();
    }
}