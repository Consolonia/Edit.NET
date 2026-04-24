using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public class YaziStorageProvider() : FileManagerStorageProviderBase("yazi")
    {
        protected override string[] GetFileOpenArguments(FilePickerOpenOptions options, string tempFilePath)
        {
            return GetArgumentsInternal(options, tempFilePath);
        }

        private static string[] GetArgumentsInternal(PickerOptions options, string tempFilePath)
        {
            string locationArgument = string.Empty;
            IStorageFolder? suggestedStartLocation = options.SuggestedStartLocation;

            if (suggestedStartLocation != null)
            {
                locationArgument = $"{suggestedStartLocation.Path.LocalPath}";
            }
            
            return ["--chooser-file", tempFilePath, locationArgument]; 
        }

        protected override string[] GetFileSaveArguments(FilePickerSaveOptions options, string tempFilePath)
        {
            return GetArgumentsInternal(options, tempFilePath);
        }
    }
}