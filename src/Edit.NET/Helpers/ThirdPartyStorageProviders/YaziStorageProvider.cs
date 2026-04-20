using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using Consolonia;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public class YaziStorageProvider : IStorageProvider
    {
        public async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
        {
            var cts = new CancellationTokenSource();

            await ((ConsoloniaLifetime)Application.Current!.ApplicationLifetime!).DisconnectFromConsoleAsync(cts.Token);

            string tempFilePath = Path.GetTempFileName();

            string location = string.Empty;
            IStorageFolder? suggestedStartLocation = options.SuggestedStartLocation;

            if (suggestedStartLocation != null)
            {
                location = $" \"{suggestedStartLocation.Path}\"";
            }

            string arguments = $"{location} --chooser-file \"{tempFilePath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "yazi",
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = location
            };

            int exitCode = -1;
            await Task.Run(() =>
            {
                using Process process = Process.Start(processStartInfo)!;
                //WaitForExitAsync probably can be used, but seems we can not continue on initial thread
                process.WaitForExit();
                cts.Cancel();
                exitCode = process.ExitCode;
            }, cts.Token);
            if (exitCode != 0)
                return [];

            return new[] { new StorageFile(File.ReadAllText(tempFilePath)) };
        }

        public Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
        {
            throw new NotImplementedException();
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
            throw new NotSupportedException();
        }

        public Task<IStorageFolder?> TryGetWellKnownFolderAsync(WellKnownFolder wellKnownFolder)
        {
            throw new NotSupportedException();
        }

        public bool CanOpen => throw new NotSupportedException();
        public bool CanSave => throw new NotSupportedException();
        public bool CanPickFolder => throw new NotSupportedException();
    }
    
    public class StorageFile(string path) : IStorageFile
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<StorageItemProperties> GetBasicPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string?> SaveBookmarkAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFolder?> GetParentAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }
        public Uri Path { get; }
        public bool CanBookmark { get; }
        public Task<Stream> OpenReadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenWriteAsync()
        {
            throw new NotImplementedException();
        }
    }
}