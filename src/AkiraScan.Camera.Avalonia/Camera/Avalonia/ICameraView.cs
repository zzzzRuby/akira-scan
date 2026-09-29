using Avalonia.Rendering.Composition;
using System;

namespace AkiraScan.Camera.Avalonia;

internal interface ICameraView: IDisposable
{
    void OnMeasure(double width, double height);
    void OnAfterFrameUpdate();

    CompositionContainerVisual Visual { get; }
}
