using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Com;

namespace AkiraScan.Camera.Platform.Windows;

internal unsafe sealed class ComPtr<T> : IDisposable where T : unmanaged, IComIID
{
    private T* _ptr;
    private bool _disposed;

    private ComPtr(T* ptr, bool addRef = false)
    {
        _ptr = ptr;
        if (addRef && _ptr != null)
            ((IUnknown*)_ptr)->AddRef();
    }

    public T* Ptr => _ptr;

    public ComPtr<U> QueryInterface<U>() where U : unmanaged, IComIID
    {
        if (_disposed || _ptr == null)
            throw new ObjectDisposedException(nameof(ComPtr<T>));

        fixed (Guid* guid = &U.Guid)
        {
            U* result = null;
            var hr = ((IUnknown*)_ptr)->QueryInterface(guid, (void**)&result);
            if (hr.Value < 0) Marshal.ThrowExceptionForHR(hr.Value);
            return new ComPtr<U>(result);
        }
    }

    public ComPtr<U>? QueryInterfaceOrNull<U>() where U : unmanaged, IComIID
    {
        if (_disposed || _ptr == null)
            throw new ObjectDisposedException(nameof(ComPtr<T>));

        fixed (Guid* guid = &U.Guid)
        {
            U* result = null;
            var hr = ((IUnknown*)_ptr)->QueryInterface(guid, (void**)&result);
            if (hr.Value < 0) return null;
            return new ComPtr<U>(result);
        }
    }

    public static ComPtr<T> Attach(T* ptr) => new(ptr, addRef: false);
    public static ComPtr<T> Borrow(T* ptr) => new(ptr, addRef: true);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_ptr != null)
        {
            ((IUnknown*)_ptr)->Release();
            _ptr = null;
        }
        GC.SuppressFinalize(this);
    }

    ~ComPtr()
    {
        if (_ptr != null)
        {
            ((IUnknown*)_ptr)->Release();
            _ptr = null;
        }
    }
}