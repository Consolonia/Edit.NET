using System.IO;
using Avalonia.Platform.Storage;

namespace EditNET.Helpers.ThirdPartyStorageProviders
{
    public abstract class FarStorageProviderBase(string executableName) : FileManagerStorageProviderBase(executableName)
    {
        private const string LuaScript = """
                                         local out = ...
                                         Macro {
                                           area="Shell";
                                           key="F5";
                                           description="Pick file and exit";
                                           action=function()
                                             local item = panel.GetCurrentPanelItem(nil, 1)
                                             if not item then return end

                                             local dir = panel.GetPanelDirectory(nil, 1).Name
                                             local sep = dir:sub(-1) == "/" and "" or "/"
                                             local path = dir .. sep .. item.FileName

                                             local f = io.open(out, "w")
                                             f:write(path)
                                             f:close()

                                             Keys("Esc")
                                             exit()
                                           end;
                                         }
                                         """;

        protected static string[] GetArgumentsInternal(PickerOptions options, string tempFilePath)
        {
            string luaScriptPath = Path.GetTempFileName();
            File.WriteAllText(luaScriptPath, LuaScript);

            string startLocation = string.Empty;
            IStorageFolder? suggestedStartLocation = options.SuggestedStartLocation;
            if (suggestedStartLocation != null)
                startLocation = suggestedStartLocation.Path.LocalPath;

            if (string.IsNullOrEmpty(startLocation))
                return ["--tty", $"lua:@{luaScriptPath} {tempFilePath}"];

            return ["--tty", "-cd", startLocation, $"lua:@{luaScriptPath} {tempFilePath}"];
        }

        protected override string[] GetFileOpenArguments(FilePickerOpenOptions options, string tempFilePath)
        {
            return GetArgumentsInternal(options, tempFilePath);
        }

        protected override string[] GetFileSaveArguments(FilePickerSaveOptions options, string tempFilePath)
        {
            return GetArgumentsInternal(options, tempFilePath);
        }
    }
}