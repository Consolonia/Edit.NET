using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public class RangerStorageProvider : IStorageProvider
    {
        public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<SaveFilePickerResult> SaveFilePickerWithResultAsync(FilePickerSaveOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageBookmarkFile?> OpenFileBookmarkAsync(string bookmark)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageBookmarkFolder?> OpenFolderBookmarkAsync(string bookmark)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFile?> TryGetFileFromPathAsync(Uri filePath)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFolder?> TryGetFolderFromPathAsync(Uri folderPath)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFolder?> TryGetWellKnownFolderAsync(WellKnownFolder wellKnownFolder)
        {
            throw new NotImplementedException();
        }

        public bool CanOpen { get; }
        public bool CanSave { get; }
        public bool CanPickFolder { get; }
    }
}