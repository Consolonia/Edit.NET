using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    internal sealed class StorageFile(string path) : IStorageFile
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

        public string Name => System.IO.Path.GetFileName( Path.LocalPath);
        public Uri Path => new(path);
        public bool CanBookmark => false;
        public Task<Stream> OpenReadAsync()
        {
            throw new NotSupportedException();
        }

        public Task<Stream> OpenWriteAsync()
        {
            throw new NotSupportedException();
        }
    }
}