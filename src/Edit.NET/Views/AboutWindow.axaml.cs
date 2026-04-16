using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Consolonia.Modal;

namespace EditNET.Views
{
    public partial class AboutWindow : ModalWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            SetVersion();
        }

        private void SetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            string ver = asm.GetName().Version!.ToString();
            VersionText.Text = $"Version {ver}";
        }

        private void Ok_OnClick(object? sender, RoutedEventArgs e)
        {
            this.CloseModal();
        }

        private void Control_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Focus();
        }
    }
}