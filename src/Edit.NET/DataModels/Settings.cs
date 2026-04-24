using System.ComponentModel.DataAnnotations;
using TextMateSharp.Grammars;

namespace EditNET.DataModels
{
    public class Settings
    {
        public ConsoloniaTheme ConsoloniaTheme { get; set; } = ConsoloniaTheme.Modern;

        public bool LightVariant { get; set; }

        public bool ShowTabs { get; set; }

        public bool ShowSpaces { get; set; }

        [RegularExpression(@"^(?i)\.[a-z0-9]+$", ErrorMessage = "Invalid extension")]
        public string DefaultExtension { get; set; } = ".txt";

        public ThemeName SyntaxTheme { get; set; } = ThemeName.VisualStudioDark;

        public string FilePicker { get; set; } = FilePickerBuiltIn;

        public const string FilePickerBuiltIn = "Built-in";
        public const string FilePickerYazi = "yazi";
        public const string FilePickerRanger = "ranger";
        public const string FilePickerFar = "far";
        public const string FilePickerFar2l = "far2l";
    }
}