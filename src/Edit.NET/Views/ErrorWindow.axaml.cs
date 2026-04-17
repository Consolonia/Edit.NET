using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Consolonia.Modal;

namespace EditNET.Views
{
    public partial class ErrorWindow : ModalWindow
    {
        private readonly Exception? _exception;
        private readonly DispatcherTimer _timer;
        private int _secondsRemaining = Program.ShutdownTimeoutSeconds;

        public ErrorWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerOnTick;
        }

        public ErrorWindow(Exception exception) : this()
        {
            _exception = exception;
            ExceptionTextBox.Text = exception.ToString();
            UpdateTimerText();
            _timer.Start();
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            _secondsRemaining--;
            if (_secondsRemaining < 0)
            {
                _timer.Stop();
                return;
            }

            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            TimerSpan.Text = _secondsRemaining.ToString();
        }

        private async void OnReportAndClose(object? sender, RoutedEventArgs e)
        {
            _timer.Stop();

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
                await topLevel.Clipboard.SetTextAsync(ExceptionTextBox.Text);

            if (topLevel?.Launcher != null)
                await topLevel.Launcher.LaunchUriAsync(new Uri("https://github.com/Consolonia/Edit.NET/issues/new"));

            TerminateAndPrintButton.Focus();
        }

        private void OnCloseAndPrint(object? sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Console.WriteLine(ExceptionTextBox.Text);

            if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledLifetime)
                controlledLifetime.Shutdown(1);
            else
                Environment.FailFast(ExceptionTextBox.Text, _exception);
        }

        private void OnClose(object? sender, RoutedEventArgs e)
        {
            _timer.Stop();
            CloseModal();
        }

        private void DefaultButton_OnLoaded(object? sender, RoutedEventArgs e)
        {
            ((Control)sender!).Focus();
        }
    }
}