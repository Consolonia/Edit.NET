using System;
using System.IO;
using System.Reactive;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Consolonia;
using Consolonia.Themes;
using EditNET.ViewModels;
using EditNET.Views;
using ReactiveUI;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace EditNET
{
    public partial class App : Application
    {
        private WindowNotificationManager? _notificationManager;

        private MainWindow MainWindow =>
            (MainWindow)((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow!;

        private AppViewModel ViewModel => (AppViewModel)DataContext!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            DataContext = new AppViewModel();
            ViewModel.SetThemeInteraction.RegisterHandler(SetThemeHandler);
            ViewModel.ShowNotificationInteraction.RegisterHandler(ShowNotificationHandler);
        }

        private void ShowNotificationHandler(IInteractionContext<Notification, Unit> context)
        {
            _notificationManager!.Show(context.Input);
            context.SetOutput(Unit.Default);
        }

        private void SetThemeHandler(IInteractionContext<(ConsoloniaTheme, bool), Unit> context)
        {
            context.SetOutput(Unit.Default);
            MainWindow.RequestedThemeVariant = context.Input.Item2 ? ThemeVariant.Light : ThemeVariant.Dark;
            if (!LoadUiTheme(context.Input.Item1))
            {
                ShowThemeIncompatible();
                return;
            }

            MainWindow.Content = new MdiView { DataContext = ViewModel.MdiViewModel };
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var desktopLifetime = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
            bool themeLoaded = LoadUiTheme(ViewModel.MdiViewModel.Settings.ConsoloniaTheme);
            desktopLifetime.MainWindow = new MainWindow
            {
                DataContext = ViewModel,
                RequestedThemeVariant = ViewModel.MdiViewModel.Settings.LightVariant
                    ? ThemeVariant.Light
                    : ThemeVariant.Dark
            };

            _notificationManager = new WindowNotificationManager(desktopLifetime.MainWindow!);

            base.OnFrameworkInitializationCompleted();

            HandleDispatcherExceptions();

            if (ViewModel.InitialLoadSettingsException != null)
                ShowNotification("Settings Error",
                    "Failed to load settings: " + ViewModel.InitialLoadSettingsException.Message,
                    NotificationType.Error);

            if (!themeLoaded) ShowThemeIncompatible();

            Dispatcher.UIThread.Post(async () =>
            {
                if (desktopLifetime.Args is { Length: > 0 })
                    await ViewModel.MdiViewModel.OpenFile(desktopLifetime.Args[0]);
                else await ViewModel.MdiViewModel.NewCommand();
                
                if (PipingWorkaround.PipedInputContent != null)
                    ViewModel.MdiViewModel.ActiveDocument.AppendContent(PipingWorkaround.PipedInputContent);
            }, DispatcherPriority.ContextIdle);
        }

        private void ShowThemeIncompatible()
        {
            ShowNotification("Theme Error",
                $"Modern themes are not supported in this environment. Switching back to {nameof(ConsoloniaTheme.TurboVisionCompatible)} and {Enum.GetName(EditorView.DefaultEditorTheme)}",
                NotificationType.Error);
        }

        private void ShowNotification(string title, string message, NotificationType notificationType)
        {
            _notificationManager!.Show(new Notification { Type = notificationType, Message = message, Title = title });
        }

        private bool LoadUiTheme(ConsoloniaTheme theme)
        {
            if (!((ConsoloniaLifetime)ApplicationLifetime!).IsRgbColorMode() &&
                theme is ConsoloniaTheme.Modern or ConsoloniaTheme.ModernContrast)
            {
                if (Styles[0] is ModernTheme or ModernContrastTheme)
                {
                    Styles[0] = new TurboVisionCompatibleTheme();
                }
                return false;
            }

            Styles[0] = theme switch
            {
                ConsoloniaTheme.Modern => new ModernTheme(),
                ConsoloniaTheme.ModernContrast => new ModernContrastTheme(),
                ConsoloniaTheme.TurboVision => new TurboVisionTheme(),
                ConsoloniaTheme.TurboVisionCompatible => new TurboVisionCompatibleTheme(),
                ConsoloniaTheme.TurboVisionGray => new TurboVisionGrayTheme(),
                ConsoloniaTheme.TurboVisionElegant => new TurboVisionElegantTheme(),
                _ => throw new InvalidDataException("Unknown theme name")
            };

            return true;
        }
    }
}