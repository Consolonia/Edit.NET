using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    internal sealed class StorageFolder(string absolutePath) : IStorageFolder
    {
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

        public string Name => absolutePath.Split(['\\'], StringSplitOptions.RemoveEmptyEntries)[^1];
        public Uri Path => new(absolutePath);
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