using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public class StorageFile : IStorageFile
    {
        private readonly string _path;

        public StorageFile(string path)
        {
            _path = path;
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

        public string Name => System.IO.Path.GetFileName( Path.LocalPath);
        public Uri Path => new(_path);
        public bool CanBookmark => false;
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