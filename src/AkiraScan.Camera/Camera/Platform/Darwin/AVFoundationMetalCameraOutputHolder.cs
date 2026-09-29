using AkiraScan.Camera.Cpu;
using CoreVideo;
using Metal;
using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Platform.Darwin;

internal sealed class AVFoundationMetalCameraOutputHolder : IAVFoundationCameraOutputHolder
{
    private readonly MetalGpu _device;
    private readonly IMTLCommandQueue _commandQueue;
    private readonly AVFoundationMetalCameraOutput _output;

    private AVFoundationMetalCameraOutputHolder(MetalGpu device, IMTLCommandQueue commandQueue, AVFoundationMetalCameraOutput output)
    {
        _device = device;
        _commandQueue = commandQueue;
        _output = output;
    }

    public static AVFoundationMetalCameraOutputHolder Create(MetalGpu device, int width, int height, CameraColor color)
    {
        var commandQueue = device.MTLDevice.CreateCommandQueue() ?? throw new InvalidOperationException("Failed to create Metal command queue");
        var output = AVFoundationMetalCameraOutput.Create(device.MTLDevice, width, height, color);

        return new AVFoundationMetalCameraOutputHolder(device, commandQueue, output);
    }

    public IGpuDevice? Gpu => _device.Device;

    public Task CopyTo(IDisposableCameraOutput output)
    {
        if (output is CpuCameraOutput cpuOutput)
        {
            _output.CopyTo(cpuOutput);
        }
        else if (output is AVFoundationMetalCameraOutput mtlOutput)
        {
            _output.CopyTo(mtlOutput, _commandQueue);
        }

        return Task.CompletedTask;
    }

    public IDisposableCameraOutput CreateOutput()
    {
        return AVFoundationMetalCameraOutput.Create(_device.MTLDevice, _output.Width, _output.Height, _output.Color);
    }

    public void Dispose()
    {
        _output.Dispose();
        _device.Dispose();
    }

    public ICameraOutput GetOutput()
    {
        return _output;
    }

    public Task Process(CVPixelBuffer pixels)
    {
        _output.Process(pixels, _commandQueue);
        return Task.CompletedTask;
    }
}
