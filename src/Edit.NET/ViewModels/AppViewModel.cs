using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommandLine;
using CommunityToolkit.Mvvm.ComponentModel;
using EditNET.DataModels;
using ReactiveUI;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace EditNET.ViewModels
{
    public class AppViewModel : ObservableObject
    {
        public const string EditNetTitle = "Edit.NET";

        private static readonly string SettingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                EditNetTitle,
                "settings.json");

        private ConsoloniaTheme _consoloniaTheme;
        private bool _consoloniaThemeLight;

        public AppViewModel()
        {
            Settings? settings = null;
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    settings = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                InitialLoadSettingsException = exception;
            }

            settings ??= new Settings();
            _consoloniaTheme = settings.ConsoloniaTheme;
            _consoloniaThemeLight = settings.LightVariant;
            MdiViewModel = new MdiViewModel(settings);
            MdiViewModel.WhenAnyValue(model => model.Settings).Skip(1).Subscribe(OnSettingsUpdated);
        }

        public MdiViewModel MdiViewModel { get; }
        public Interaction<(ConsoloniaTheme, bool), Unit> SetThemeInteraction { get; } = new();
        public Interaction<Notification, Unit> ShowNotificationInteraction { get; } = new();
        public Exception? InitialLoadSettingsException { get; }

        private void OnSettingsUpdated(Settings settings)
        {
            SaveSettings(settings);
            if (_consoloniaTheme != settings.ConsoloniaTheme || _consoloniaThemeLight != settings.LightVariant)
            {
                SetThemeInteraction.Handle((settings.ConsoloniaTheme, settings.LightVariant)).Wait();
                _consoloniaTheme = settings.ConsoloniaTheme;
                _consoloniaThemeLight = settings.LightVariant;
            }
        }

        private async void SaveSettings(Settings settings)
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsFilePath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                await ShowNotificationInteraction.Handle(new Notification("Settings Error",
                    "Failed to save settings file: " + exception.Message, NotificationType.Error));
            }
        }

        public async Task ParseArguments(string[] desktopLifetimeArgs)
        {
            ParserResult<StartupArguments> parserResult =
                Parser.Default.ParseArguments<StartupArguments>(desktopLifetimeArgs);

            switch (parserResult.Tag)
            {
                case ParserResultType.Parsed:
                    MdiViewModel.Debug = parserResult.Value.Debug;
                    if (!string.IsNullOrEmpty(parserResult.Value.File))
                        await MdiViewModel.OpenFile(parserResult.Value.File);
                    else
                        MdiViewModel.NewCommand();
                    break;
                case ParserResultType.NotParsed:
                    MdiViewModel.NewCommand();
                    string errorMessage = parserResult.Errors.Aggregate("Invalid arguments provided:",
                        (current, error) => current + (Environment.NewLine + error));
                    await MdiViewModel.ActiveDocument.MessageBoxInteraction.Handle(
                        new MessageBoxModel("Error", errorMessage, MessageBoxButtons.Ok));
                    await Console.Error.WriteLineAsync(errorMessage);
                    break;
            }

            if (PipingWorkaround.PipedInputContent != null)
                MdiViewModel.ActiveDocument.AppendContent(PipingWorkaround.PipedInputContent);
        }
    }
}