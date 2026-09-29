using System;
using System.Threading;

namespace AkiraScan.Camera.Utils;

internal sealed class Arc<T>(T obj): IDisposable where T: class, IDisposable
{
    private int _value = 1;

    public Arc<T> Clone()
    {
        Interlocked.Increment(ref _value);
        return this;
    }

    public T Inner => obj;

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _value) == 0)
        {
            obj.Dispose();
        }
    }
}
