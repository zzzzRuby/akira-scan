using AkiraScan.Camera;
using Metal;

internal sealed class AVFoundationMetalCameraOutputLock(IMTLSharedEvent copyEvent, IMTLSharedEvent presentEvent) : ITimelineSemaphoresGpuCameraOutputLock
{
    public nint RenderLock => copyEvent.Handle;
    public nint PresentLock => presentEvent.Handle;

    public ulong PresentWait => presentEvent.SignaledValue;
    public ulong RenderWait => copyEvent.SignaledValue;

    public void Dispose()
    {
        copyEvent.Dispose();
        presentEvent.Dispose();
    }

    internal void AcquireAtRender(IMTLCommandBuffer commandBuffer)
    {
        commandBuffer.EncodeWait(copyEvent, copyEvent.SignaledValue);
    }

    internal void ReleaseAtRender(IMTLCommandBuffer commandBuffer)
    {
        presentEvent.SignaledValue++;
        commandBuffer.EncodeSignal(presentEvent, presentEvent.SignaledValue);
    }
}