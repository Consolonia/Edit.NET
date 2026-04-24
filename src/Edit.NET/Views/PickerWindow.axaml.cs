using System;
using System.Threading.Tasks;
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

        private async void OnProcessExited(object? sender, ProcessExitedEventArgs e)
        {
            ExitCode = e.ExitCode;
            if (e.ExitCode != 0)
                await Task.Delay(1000);
            
            CloseModal();
        }

        public int ExitCode { get; set; }
    }
}