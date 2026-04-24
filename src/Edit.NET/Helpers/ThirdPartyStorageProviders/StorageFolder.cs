using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public class StorageFolder : IStorageFolder
    {
        private readonly string _absolutePath;

        public StorageFolder(string absolutePath)
        {
            _absolutePath = absolutePath;
        }

        public void Dispose()
        {
            throw new NotSupportedException();
        }

        public Task<StorageItemProperties> GetBasicPropertiesAsync()
        {
            throw new NotSupportedException();
        }

        public Task<string?> SaveBookmarkAsync()
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFolder?> GetParentAsync()
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync()
        {
            throw new NotSupportedException();
        }

        public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
        {
            throw new NotSupportedException();
        }

        public string Name => _absolutePath.Split(['\\'], StringSplitOptions.RemoveEmptyEntries)[^1];
        public Uri Path => new(_absolutePath);
        public bool CanBookmark => false;

        public IAsyncEnumerable<IStorageItem> GetItemsAsync()
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFolder?> GetFolderAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFile?> GetFileAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFile?> CreateFileAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task<IStorageFolder?> CreateFolderAsync(string name)
        {
            throw new NotSupportedException();
        }
    }
}