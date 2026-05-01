using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Consolonia;
using Consolonia.Modal;
using EditNET.DataModels;
using EditNET.ViewModels;
using JetBrains.Annotations;

namespace EditNET.Views
{
    public partial class EditSettingsDialog : ModalWindow
    {
        public ICommand OnOkCommand { get; private set; }
        
        [UsedImplicitly]
        public EditSettingsDialog()
        {
            InitializeComponent();
            OnOkCommand = new RelayCommand(() => { OnOk(null!, null!); });
            if (!((ConsoloniaLifetime)Application.Current!.ApplicationLifetime!).IsRgbColorMode())
                CompatibilityErrorTxt.IsVisible = true;
        }

        public EditSettingsDialog(Settings settings) : this()
        {
            DataContext = new EditSettingsViewModel(settings);
        }

        public Settings? Result { get; private set; }

        public void OnOk(object sender, RoutedEventArgs e)
        {
            Result = ((EditSettingsViewModel)DataContext!).Settings;
            CloseModal();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = null;
            CloseModal();
        }

        private void Control_OnLoaded(object? sender, RoutedEventArgs e)
        {
            ((Control)sender!).Focus();
        }
    }
}