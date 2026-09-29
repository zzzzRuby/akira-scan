using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace AkiraScan.Camera.Avalonia;

internal sealed class CameraViewGpu : ICameraView
{
    private readonly ICameraBuffer _cameraBuffer;
    private readonly CompositionSurfaceVisual _visual;
    private readonly CompositionDrawingSurface _surface;

    public CompositionContainerVisual Visual => _visual;

    internal interface ICameraBuffer
    {
        Task Update(CompositionDrawingSurface surface);
    }

    private class CameraBuffer(ICompositionImportedGpuImage image) : ICameraBuffer
    {
        public async Task Update(CompositionDrawingSurface surface)
        {
            await surface.UpdateAsync(image);
        }
    }

    private sealed class KeyedMutexCameraBuffer(ICompositionImportedGpuImage image, IKeyedMutexGpuCameraOutputLock gpuLock) : ICameraBuffer
    {
        public async Task Update(CompositionDrawingSurface surface)
        {
            await surface.UpdateWithKeyedMutexAsync(image, gpuLock.PresentLock, gpuLock.RenderLock);
        }
    }

    private sealed class SemaphoresCameraBuffer : ICameraBuffer
    {
        private readonly ICompositionImportedGpuImage _image;
        private readonly ICompositionImportedGpuSemaphore _waitFor, _signal;

        private SemaphoresCameraBuffer(ICompositionImportedGpuImage image, ICompositionImportedGpuSemaphore waitFor, ICompositionImportedGpuSemaphore signal)
        {
            _waitFor = waitFor;
            _signal = signal;
            _image = image;
        }

        public static SemaphoresCameraBuffer Create(ICompositionGpuInterop gpuInterop, ICompositionImportedGpuImage image, ISemaphoresGpuCameraOutputLock gpuLock, string? descriptor)
        {
            var waitFor = gpuInterop.ImportSemaphore(new PlatformHandle(gpuLock.PresentLock, descriptor));
            var signal = gpuInterop.ImportSemaphore(new PlatformHandle(gpuLock.RenderLock, descriptor));
            return new SemaphoresCameraBuffer(image, waitFor, signal);
        }

        public async Task Update(CompositionDrawingSurface surface)
        {
            await surface.UpdateWithSemaphoresAsync(_image, _waitFor, _signal);
        }
    }

    private sealed class TimelineSemaphoresCameraBuffer : ICameraBuffer
    {
        private readonly ICompositionImportedGpuImage _image;
        private readonly ICompositionImportedGpuSemaphore _waitFor, _signal;
        private readonly ITimelineSemaphoresGpuCameraOutputLock _lock;

        private TimelineSemaphoresCameraBuffer(ICompositionImportedGpuImage image, ICompositionImportedGpuSemaphore waitFor, ICompositionImportedGpuSemaphore signal, ITimelineSemaphoresGpuCameraOutputLock @lock)
        {
            _waitFor = waitFor;
            _signal = signal;
            _image = image;
            _lock = @lock;
        }

        public static TimelineSemaphoresCameraBuffer Create(ICompositionGpuInterop gpuInterop, ICompositionImportedGpuImage image, ITimelineSemaphoresGpuCameraOutputLock gpuLock, string? descriptor)
        {
            var waitFor = gpuInterop.ImportSemaphore(new PlatformHandle(gpuLock.PresentLock, descriptor));
            var signal = gpuInterop.ImportSemaphore(new PlatformHandle(gpuLock.RenderLock, descriptor));
            return new TimelineSemaphoresCameraBuffer(image, waitFor, signal, gpuLock);
        }

        public async Task Update(CompositionDrawingSurface surface)
        {
            await surface.UpdateWithTimelineSemaphoresAsync(_image, _waitFor, _lock.PresentWait, _signal, _lock.RenderWait);
        }
    }

    private CameraViewGpu(ICameraBuffer cameraBuffer, CompositionSurfaceVisual visual, CompositionDrawingSurface surface)
    {
        _cameraBuffer = cameraBuffer;
        _visual = visual;
        _surface = surface;
    }

    public static CameraViewGpu Setup(Compositor compositor, ICompositionGpuInterop gpuInterop, IGpuCameraOutput output)
    {
        var surface = compositor.CreateDrawingSurface();
        var surfaceVisual = compositor.CreateSurfaceVisual();
        surfaceVisual.Surface = surface;

        var imageDescriptor = output.Gpu switch
        {
            CameraGpu.D3D11 => KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle,
            CameraGpu.Metal => KnownPlatformGraphicsExternalImageHandleTypes.IOSurfaceRef,
            CameraGpu.Vulkan => OperatingSystem.IsWindows() switch
            {
                false => KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaquePosixFileDescriptor,
                true => KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaqueNtHandle,
            },
            _ => throw new NotSupportedException("gpu render is not supported")
        };

        var semaphoreDescriptor = output.Gpu switch
        {
            CameraGpu.D3D11 => null,
            CameraGpu.Metal => KnownPlatformGraphicsExternalSemaphoreHandleTypes.MetalSharedEvent,
            CameraGpu.Vulkan => OperatingSystem.IsWindows() switch
            {
                false => KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaquePosixFileDescriptor,
                true => KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaqueNtHandle,
            },
            _ => throw new NotSupportedException("gpu render is not supported")
        };

        var importedImage = gpuInterop.ImportImage(
            new PlatformHandle(output.Handle, imageDescriptor),
            new PlatformGraphicsExternalImageProperties
            {
                Format = output.Color switch
                {
                    CameraColor.Rgba8888 => PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                    CameraColor.Bgra8888 => PlatformGraphicsExternalImageFormat.B8G8R8A8UNorm,
                    _ => throw new NotSupportedException(),
                },
                Width = output.Width,
                Height = output.Height,
                TopLeftOrigin = true
            });

        ICameraBuffer cameraBuffer = output.Lock switch
        {
            IKeyedMutexGpuCameraOutputLock keyedMutexLock => new KeyedMutexCameraBuffer(importedImage, keyedMutexLock),
            ISemaphoresGpuCameraOutputLock semaphoresLock => SemaphoresCameraBuffer.Create(gpuInterop, importedImage, semaphoresLock, semaphoreDescriptor),
            ITimelineSemaphoresGpuCameraOutputLock timelineSemaphoresLock => TimelineSemaphoresCameraBuffer.Create(gpuInterop, importedImage, timelineSemaphoresLock, semaphoreDescriptor),
            _ => new CameraBuffer(importedImage)
        };

        return new CameraViewGpu(cameraBuffer, surfaceVisual, surface);
    }

    public void OnAfterFrameUpdate()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await _cameraBuffer.Update(_surface);
        });
    }

    public void Dispose()
    {
    }

    public void OnMeasure(double width, double height)
    {
    }
}
