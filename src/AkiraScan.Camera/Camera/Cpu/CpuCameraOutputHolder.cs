using System.Threading.Tasks;

namespace AkiraScan.Camera.Cpu;

internal abstract class CpuCameraOutputHolder(int width, int height, CameraColor color): ICameraOutputHolder
{
    protected CpuCameraOutput _output = new(width, height, color);

    public IGpuDevice? Gpu => null;

    public ICameraOutput GetOutput()
    {
        return _output;
    }

    public IDisposableCameraOutput CreateOutput()
    {
        return _output.CreateCompatibleCpuOutput();
    }

    public async Task CopyTo(IDisposableCameraOutput output)
    {
        if (output is CpuCameraOutput cpuOutput)
        {
            await _output.CopyTo(cpuOutput);
        }
    }

    public virtual void OnDispose() { }

    public void Dispose()
    {
        OnDispose();
        _output.Dispose();
    }
}