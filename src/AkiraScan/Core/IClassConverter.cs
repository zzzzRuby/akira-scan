using System;

namespace AkiraScan.Core;

public interface IClassConverter<T> where T: struct, Enum
{
    static abstract T? Convert(int klass);
}
