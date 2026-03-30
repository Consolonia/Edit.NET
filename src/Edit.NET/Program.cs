// DUPFINDER_ignore

using System.Threading;
using Avalonia;
using Consolonia;
using Consolonia.ManagedWindows.Storage;

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
                .UseConsoloniaStorage()
                .LogToException();
        }
    }
}