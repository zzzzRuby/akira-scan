using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AkiraScan.Camera.Platform.Android;

[SupportedOSPlatform("android26.0")]
internal sealed partial class AHardwareBuffer : SafeHandle
{
    public AHardwareBuffer() : base(IntPtr.Zero, ownsHandle: true) { }

    private AHardwareBuffer(IntPtr handle): base(handle, ownsHandle: true) { }

    public static AHardwareBuffer Allocate(AHardwareBuffer_Desc desc)
    {
        unsafe
        {
            IntPtr buffer = IntPtr.Zero;
            int result = Methods.AHardwareBuffer_allocate(&desc, &buffer);
            if (result != 0 || buffer == IntPtr.Zero)
                throw new InvalidOperationException($"AHardwareBuffer_allocate failed with code {result}");

            return new AHardwareBuffer(buffer);
        }
    }

    public IntPtr NativeHandle => handle;

    public AHardwareBuffer_Desc Describe()
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        AHardwareBuffer_Desc desc;
        unsafe
        {
            Methods.AHardwareBuffer_describe(handle, &desc);
        }
        return desc;
    }

    public IntPtr Lock(AHardwareBuffer_UsageFlags usage, ARect? rect = null)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        IntPtr virtualAddress = IntPtr.Zero;
        unsafe
        {
            int result;
            if (rect.HasValue)
            {
                ARect r = rect.Value;
                result = Methods.AHardwareBuffer_lock(handle, (ulong)usage, -1, &r, &virtualAddress);
            }
            else
            {
                result = Methods.AHardwareBuffer_lock(handle, (ulong)usage, -1, null, &virtualAddress);
            }

            if (result != 0)
                throw new InvalidOperationException($"AHardwareBuffer_lock failed with code {result}");
        }
        return virtualAddress;
    }

    public void Unlock()
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        unsafe
        {
            int result = Methods.AHardwareBuffer_unlock(handle, null);
            if (result != 0)
                throw new InvalidOperationException($"AHardwareBuffer_unlock failed with code {result}");
        }
    }

    [SupportedOSPlatform("android31.0")]
    public ulong GetId()
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        ulong id = 0;
        unsafe
        {
            int result = Methods.AHardwareBuffer_getId(handle, &id);
            if (result != 0)
                throw new InvalidOperationException($"AHardwareBuffer_getId failed with code {result}");
        }
        return id;
    }

    public void SendToUnixSocket(int fd)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        int result = Methods.AHardwareBuffer_sendHandleToUnixSocket(handle, fd);
        if (result != 0)
            throw new InvalidOperationException($"AHardwareBuffer_sendHandleToUnixSocket failed with code {result}");
    }

    public static AHardwareBuffer RecvFromUnixSocket(int fd)
    {
        IntPtr handle = IntPtr.Zero;
        unsafe
        {
            int result = Methods.AHardwareBuffer_recvHandleFromUnixSocket(fd, &handle);
            if (result != 0)
                throw new InvalidOperationException($"AHardwareBuffer_recvHandleFromUnixSocket failed with code {result}");
        }
        return new AHardwareBuffer(handle);
    }

    [SupportedOSPlatform("android29.0")]
    public AHardwareBuffer_Planes LockPlanes(AHardwareBuffer_UsageFlags usage, ARect? rect = null)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        AHardwareBuffer_Planes planes = default;
        unsafe
        {
            int result;
            if (rect.HasValue)
            {
                ARect r = rect.Value;
                result = Methods.AHardwareBuffer_lockPlanes(handle, (ulong)usage, -1, &r, &planes);
            }
            else
            {
                result = Methods.AHardwareBuffer_lockPlanes(handle, (ulong)usage, -1, null, &planes);
            }

            if (result != 0)
                throw new InvalidOperationException($"AHardwareBuffer_lockPlanes failed with code {result}");
        }
        return planes;
    }

    [SupportedOSPlatform("android29.0")]
    public static bool IsSupported(AHardwareBuffer_Desc desc)
    {
        unsafe
        {
            return Methods.AHardwareBuffer_isSupported(&desc) != 0;
        }
    }

    [SupportedOSPlatform("android29.0")]
    public IntPtr LockAndGetInfo(AHardwareBuffer_UsageFlags usage, out int bytesPerPixel, out int bytesPerStride)
    {
        return LockAndGetInfo(usage, null, out bytesPerPixel, out bytesPerStride);
    }

    [SupportedOSPlatform("android29.0")]
    public IntPtr LockAndGetInfo(AHardwareBuffer_UsageFlags usage, ARect? rect, out int bytesPerPixel, out int bytesPerStride)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);

        IntPtr virtualAddress = IntPtr.Zero;
        int bpp = 0;
        int bps = 0;
        unsafe
        {
            int result;
            if (rect.HasValue)
            {
                ARect r = rect.Value;
                result = Methods.AHardwareBuffer_lockAndGetInfo(handle, (ulong)usage, -1, &r, &virtualAddress, &bpp, &bps);
            }
            else
            {
                result = Methods.AHardwareBuffer_lockAndGetInfo(handle, (ulong)usage, -1, null, &virtualAddress, &bpp, &bps);
            }

            if (result != 0)
                throw new InvalidOperationException($"AHardwareBuffer_lockAndGetInfo failed with code {result}");
        }

        bytesPerPixel = bpp;
        bytesPerStride = bps;
        return virtualAddress;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (handle != IntPtr.Zero)
        {
            Methods.AHardwareBuffer_release(handle);
            handle = IntPtr.Zero;
        }
        return true;
    }
}