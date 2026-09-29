using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace AkiraScan.Camera.GL;

internal sealed class EGL
{
    private readonly d_eglGetNativeClientBufferANDROID? _eglGetNativeClientBufferANDROID;
    private readonly d_eglCreateImage _eglCreateImage;
    private readonly d_eglDestroyImage _eglDestroyImage;
    private readonly d_eglGetDisplay _eglGetDisplay;
    private readonly d_eglGetCurrentDisplay _eglGetCurrentDisplay;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_eglGetNativeClientBufferANDROID(IntPtr hardwareBuffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate IntPtr d_eglCreateImage(
        IntPtr dpy,
        IntPtr ctx,
        uint target,
        IntPtr buffer,
        IntPtr* attrib_list
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_eglDestroyImage(IntPtr dpy, IntPtr image);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_eglGetDisplay(IntPtr display_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_eglGetCurrentDisplay();

    private EGL(IntPtr eglGetNativeClientBufferANDROID, IntPtr eglCreateImage, IntPtr eglDestroyImage, IntPtr eglGetDisplay, IntPtr eglGetCurrentDisplay)
    {
        _eglGetNativeClientBufferANDROID = eglGetNativeClientBufferANDROID != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<d_eglGetNativeClientBufferANDROID>(eglGetNativeClientBufferANDROID) : null;
        _eglCreateImage = Marshal.GetDelegateForFunctionPointer<d_eglCreateImage>(eglCreateImage);
        _eglDestroyImage = Marshal.GetDelegateForFunctionPointer<d_eglDestroyImage>(eglDestroyImage);
        _eglGetDisplay = Marshal.GetDelegateForFunctionPointer<d_eglGetDisplay>(eglGetDisplay);
        _eglGetCurrentDisplay = Marshal.GetDelegateForFunctionPointer<d_eglGetCurrentDisplay>(eglGetCurrentDisplay);
    }

    public static EGL? TryCreate(IGLDevice device)
    {
        var eglGetNativeClientBufferANDROID = device.GetProcAddress("eglGetNativeClientBufferANDROID");
        var eglGetDisplay = device.GetProcAddress("eglGetDisplay");
        var eglGetCurrentDisplay = device.GetProcAddress("eglGetCurrentDisplay");
        var eglCreateImage = device.GetProcAddress("eglCreateImage");
        var eglDestroyImage = device.GetProcAddress("eglDestroyImage");

        IntPtr[] funcs = [
            eglGetDisplay,
            eglGetCurrentDisplay,
            eglCreateImage,
            eglDestroyImage,
        ];

        if (funcs.Any(x => x == IntPtr.Zero))
            return null;

        return new EGL(eglGetNativeClientBufferANDROID, eglCreateImage, eglDestroyImage, eglGetDisplay, eglGetCurrentDisplay);
    }

    public bool HasGetNativeClientBufferANDROID => _eglGetNativeClientBufferANDROID is not null;

    public IntPtr GetNativeClientBufferANDROID(IntPtr hardwareBuffer)
    {
        if (_eglGetNativeClientBufferANDROID is null) throw new InvalidOperationException();
        return _eglGetNativeClientBufferANDROID(hardwareBuffer);
    }

    public IntPtr CreateImage(IntPtr dpy, IntPtr ctx, uint target, IntPtr buffer, uint[] attribList)
    {
        unsafe
        {
            var attriListNew = attribList.Select(x => (IntPtr)x).ToArray();
            fixed (IntPtr* attrib = attriListNew)
            {
                return _eglCreateImage(dpy, ctx, target, buffer, attrib);
            }
        }
    }

    public bool DestroyImage(IntPtr dpy, IntPtr image)
    {
        return _eglDestroyImage(dpy, image) == (int)EGLEnum.True;
    }

    public IntPtr GetDisplay(IntPtr displayId)
    {
        return _eglGetDisplay(displayId);
    }

    public IntPtr GetCurrentDisplay()
    {
        return _eglGetCurrentDisplay();
    }
}
