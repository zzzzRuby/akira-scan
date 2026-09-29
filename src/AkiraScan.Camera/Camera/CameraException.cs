using System;

namespace AkiraScan.Camera;

public sealed class CameraException : Exception
{
    public CameraException() : base()
    {
    }

    public CameraException(string message) : base(message)
    {
    }

    public static CameraException NoValidCameraSource()
    {
        return new CameraException("no valid camera source.");
    }
}
