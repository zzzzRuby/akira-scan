using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AkiraScan.Camera.Platform.Linux.V4L2PInvoke;

namespace AkiraScan.Camera.Platform.Linux;

internal sealed class LinuxCameraProviderFactory : ICameraProviderFactory
{
    private static uint MakeFourcc(char a, char b, char c, char d) =>
        (uint)(a | (b << 8) | (c << 16) | (d << 24));

    /*
    private static VulkanGpu? SelectGpu(ModernGpuDevice[] gpus)
    {
        var vulkanGpus = gpus
            .Where(g => g.Gpu == CameraGpu.Vulkan && g.DeviceUuid is { Length: 16 })
            .ToArray();

        if (vulkanGpus.Length == 0) return null;

        var vk = Vk.GetApi();

        // 创建 Instance（仅用于设备发现）
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            ApiVersion = Vk.Version12
        };

        var extensions = new[] { "VK_KHR_external_memory_capabilities", "VK_KHR_get_physical_device_properties2" };
        unsafe
        {
            var extPtr = (byte**)SilkMarshal.StringArrayToPtr(extensions);
            var createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = (uint)extensions.Length,
                PpEnabledExtensionNames = extPtr
            };

            Instance instance;
            if (vk.CreateInstance(&createInfo, null, &instance) != Result.Success)
            {
                SilkMarshal.Free((nint)extPtr);
                vk.Dispose();
                return null;
            }
            SilkMarshal.Free((nint)extPtr);

            uint deviceCount = 0;
            vk.EnumeratePhysicalDevices(instance, &deviceCount, null);
            if (deviceCount == 0)
            {
                vk.DestroyInstance(instance, null);
                vk.Dispose();
                return null;
            }

            var devices = stackalloc PhysicalDevice[(int)deviceCount];
            vk.EnumeratePhysicalDevices(instance, &deviceCount, devices);

            foreach (var modern in vulkanGpus)
            {
                for (uint i = 0; i < deviceCount; i++)
                {
                    var idProps = new PhysicalDeviceIDProperties { SType = StructureType.PhysicalDeviceIDProperties };
                    var props2 = new PhysicalDeviceProperties2
                    {
                        SType = StructureType.PhysicalDeviceProperties2,
                        PNext = &idProps
                    };
                    vk.GetPhysicalDeviceProperties2(devices[i], &props2);

                    bool match = true;
                    for (int j = 0; j < 16; j++)
                    {
                        if (idProps.DeviceUuid[j] != modern.DeviceUuid![j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match) continue;

                    // 查找图形 Queue Family
                    uint queueFamilyCount = 0;
                    vk.GetPhysicalDeviceQueueFamilyProperties(devices[i], &queueFamilyCount, null);
                    var queueFamilies = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
                    vk.GetPhysicalDeviceQueueFamilyProperties(devices[i], &queueFamilyCount, queueFamilies);

                    uint graphicsQueueFamily = uint.MaxValue;
                    for (uint q = 0; q < queueFamilyCount; q++)
                    {
                        if ((queueFamilies[q].QueueFlags & QueueFlags.GraphicsBit) != 0)
                        {
                            graphicsQueueFamily = q;
                            break;
                        }
                    }

                    if (graphicsQueueFamily == uint.MaxValue) continue;

                    float priority = 1.0f;
                    var queueCreateInfo = new DeviceQueueCreateInfo
                    {
                        SType = StructureType.DeviceQueueCreateInfo,
                        QueueFamilyIndex = graphicsQueueFamily,
                        QueueCount = 1,
                        PQueuePriorities = &priority
                    };

                    var devExtensions = new[] { "VK_KHR_external_memory", "VK_KHR_external_memory_fd" };
                    var devExtPtr = (byte**)SilkMarshal.StringArrayToPtr(devExtensions);
                    var deviceCreateInfo = new DeviceCreateInfo
                    {
                        SType = StructureType.DeviceCreateInfo,
                        QueueCreateInfoCount = 1,
                        PQueueCreateInfos = &queueCreateInfo,
                        EnabledExtensionCount = (uint)devExtensions.Length,
                        PpEnabledExtensionNames = devExtPtr
                    };

                    Device device;
                    var result = vk.CreateDevice(devices[i], &deviceCreateInfo, null, &device);
                    SilkMarshal.Free((nint)devExtPtr);
                    if (result != Result.Success) continue;

                    Queue queue;
                    vk.GetDeviceQueue(device, graphicsQueueFamily, 0, &queue);

                    var poolInfo = new CommandPoolCreateInfo
                    {
                        SType = StructureType.CommandPoolCreateInfo,
                        QueueFamilyIndex = graphicsQueueFamily,
                        Flags = CommandPoolCreateFlags.ResetCommandBufferBit
                    };
                    CommandPool commandPool;
                    vk.CreateCommandPool(device, &poolInfo, null, &commandPool);

                    vk.TryGetDeviceExtension(instance, device, out KhrExternalMemoryFd? extFd);

                    return new VulkanGpu(modern, vk, instance, devices[i], device, graphicsQueueFamily, queue, commandPool, extFd);
                }
            }

            vk.DestroyInstance(instance, null);
            vk.Dispose();
        }

        return null;
    }
    */

    public async Task<CameraDescriptor[]> EnumCamerasAsync()
    {
        if (Directory.Exists("/dev"))
        {
            return Directory.EnumerateFiles("/dev", "video*")
               .Order()
               .Select(x => new CameraDescriptor
               {
                   UniqueId = x,
                   Facing = CameraFacing.Unspecified,
               })
               .ToArray();
        }

        return [];
    }

    public async Task<ICameraProvider> OpenCameraAsync(CameraParameters? param)
    {
        param ??= new CameraParameters();

        var uniqueId = param.CameraUniqueId
            ?? (await EnumCamerasAsync()).FirstOrDefault()?.UniqueId
            ?? throw CameraException.NoValidCameraSource();

        var gpus = param.PreferGpuLists?
            .Select(x => x as ModernGpuDevice)?
            .Where(x => x is not null && x.Gpu == CameraGpu.Vulkan)?
            .OfType<ModernGpuDevice>()?
            .ToArray() ?? [];

        var color = CameraColor.Bgra8888;

        return await V4L2CameraProvider.CreateAsync(uniqueId, color);
    }
}