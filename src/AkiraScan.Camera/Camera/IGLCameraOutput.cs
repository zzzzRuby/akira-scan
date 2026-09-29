using System.Threading;

namespace AkiraScan.Camera;

public interface IGLCameraOutput : ICameraOutput
{
    int GLTexture { get; }

    SemaphoreSlim RenderSemaphore { get; }

    SemaphoreSlim PresentSemaphore { get; }
}