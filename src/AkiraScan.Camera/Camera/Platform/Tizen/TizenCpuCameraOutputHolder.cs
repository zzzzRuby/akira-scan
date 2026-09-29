using AkiraScan.Camera.Cpu;
using System;
using Tizen.Multimedia;

namespace AkiraScan.Camera.Platform.Tizen;

internal sealed class TizenCpuCameraOutputHolder(int width, int height, CameraColor color): CpuCameraOutputHolder(width, height, color), ITizenCameraOutputHolder
{
    public event Action? AfterFrameUpdate;

    public void SetupCamera(global::Tizen.Multimedia.Camera camera)
    {
        camera.Preview += OnPreviewFrame;
    }

    public void UnsetupCamera(global::Tizen.Multimedia.Camera camera)
    {
        camera.Preview -= OnPreviewFrame;
    }

    private async void OnPreviewFrame(object? sender, PreviewEventArgs frame)
    {
        var preview = frame.Preview;

        using (var l = await _output.UpdateLockAsync())
        {
            //TODO
            //int length = Math.Min(frame.Length, _output.BufferArray.Length);
            //Array.Copy(frame, _output.BufferArray, length);
        }

        AfterFrameUpdate?.Invoke();
    }
}
