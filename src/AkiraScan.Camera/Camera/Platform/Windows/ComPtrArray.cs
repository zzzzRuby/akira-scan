using System;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;

namespace AkiraScan.Camera.Platform.Windows;

internal unsafe sealed class ComPtrArray<T>(T** array, uint count) : IDisposable where T : unmanaged
{
    public uint Count => count;

    public T* this[int index]
    {
        get
        {
            if (index < 0 || index >= count) throw new IndexOutOfRangeException();
            return array[index];
        }
    }

    public void Dispose()
    {
        if (array == null) return;
        for (int i = 0; i < count; i++)
        {
            if (array[i] != null)
                ((IUnknown*)array[i])->Release();
        }
        Marshal.FreeCoTaskMem((nint)array);
        array = null;
        GC.SuppressFinalize(this);
    }

    ~ComPtrArray()
    {
        if (array == null) return;
        for (int i = 0; i < count; i++)
        {
            if (array[i] != null)
                ((IUnknown*)array[i])->Release();
        }
        Marshal.FreeCoTaskMem((nint)array);
        array = null;
    }
}
