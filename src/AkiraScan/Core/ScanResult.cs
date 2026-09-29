namespace AkiraScan.Core;

public sealed class ScanResult<T>
{
    public required int Left { get; set; }
    public required int Top { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required T Value { get; set; }
    public required double Confidence { get; set; }
}
