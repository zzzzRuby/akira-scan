using System;

namespace AkiraScan.Camera.Platform.Tizen;

internal interface ITizenCameraOutputHolder: ICameraOutputHolder
{
    void SetupCamera(global::Tizen.Multimedia.Camera camera);
    void UnsetupCamera(global::Tizen.Multimedia.Camera camera);

    event Action? AfterFrameUpdate;
}
