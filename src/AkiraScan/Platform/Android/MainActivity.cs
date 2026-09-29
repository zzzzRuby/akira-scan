using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia.Android;

namespace AkiraScan.Platform.Android;

[Activity(
    Label = "AkiraScan",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
internal class MainActivity : AvaloniaMainActivity
{
    private const int CameraPermissionCode = 101;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        CheckCameraPermission();
    }

    private void CheckCameraPermission()
    {
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int)Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, [Manifest.Permission.Camera], CameraPermissionCode);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == CameraPermissionCode)
        {
            if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                // Permission granted! You can now access the camera.
            }
            else
            {
                // Permission denied. Handle appropriately (e.g., show a message).
            }
        }
    }
}
