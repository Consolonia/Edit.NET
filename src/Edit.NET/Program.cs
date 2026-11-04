// DUPFINDER_ignore

using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls.Platform;
using Consolonia;
using CommandLine;
using CommandLine.Text;
using EditNET.DataModels;
using EditNET.Helpers.ThirdPartyStorageProviders;

namespace EditNET
{
    public static partial class Program
    {
        private static void Main(string[] args)
        {
            using var parser = new Parser(settings => settings.HelpWriter = null);
            var parserResult = parser.ParseArguments<StartupArguments>(args);
            if (parserResult.Tag == ParserResultType.NotParsed &&
                parserResult.Errors.Count() == 1 &&
                parserResult.Errors.Single().Tag is ErrorType.HelpRequestedError or ErrorType.VersionRequestedError)
            {
                Console.WriteLine(HelpText.AutoBuild(parserResult));
                return;
            }
            
            App.HandleProcessExceptions(Thread.CurrentThread);

            PipingWorkaround.ReadPipedInputAndReattachStdInFromTerminalIfRedirected();

            BuildAvaloniaApp(parserResult)
                .StartWithConsoleLifetime(args);
        }

        private static AppBuilder BuildAvaloniaApp(ParserResult<StartupArguments> parserResult)
        {
            return AppBuilder.Configure(() => new App { ParsedArguments = parserResult })
                .UseConsolonia()
                .UseAutoDetectedConsole()
                .WithDeveloperTools()
                //.UseConsoloniaStorage()
                .With<IStorageProviderFactory>(new PreferencesStorageProviderFactory())
                .LogToException();
        }
    }
}
