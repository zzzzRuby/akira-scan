using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;

namespace AkiraScan.Core;

public interface IScanner
{
    List<ScanResult<T>> Scan<T, C>(Tensor<byte> images, bool is_bgra) where T : struct, Enum where C : IClassConverter<T>;
}