using System;

namespace AkiraScan.Camera;

public enum CameraColor
{
    Bgra8888,
    Rgba8888,
}

public enum CameraRotate
{
    None,
    Rotate90,
    Rotate180,
    Rotate270,
}

public interface ICameraOutput
{
    int Width { get; }

    int Height { get; }

    CameraColor Color { get; }

    CameraRotate Rotate { get; }
}

public interface IDisposableCameraOutput : ICameraOutput, IDisposable
{
}