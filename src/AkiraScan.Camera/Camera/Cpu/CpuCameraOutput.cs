using System;
using System.Threading;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Cpu;

internal sealed class CpuCameraOutput(int width, int height, CameraColor color) : ICpuCameraOutput, IDisposableCameraOutput
{
    private readonly byte[] _buffer = new byte[width * height * 4];
    private readonly SemaphoreSlim _renderSemaphore = new(1, 1);
    private readonly SemaphoreSlim _presentSemaphore = new(0, 1);

    public int Width => width;

    public int Height => height;

    public CameraColor Color => color;

    public ReadOnlyMemory<byte> Buffer => _buffer;

    public byte[] BufferArray => _buffer;

    public int Stride => Width * 4;

    public CameraRotate Rotate { get; set; } = CameraRotate.None;

    public void Dispose()
    {
        _renderSemaphore.Dispose();
        _presentSemaphore.Dispose();
    }

    private sealed class SemaphoreReleaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }

    internal async Task<IDisposable> UpdateLockAsync()
    {
        await _renderSemaphore.WaitAsync();
        return new SemaphoreReleaser(_presentSemaphore);
    }

    public async Task<IDisposable> PresentLockAsync()
    {
        await _presentSemaphore.WaitAsync();
        return new SemaphoreReleaser(_renderSemaphore);
    }

    private async Task<IDisposable> EnsurePresent()
    {
        await _presentSemaphore.WaitAsync();
        return new SemaphoreReleaser(_presentSemaphore);
    }

    private async Task<IDisposable> EnsureRender()
    {
        await _renderSemaphore.WaitAsync();
        return new SemaphoreReleaser(_renderSemaphore);
    }

    public async Task CopyTo(CpuCameraOutput output)
    {
        using var l1 = await EnsurePresent();
        using var l2 = await output.EnsureRender();

        Array.Copy(_buffer, output._buffer, _buffer.Length);
    }
}