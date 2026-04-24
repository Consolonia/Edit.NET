using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EditNET.DataModels;
using JetBrains.Annotations;
using TextMateSharp.Grammars;

namespace EditNET.ViewModels
{
    public class EditSettingsViewModel(Settings settings) : ObservableValidator
    {
        [UsedImplicitly(Reason = "Used by designer")]
        public EditSettingsViewModel() : this(new Settings())
        {
        }

        public Settings Settings { get; } = settings;

        public IReadOnlyCollection<ConsoloniaTheme> AvailableThemes { get; } = Enum.GetValues<ConsoloniaTheme>();

        public IReadOnlyCollection<ThemeName> SyntaxThemes { get; } = Enum.GetValues<ThemeName>();

        public IReadOnlyCollection<string> AvailableFilePickers { get; } =
        [
            Settings.FilePickerBuiltIn, 
            Settings.FilePickerYazi, 
            Settings.FilePickerRanger,
            Settings.FilePickerFar,
            Settings.FilePickerFar2l
        ];
    }
}