using System.Collections.Generic;
using System.IO;
using Avalonia.Data.Converters;
using DynamicData.Binding;
using EditNET.ViewModels;

namespace EditNET
{
    public static class EditConverters
    {
        public static readonly IValueConverter ModifiedConverter =
            new FuncValueConverter<bool, string>(modified => modified ? "Modified" : "Saved");

        public static readonly IValueConverter FilePathToNameConverter =
            new FuncValueConverter<string, string>(filePath =>
                string.IsNullOrEmpty(filePath) ? "Untitled" : Path.GetFileName(filePath));

        public static readonly IValueConverter DocumentsObservableCollectionToInfoConverter =
            new FuncValueConverter<ICollection<EditorViewModel>, string>(models => $"{models.Count} document(s)");
    }
}