using AkiraScan.Camera.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AkiraScan.Camera.Platform.Android;

[StructLayout(LayoutKind.Sequential)]
internal partial struct ARect
{
    [NativeTypeName("int32_t")]
    public int left;

    [NativeTypeName("int32_t")]
    public int top;

    [NativeTypeName("int32_t")]
    public int right;

    [NativeTypeName("int32_t")]
    public int bottom;
}

[NativeTypeName("unsigned int")]
internal enum AHardwareBuffer_Format : uint
{
    AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM = 1,
    AHARDWAREBUFFER_FORMAT_R8G8B8X8_UNORM = 2,
    AHARDWAREBUFFER_FORMAT_R8G8B8_UNORM = 3,
    AHARDWAREBUFFER_FORMAT_R5G6B5_UNORM = 4,
    AHARDWAREBUFFER_FORMAT_R16G16B16A16_FLOAT = 0x16,
    AHARDWAREBUFFER_FORMAT_R10G10B10A2_UNORM = 0x2b,
    AHARDWAREBUFFER_FORMAT_BLOB = 0x21,
    AHARDWAREBUFFER_FORMAT_D16_UNORM = 0x30,
    AHARDWAREBUFFER_FORMAT_D24_UNORM = 0x31,
    AHARDWAREBUFFER_FORMAT_D24_UNORM_S8_UINT = 0x32,
    AHARDWAREBUFFER_FORMAT_D32_FLOAT = 0x33,
    AHARDWAREBUFFER_FORMAT_D32_FLOAT_S8_UINT = 0x34,
    AHARDWAREBUFFER_FORMAT_S8_UINT = 0x35,
    AHARDWAREBUFFER_FORMAT_Y8Cb8Cr8_420 = 0x23,
    AHARDWAREBUFFER_FORMAT_YCbCr_P010 = 0x36,
    AHARDWAREBUFFER_FORMAT_YCbCr_P210 = 0x3c,
    AHARDWAREBUFFER_FORMAT_R8_UNORM = 0x38,
    AHARDWAREBUFFER_FORMAT_R16_UINT = 0x39,
    AHARDWAREBUFFER_FORMAT_R16G16_UINT = 0x3a,
    AHARDWAREBUFFER_FORMAT_R10G10B10A10_UNORM = 0x3b,
}

[NativeTypeName("unsigned long")]
internal enum AHardwareBuffer_UsageFlags : ulong
{
    AHARDWAREBUFFER_USAGE_CPU_READ_NEVER = 0U,
    AHARDWAREBUFFER_USAGE_CPU_READ_RARELY = 2U,
    AHARDWAREBUFFER_USAGE_CPU_READ_OFTEN = 3U,
    AHARDWAREBUFFER_USAGE_CPU_READ_MASK = 0xFU,
    AHARDWAREBUFFER_USAGE_CPU_WRITE_NEVER = 0U << 4,
    AHARDWAREBUFFER_USAGE_CPU_WRITE_RARELY = 2U << 4,
    AHARDWAREBUFFER_USAGE_CPU_WRITE_OFTEN = 3U << 4,
    AHARDWAREBUFFER_USAGE_CPU_WRITE_MASK = 0xFU << 4,
    AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE = 1U << 8,
    AHARDWAREBUFFER_USAGE_GPU_FRAMEBUFFER = 1U << 9,
    AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT = AHARDWAREBUFFER_USAGE_GPU_FRAMEBUFFER,
    AHARDWAREBUFFER_USAGE_COMPOSER_OVERLAY = 1UL << 11,
    AHARDWAREBUFFER_USAGE_PROTECTED_CONTENT = 1U << 14,
    AHARDWAREBUFFER_USAGE_VIDEO_ENCODE = 1U << 16,
    AHARDWAREBUFFER_USAGE_SENSOR_DIRECT_DATA = 1U << 23,
    AHARDWAREBUFFER_USAGE_GPU_DATA_BUFFER = 1U << 24,
    AHARDWAREBUFFER_USAGE_GPU_CUBE_MAP = 1U << 25,
    AHARDWAREBUFFER_USAGE_GPU_MIPMAP_COMPLETE = 1U << 26,
    AHARDWAREBUFFER_USAGE_FRONT_BUFFER = 1UL << 32,
    AHARDWAREBUFFER_USAGE_VENDOR_0 = 1UL << 28,
    AHARDWAREBUFFER_USAGE_VENDOR_1 = 1UL << 29,
    AHARDWAREBUFFER_USAGE_VENDOR_2 = 1UL << 30,
    AHARDWAREBUFFER_USAGE_VENDOR_3 = 1UL << 31,
    AHARDWAREBUFFER_USAGE_VENDOR_4 = 1UL << 48,
    AHARDWAREBUFFER_USAGE_VENDOR_5 = 1UL << 49,
    AHARDWAREBUFFER_USAGE_VENDOR_6 = 1UL << 50,
    AHARDWAREBUFFER_USAGE_VENDOR_7 = 1UL << 51,
    AHARDWAREBUFFER_USAGE_VENDOR_8 = 1UL << 52,
    AHARDWAREBUFFER_USAGE_VENDOR_9 = 1UL << 53,
    AHARDWAREBUFFER_USAGE_VENDOR_10 = 1UL << 54,
    AHARDWAREBUFFER_USAGE_VENDOR_11 = 1UL << 55,
    AHARDWAREBUFFER_USAGE_VENDOR_12 = 1UL << 56,
    AHARDWAREBUFFER_USAGE_VENDOR_13 = 1UL << 57,
    AHARDWAREBUFFER_USAGE_VENDOR_14 = 1UL << 58,
    AHARDWAREBUFFER_USAGE_VENDOR_15 = 1UL << 59,
    AHARDWAREBUFFER_USAGE_VENDOR_16 = 1UL << 60,
    AHARDWAREBUFFER_USAGE_VENDOR_17 = 1UL << 61,
    AHARDWAREBUFFER_USAGE_VENDOR_18 = 1UL << 62,
    AHARDWAREBUFFER_USAGE_VENDOR_19 = 1UL << 63,
}

[StructLayout(LayoutKind.Sequential)]
internal partial struct AHardwareBuffer_Desc
{
    [NativeTypeName("uint32_t")]
    public uint width;

    [NativeTypeName("uint32_t")]
    public uint height;

    [NativeTypeName("uint32_t")]
    public uint layers;

    [NativeTypeName("uint32_t")]
    public uint format;

    [NativeTypeName("uint64_t")]
    public ulong usage;

    [NativeTypeName("uint32_t")]
    public uint stride;

    [NativeTypeName("uint32_t")]
    public uint rfu0;

    [NativeTypeName("uint64_t")]
    public ulong rfu1;
}

[StructLayout(LayoutKind.Sequential)]
internal partial struct AHardwareBuffer_Plane
{
    [NativeTypeName("void * _Nullable")]
    public IntPtr data;

    [NativeTypeName("uint32_t")]
    public uint pixelStride;

    [NativeTypeName("uint32_t")]
    public uint rowStride;
}

[StructLayout(LayoutKind.Sequential)]
internal partial struct AHardwareBuffer_Planes
{
    [NativeTypeName("uint32_t")]
    public uint planeCount;

    [NativeTypeName("AHardwareBuffer_Plane[4]")]
    public _planes_e__FixedBuffer planes;

    [InlineArray(4)]
    public partial struct _planes_e__FixedBuffer
    {
        public AHardwareBuffer_Plane e0;
    }
}

internal static unsafe partial class Methods
{
    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial int AHardwareBuffer_allocate([NativeTypeName("const AHardwareBuffer_Desc * _Nonnull")] AHardwareBuffer_Desc* desc, [NativeTypeName("AHardwareBuffer * _Nullable * _Nonnull")] IntPtr* outBuffer);
    
    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial void AHardwareBuffer_acquire([NativeTypeName("AHardwareBuffer * _Nonnull")] IntPtr buffer);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial void AHardwareBuffer_release([NativeTypeName("AHardwareBuffer * _Nonnull")] IntPtr buffer);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial void AHardwareBuffer_describe([NativeTypeName("const AHardwareBuffer * _Nonnull")] IntPtr buffer, [NativeTypeName("AHardwareBuffer_Desc * _Nonnull")] AHardwareBuffer_Desc* outDesc);
    
    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial int AHardwareBuffer_lock([NativeTypeName("AHardwareBuffer * _Nonnull")] IntPtr buffer, [NativeTypeName("uint64_t")] ulong usage, [NativeTypeName("int32_t")] int fence, [NativeTypeName("const ARect * _Nullable")] ARect* rect, [NativeTypeName("void * _Nullable * _Nonnull")] IntPtr* outVirtualAddress);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial int AHardwareBuffer_unlock([NativeTypeName("AHardwareBuffer * _Nonnull")] IntPtr buffer, [NativeTypeName("int32_t * _Nullable")] int* fence);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial int AHardwareBuffer_sendHandleToUnixSocket([NativeTypeName("const AHardwareBuffer * _Nonnull")] IntPtr buffer, int socketFd);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial int AHardwareBuffer_recvHandleFromUnixSocket(int socketFd, [NativeTypeName("AHardwareBuffer * _Nullable * _Nonnull")] IntPtr* outBuffer);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android29.0")]
    public static partial int AHardwareBuffer_lockPlanes([NativeTypeName("AHardwareBuffer * _Nonnull")] IntPtr buffer, [NativeTypeName("uint64_t")] ulong usage, [NativeTypeName("int32_t")] int fence, [NativeTypeName("const ARect * _Nullable")] ARect* rect, [NativeTypeName("AHardwareBuffer_Planes * _Nonnull")] AHardwareBuffer_Planes* outPlanes);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android29.0")]
    public static partial int AHardwareBuffer_isSupported([NativeTypeName("const AHardwareBuffer_Desc * _Nonnull")] AHardwareBuffer_Desc* desc);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android29.0")]
    public static partial int AHardwareBuffer_lockAndGetInfo([NativeTypeName("AHardwareBuffer * _Nonnull")] IntPtr buffer, [NativeTypeName("uint64_t")] ulong usage, [NativeTypeName("int32_t")] int fence, [NativeTypeName("const ARect * _Nullable")] ARect* rect, [NativeTypeName("void * _Nullable * _Nonnull")] IntPtr* outVirtualAddress, [NativeTypeName("int32_t * _Nonnull")] int* outBytesPerPixel, [NativeTypeName("int32_t * _Nonnull")] int* outBytesPerStride);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android31.0")]
    public static partial int AHardwareBuffer_getId([NativeTypeName("const AHardwareBuffer * _Nonnull")] IntPtr buffer, [NativeTypeName("uint64_t * _Nonnull")] ulong* outId);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [SupportedOSPlatform("android26.0")]
    public static partial IntPtr AHardwareBuffer_fromHardwareBuffer([NativeTypeName("JNIEnv *")] IntPtr env, [NativeTypeName("jobject")] IntPtr hardwareBufferObj);

    [LibraryImport("libandroid.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: NativeTypeName("jobject")]
    [SupportedOSPlatform("android26.0")]
    public static partial IntPtr AHardwareBuffer_toHardwareBuffer([NativeTypeName("JNIEnv *")] IntPtr env, IntPtr hardwareBuffer);
}