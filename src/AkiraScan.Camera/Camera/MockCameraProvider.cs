using AkiraScan.Camera.Cpu;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace AkiraScan.Camera;

internal sealed class MockCameraProvider : ICameraProvider
{
    private readonly CpuCameraOutput _output;
    private readonly Timer _timer;

    private MockCameraProvider(CpuCameraOutput output)
    {
        _output = output;

        _timer = new Timer(1000.0 / 60.0);
        _timer.Elapsed += TimerElapsed;
    }

    private async void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var random = new Random();

        using (await _output.UpdateLockAsync())
        {
            random.NextBytes(_output.BufferArray);
        }

        AfterFrameUpdate?.Invoke();
    }

    internal static async Task<MockCameraProvider> CreateAsync()
    {
        var output = new CpuCameraOutput(1920, 1080, CameraColor.Rgba8888);

        return new MockCameraProvider(output);
    }

    public ICameraOutput Output => _output;

    public IGpuDevice? Gpu => null;

    public event Action? AfterFrameUpdate;

    public void Dispose()
    {
        _timer.Elapsed -= TimerElapsed;
        _timer.Dispose();
    }

    public async Task StartCameraAsync()
    {
        _timer.Start();
    }

    public async Task StopCameraAsync()
    {
        _timer.Stop();
    }

    public IDisposableCameraOutput CreateCompatibleOutput()
    {
        return new CpuCameraOutput(_output.Width, _output.Height, _output.Color);
    }

    public async Task CopyTo(IDisposableCameraOutput output)
    {
        if (output is CpuCameraOutput cpuOutput)
        {
            await _output.CopyTo(cpuOutput);
        }
    }
}