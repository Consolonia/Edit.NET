using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public class RangerStorageProvider() : FileManagerStorageProviderBase("ranger")
    {
        protected override string[] GetFileOpenArguments(FilePickerOpenOptions options, string tempFilePath)
        {
            return GetArgumentsInternal(options, tempFilePath);
        }

        protected override string[] GetFileSaveArguments(FilePickerSaveOptions options, string tempFilePath)
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

            return [locationArgument, $"--choosefile={tempFilePath}"];
        }
    }
}