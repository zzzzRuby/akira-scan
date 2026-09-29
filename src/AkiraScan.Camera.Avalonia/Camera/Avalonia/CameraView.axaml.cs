using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AkiraScan.Camera.Avalonia;

public sealed partial class CameraView : UserControl
{
    private ICameraProvider? _camera;
    private ICameraView? _view;
    private readonly ICameraProviderFactory _cameraFactory = CameraProviderFactory.Create();

    public CameraView()
    {
        InitializeComponent();
    }

    protected override Size MeasureOverride(Size finalSize)
    {
        if (_camera is not null)
        {
            var output = _camera.Output;

            double x1 = output.Width;
            double y1 = output.Height;
            double x_img = output.Rotate switch
            {
                CameraRotate.None => output.Width,
                CameraRotate.Rotate90 => output.Height,
                CameraRotate.Rotate180 => output.Width,
                CameraRotate.Rotate270 => output.Height,
                _ => throw new NotImplementedException(),
            };
            double y_img = output.Rotate switch
            {
                CameraRotate.None => output.Height,
                CameraRotate.Rotate90 => output.Width,
                CameraRotate.Rotate180 => output.Height,
                CameraRotate.Rotate270 => output.Width,
                _ => throw new NotImplementedException(),
            };

            var x2 = finalSize.Width;
            var y2 = finalSize.Height;

            var scale = Math.Min(x2 / x_img, y2 / y_img);

            var visual = _view?.Visual;
            visual?.CenterPoint = new Vector3D(x1 * scale / 2, y1 * scale / 2, 0.0);
            visual?.RotationAngle = output.Rotate switch
            {
                CameraRotate.None => 0.0f,
                CameraRotate.Rotate90 => (float)(Math.PI / 2),
                CameraRotate.Rotate180 => (float)Math.PI,
                CameraRotate.Rotate270 => (float)(Math.PI / 2 * 3),
                _ => throw new NotImplementedException(),
            };
            visual?.Offset = new Vector3D((x2 - x1 * scale) / 2, (y2 - y1 * scale) / 2, 0);
            visual?.Size = new Vector(x1 * scale, y1 * scale);
            _view?.OnMeasure(x1 * scale, y1 * scale);
            return finalSize;
        }
        return base.MeasureOverride(finalSize);
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var elementVisual = ElementComposition.GetElementVisual(this);
        if (elementVisual == null) return;
        var compositor = elementVisual.Compositor;

        var gpuInterop = await compositor.TryGetCompositionGpuInterop();

        List<IGpuDevice>? supportGpu = null;
        if (gpuInterop is not null)
        {
            var list = new List<IGpuDevice>();
            if (gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle))
                list.Add(new ModernGpuDevice(CameraGpu.D3D11, gpuInterop.DeviceLuid, gpuInterop.DeviceUuid));

            if (gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.IOSurfaceRef) &&
                gpuInterop.SupportedSemaphoreTypes.Contains(KnownPlatformGraphicsExternalSemaphoreHandleTypes.MetalSharedEvent))
                list.Add(new ModernGpuDevice(CameraGpu.Metal, gpuInterop.DeviceLuid, gpuInterop.DeviceUuid));

            if (OperatingSystem.IsWindows())
            {
                if (gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaqueNtHandle) &&
                    gpuInterop.SupportedSemaphoreTypes.Contains(KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaqueNtHandle))
                    list.Add(new ModernGpuDevice(CameraGpu.Vulkan, gpuInterop.DeviceLuid, gpuInterop.DeviceUuid));
            }
            else
            {
                if (gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaquePosixFileDescriptor) &&
                    gpuInterop.SupportedSemaphoreTypes.Contains(KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaquePosixFileDescriptor))
                    list.Add(new ModernGpuDevice(CameraGpu.Vulkan, gpuInterop.DeviceLuid, gpuInterop.DeviceUuid));
            }

            supportGpu = list;
        }

        var glFeature = await compositor.TryGetRenderInterfaceFeature(typeof(IOpenGlTextureSharingRenderInterfaceContextFeature)) as IOpenGlTextureSharingRenderInterfaceContextFeature;
        if (glFeature is not null && glFeature.CanCreateSharedContext)
        {
            var glContext = glFeature.CreateSharedContext();

            if (glContext is not null)
            {
                var device = new AvaloniaGLDevice(glContext);
                supportGpu ??= [];
                supportGpu.Add(device);
            }
        }

        _camera ??= await _cameraFactory.OpenCameraAsync(new CameraParameters
        {
            PreferGpuLists = supportGpu?.ToArray(),
        });

        if (_camera.Output is IGpuCameraOutput output)
        {
            if (gpuInterop is null)
                throw new NotSupportedException("gpu render is not supported");

            _view = CameraViewGpu.Setup(compositor, gpuInterop, output);
            _camera.AfterFrameUpdate += _view.OnAfterFrameUpdate;
            ElementComposition.SetElementChildVisual(this, _view.Visual);
        }
        else if (_camera.Output is IGLCameraOutput glOutput)
        {
            if (gpuInterop is null)
                throw new NotSupportedException("gpu render is not supported");
            if (glFeature is null)
                throw new NotSupportedException("gpu render is not supported");
            if (_camera.Gpu is not AvaloniaGLDevice device)
                throw new NotSupportedException("gpu render is not supported");

            _view = CameraViewGL.Setup(compositor, device.Inner, gpuInterop, glFeature, glOutput);
            _camera.AfterFrameUpdate += _view.OnAfterFrameUpdate;
            ElementComposition.SetElementChildVisual(this, _view.Visual);
        }
        else if (_camera.Output is ICpuCameraOutput cpuOutput)
        {
            _view = CameraViewCpu.Setup(compositor, cpuOutput);
            _camera.AfterFrameUpdate += _view.OnAfterFrameUpdate;
            ElementComposition.SetElementChildVisual(this, _view.Visual);
        }

        await _camera.StartCameraAsync();

        InvalidateMeasure();
    }

    protected override async void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_camera is not null)
        {
            await _camera.StopCameraAsync();
            _camera.Dispose();
            _camera = null;
        }
        _view?.Dispose();
        _view = null;
        base.OnDetachedFromVisualTree(e);
    }
}