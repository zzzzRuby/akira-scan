using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;

namespace AkiraScan.Core;

public sealed class Scanner : IScanner, IDisposable
{
    private readonly InferenceSession _session;

    private Scanner(InferenceSession session)
    {
        _session = session;
    }

    private static void TryIgnoreCatch(Action action)
    {
        try
        {
            action();
        }
        catch
        {

        }
    }

    private static SessionOptions GetDefaultSessionOptions()
    {
        var result = new SessionOptions();
        if (OperatingSystem.IsWindows())
        {
            TryIgnoreCatch(() => result.AppendExecutionProvider_CUDA());
        }
        else if (OperatingSystem.IsLinux())
        {
            TryIgnoreCatch(() => result.AppendExecutionProvider_CUDA());
        }
        else if (OperatingSystem.IsAndroid())
        {
            TryIgnoreCatch(() => result.AppendExecutionProvider_Nnapi());
        }
        else if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsIOS() || OperatingSystem.IsWatchOS() || OperatingSystem.IsTvOS() || OperatingSystem.IsMacOS())
        {
            TryIgnoreCatch(() => result.AppendExecutionProvider_CoreML());
        }
        return result;
    }

    public static Scanner Create(string modelPath)
    {
        var result = new InferenceSession(modelPath, GetDefaultSessionOptions());
        return new Scanner(result);
    }

    public static Scanner Create(byte[] model)
    {
        var result = new InferenceSession(model, GetDefaultSessionOptions());
        return new Scanner(result);
    }

    public List<ScanResult<T>> Scan<T, C>(Tensor<byte> images, bool is_bgra) where T : struct, Enum where C : IClassConverter<T>
    {
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("images", images),
            NamedOnnxValue.CreateFromTensor("is_bgra", new DenseTensor<bool>(new bool[] { is_bgra }.AsMemory(), []))
        };
        using var results = _session.Run(inputs);
        var outputTensor = results[0].AsTensor<float>();

        var num_detections = outputTensor.Dimensions[1];

        var result = new List<ScanResult<T>>(num_detections);
        for (var i = 0; i < num_detections; i++) {
            var x1 = (int)outputTensor[0, i, 0];
            var y1 = (int)outputTensor[0, i, 1];
            var x2 = (int)outputTensor[0, i, 2];
            var y2 = (int)outputTensor[0, i, 3];
            var confidence = (double)outputTensor[0, i, 4];
            var class_id = (int)outputTensor[0, i, 5];

            if (confidence < 0.4)
                continue;

            var klass = C.Convert(class_id);
            if (klass is null)
                continue;

            result.Add(new ScanResult<T>
            {
                Left = x1,
                Top = y1,
                Width = x2 - x1,
                Height = y2 - y1,
                Value = klass.Value,
                Confidence = confidence,
            });
        }
        return result;
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}
