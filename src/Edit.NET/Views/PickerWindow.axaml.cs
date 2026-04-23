using System;
using Consolonia.Modal;
using Iciclecreek.Terminal;

namespace EditNET.Views
{
    public partial class PickerWindow : ModalWindow
    {
        public string AppToRun { get; init; }
        public string[] AppArgs { get; init; }
        public string AppStartLocation { get; init; }

        public PickerWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, EventArgs e)
        {
            await Terminal.LaunchProcess(AppStartLocation, AppToRun, AppArgs);
        }

        private void OnProcessExited(object? sender, ProcessExitedEventArgs e)
        {
            ExitCode = e.ExitCode;

            CloseModal();
        }

        public int ExitCode { get; set; }
    }
}