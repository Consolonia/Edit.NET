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

        public string Name => _absolutePath.Split(['\\'], StringSplitOptions.RemoveEmptyEntries)[^1];
        public Uri Path => new(_absolutePath);
        public bool CanBookmark => false;

        public IAsyncEnumerable<IStorageItem> GetItemsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFolder?> GetFolderAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFile?> GetFileAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFile?> CreateFileAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IStorageFolder?> CreateFolderAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}