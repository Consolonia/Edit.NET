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
    /*           
       8) check piping still working
     * 
     */
    
    public partial class MdiViewModel : ObservableObject
    {
        public Interaction<Unit, string?> OpenFileInteraction { get; } = new();
        public Interaction<Unit, Unit> ShutdownInteraction { get; } = new();
        
        public ObservableCollection<EditorViewModel> Documents { get; } = [];
        [ObservableProperty] private EditorViewModel _activeDocument;
        [ObservableProperty] private Settings _settings;

        public MdiViewModel()
        {
            Documents.CollectionChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(Documents));
                if (Documents.Count == 0)
                {
                    await ShutdownInteraction.Handle(Unit.Default).ToTask();
                }
            };
        }


        public MdiViewModel(Settings settings) : this()
        {
            _settings = settings;
        }
        
        public async Task NewCommand()
        {
            Documents.Add(new EditorViewModel(Settings));
            
            /*
            if (!await ActiveDocument.CheckSaved())
            {
                await ActiveDocument.FocusEditorInteraction.Handle(Unit.Default);
                return;
            }

            Document = new TextDocument();
            FilePath = null;

            await ActiveDocument.FocusEditorInteraction.Handle(Unit.Default);*/
        }

        public async Task OpenCommand()
        {
            /*if (!await CheckSaved())
            {
                await FocusEditorInteraction.Handle(Unit.Default);
                return;
            }*/

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
            {
                if (!await document.CheckSaved())
                {
                    await ActiveDocument.FocusEditorInteraction.Handle(Unit.Default);
                    return;
                }
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
                    editorViewModel.Document = new TextDocument(new StringTextSource(await File.ReadAllTextAsync(path)));
                });

            string? directoryName = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryName)) // path can be in the current directory
                Directory.SetCurrentDirectory(directoryName);
            
            await editorViewModel.FocusEditorInteraction.Handle(Unit.Default);
        }
    }
}