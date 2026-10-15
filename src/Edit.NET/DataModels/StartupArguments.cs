using CommandLine;
using JetBrains.Annotations;

namespace EditNET.DataModels
{
    [UsedImplicitly]
    public class StartupArguments
    {
        [Value(0, MetaName = "file", Required = false, HelpText = "The path to the file to open")]
        public string? File { get; set; }

        [Option('d', "debug", Required = false, HelpText = "Whether to run in debug mode")]
        public bool Debug { get; set; }
    }
}