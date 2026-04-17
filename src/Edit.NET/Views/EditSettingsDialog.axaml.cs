using Avalonia;
using Avalonia.Interactivity;
using Consolonia;
using Consolonia.Modal;
using EditNET.DataModels;
using EditNET.ViewModels;
using JetBrains.Annotations;

namespace EditNET.Views
{
    public partial class EditSettingsDialog : ModalWindow
    {
        [UsedImplicitly]
        public EditSettingsDialog()
        {
            InitializeComponent();
            if (!((ConsoloniaLifetime)Application.Current!.ApplicationLifetime!).IsRgbColorMode())
                CompatibilityErrorTxt.IsVisible = true;
        }

        public EditSettingsDialog(Settings settings) : this()
        {
            DataContext = new EditSettingsViewModel(settings);
        }

        public Settings? Result { get; private set; }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            Result = ((EditSettingsViewModel)DataContext!).Settings;
            CloseModal();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = null;
            CloseModal();
        }
    }
}