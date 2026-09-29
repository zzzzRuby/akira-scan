using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera;

public interface ICpuCameraOutput : ICameraOutput
{
    ReadOnlyMemory<byte> Buffer { get; }

    int Stride { get; }

    Task<IDisposable> PresentLockAsync();
}