using Android.Views;
using System;

namespace AkiraScan.Camera.Platform.Android;

internal interface IAndroidCameraOutputHolder: ICameraOutputHolder
{
    event Action? AfterFrameUpdate;

    Surface Surface { get; }
}
