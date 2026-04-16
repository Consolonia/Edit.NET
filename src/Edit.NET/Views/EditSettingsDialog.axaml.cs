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
        public Settings? Result { get; private set; }

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

        private void OnOk(object sender, RoutedEventArgs e)
        {
            Result = ((EditSettingsViewModel)DataContext!).Settings;
            this.CloseModal();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = null;
            this.CloseModal();
        }
    }
}