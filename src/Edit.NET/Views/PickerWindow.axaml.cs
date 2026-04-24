using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consolonia.Modal;
using Iciclecreek.Terminal;

namespace EditNET.Views
{
    public partial class PickerWindow : ModalWindow
    {
        public required string AppToRun { get; init; }
        public required IReadOnlyCollection<string> AppArgs { get; init; }
        public required string AppStartLocation { get; init; }

        
        public PickerWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, EventArgs e)
        {
            await Terminal.LaunchProcess(AppStartLocation, AppToRun, AppArgs.ToArray());
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