using System;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform.Storage;
using Consolonia.ManagedWindows.Storage;
using EditNET.DataModels;
using EditNET.Helpers.ThirdPartyStorageProviders;
using EditNET.ViewModels;
using EditNET.Views;

namespace EditNET.Helpers
{
    internal class PreferencesStorageProviderFactory : ConsoloniaStorageProviderFactory, IStorageProviderFactory
    {
        IStorageProvider IStorageProviderFactory.CreateProvider(TopLevel topLevel)
        {
            var mainWindow = (MainWindow)topLevel;
            var appViewModel = (AppViewModel)mainWindow.DataContext!;
            EditorViewModel editorViewModel = appViewModel.EditorViewModel;

            return editorViewModel.Settings.FilePicker switch
            {
                Settings.FilePickerBuiltIn => base.CreateProvider(topLevel),
                Settings.FilePickerYazi => new YaziStorageProvider(),
                Settings.FilePickerRanger => new RangerStorageProvider(),
                _ => throw new NotSupportedException("Unsupported file picker: " + editorViewModel.Settings.FilePicker +
                                                     ". Supported values: BuiltIn, Yazi, Ranger")
            };
        }
    }
}