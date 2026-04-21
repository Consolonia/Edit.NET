// DUPFINDER_ignore

using System.Threading;
using Avalonia;
using Avalonia.Controls.Platform;
using Avalonia.Platform.Storage;
using Consolonia;
using Consolonia.ManagedWindows.Storage;
using EditNET.Helpers;
using EditNET.Helpers.ThirdPartyStorageProviders;

namespace EditNET
{
    public static partial class Program
    {
        private static Thread? _mainThread;

        private static void Main(string[] args)
        {
            _mainThread = Thread.CurrentThread;
            BuildAvaloniaApp()
                .StartWithConsoleLifetime(args);
        }

        private static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UseConsolonia()
                .UseAutoDetectedConsole()
                .WithDeveloperTools()
                //.UseConsoloniaStorage()
                .With<IStorageProviderFactory>(new PreferencesStorageProviderFactory())
                .LogToException();
        }
    }
}