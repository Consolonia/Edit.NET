using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Consolonia;
using Consolonia.Core.Infrastructure;
using EditNET.Views;
using Iciclecreek.Terminal;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public abstract class FileManagerStorageProviderBase(string executableName) : IStorageProvider
    {
        protected abstract string[] GetFileOpenArguments(FilePickerOpenOptions options, string tempFilePath);
        protected abstract string[] GetFileSaveArguments(FilePickerSaveOptions options, string tempFilePath);

        private async Task<string?> RunFileManagerAsync(Func<string, string[]> argumentsFactory, string? workingDirectory)
        {
            
            var applicationLifetime = (ConsoloniaLifetime)Application.Current!.ApplicationLifetime!;

            string tempFilePath = Path.GetTempFileName();

            try
            {
                var pickerWindow = new PickerWindow
                {
                    AppStartLocation = workingDirectory ?? string.Empty,
                    AppToRun = executableName,
                    AppArgs = argumentsFactory(tempFilePath)
                };
                
                //todo: 8D932615-A858-4063-835C-CDFCD5FFB799 Add test to check that this workaround is still necessary
                ((MainWindow)applicationLifetime.MainWindow!).FindDescendantOfType<TextEditor>()!.TextArea.Focus();
                
                await pickerWindow.ShowModalAsync(applicationLifetime.MainWindow);
                int exitCode = pickerWindow.ExitCode;
                return exitCode != 0 ? null : File.ReadAllText(tempFilePath);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        public async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
        {
            string? location = options.SuggestedStartLocation?.Path.LocalPath;

            string? selectedPath = await RunFileManagerAsync(tempFilePath => GetFileOpenArguments(options, tempFilePath), location);
            if (selectedPath == null)
                return [];

            return [new StorageFile(selectedPath)];
        }

        public async Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
        {
            string? location = options.SuggestedStartLocation?.Path.LocalPath;

            string? selectedPath = await RunFileManagerAsync(tempFilePath => GetFileSaveArguments(options, tempFilePath), location);
            if (selectedPath == null)
                return null;

            return new StorageFile(selectedPath);
        }

        public Task<SaveFilePickerResult> SaveFilePickerWithResultAsync(FilePickerSaveOptions options)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageBookmarkFile?> OpenFileBookmarkAsync(string bookmark)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageBookmarkFolder?> OpenFolderBookmarkAsync(string bookmark)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFile?> TryGetFileFromPathAsync(Uri filePath)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFolder?> TryGetFolderFromPathAsync(Uri folderPath)
        {
            string absolutePath = folderPath.LocalPath;
            return Task.FromResult<IStorageFolder?>(new StorageFolder(Path.GetDirectoryName(absolutePath)!));
        }

        public Task<IStorageFolder?> TryGetWellKnownFolderAsync(WellKnownFolder wellKnownFolder)
        {
            throw new NotSupportedException();
        }

        public bool CanOpen => throw new NotSupportedException();
        public bool CanSave => throw new NotSupportedException();
        public bool CanPickFolder => throw new NotSupportedException();
    }
}