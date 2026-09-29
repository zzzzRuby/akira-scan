using UIKit;

namespace AkiraScan.Platform.iOS;

internal static class Application
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
