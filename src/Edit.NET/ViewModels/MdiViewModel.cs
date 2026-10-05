using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using EditNET.DataModels;
using ReactiveUI;

namespace EditNET.ViewModels
{
    public partial class MdiViewModel : ObservableObject
    {
        [ObservableProperty] private EditorViewModel _activeDocument = null!;
        [ObservableProperty] private Settings _settings = null!;

        [Obsolete("For designer only")]
        public MdiViewModel()
        {
            Documents.CollectionChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(Documents));
                if (Documents.Count == 0) await ShutdownInteraction.Handle(Unit.Default).ToTask();
            };
        }


#pragma warning disable CS0618 // Type or member is obsolete
        public MdiViewModel(Settings settings) : this()
#pragma warning restore CS0618 // Type or member is obsolete
        {
            _settings = settings;
        }

        public Interaction<Unit, string?> OpenFileInteraction { get; } = new();
        public Interaction<Unit, Unit> ShutdownInteraction { get; } = new();

        public ObservableCollection<EditorViewModel> Documents { get; } = [];

        public async Task NewCommand()
        {
            Documents.Add(new EditorViewModel(Settings));
        }

        public async Task OpenCommand()
        {
            string? filePath = await OpenFileInteraction.Handle(Unit.Default);
            if (filePath == null)
            {
                await ActiveDocument.FocusEditorInteraction.Handle(Unit.Default);
                return;
            }

            await OpenFile(Path.GetFullPath(filePath));
        }

        public async Task SaveCommand()
        {
            await ActiveDocument.SaveCommand();
        }

        public async Task SaveAsCommand()
        {
            await ActiveDocument.SaveAsCommand();
        }

        public async Task ExitCommand()
        {
            foreach (EditorViewModel document in Documents)
                if (!await document.CheckSaved())
                {
                    await ActiveDocument.FocusEditorInteraction.Handle(Unit.Default);
                    return;
                }

            await ShutdownInteraction.Handle(Unit.Default);
        }

        public async Task OpenFile(string path)
        {
            if (!Path.IsPathFullyQualified(path))
                path = Path.GetFullPath(path, Environment.CurrentDirectory);
            var editorViewModel = new EditorViewModel(Settings)
            {
                FilePath = path
            };

            Documents.Add(editorViewModel);
            if (File.Exists(editorViewModel.FilePath))
                await editorViewModel.HandleFileExceptions(async () =>
                {
                    editorViewModel.Document =
                        new TextDocument(new StringTextSource(await File.ReadAllTextAsync(path)));
                });

            string? directoryName = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryName)) // path can be in the current directory
                Directory.SetCurrentDirectory(directoryName);

            await editorViewModel.FocusEditorInteraction.Handle(Unit.Default);
        }
    }
}