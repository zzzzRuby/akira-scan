using Avalonia;
using System;

namespace AkiraScan.Platform.Desktop;

public static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
#if MACOS
            .UseAvaloniaNative()
            .UseHarfBuzz()
            .UseSkia()
#elif WINDOWS
            .UseWin32()
            .UseHarfBuzz()
            .UseSkia()
#else
            .UsePlatformDetect()
#endif
            .WithInterFont()
            .LogToTrace();
}