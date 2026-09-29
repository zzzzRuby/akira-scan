using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera;

internal interface ICameraOutputHolder: IDisposable
{
    IGpuDevice? Gpu { get; }

    ICameraOutput GetOutput();

    IDisposableCameraOutput CreateOutput();

    Task CopyTo(IDisposableCameraOutput output);
}
