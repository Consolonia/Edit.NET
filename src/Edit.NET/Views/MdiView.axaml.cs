using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Consolonia.Themes.Infrastructure;
using DynamicData.Binding;
using EditNET.DataModels;
using EditNET.Helpers;
using EditNET.ViewModels;
using Iciclecreek.Avalonia.WindowManager;
using Iciclecreek.Terminal;
using ReactiveUI;

namespace EditNET.Views
{
    public partial class MdiView : UserControl
    {
        private CompositeDisposable? _dataContextHandlers;

        public MdiView()
        {
            InitializeComponent();

            //todo: 8D932615-A858-4063-835C-CDFCD5FFB799 check that it's still necessary in tests once tests added
            StyledProperty<IList<string>>
                unused = TerminalControl
                    .ArgsProperty; //initializaing the control, it changes the style and they become unavailable due some Avalonia issue
        }

        private MainWindow MainWindow => this.FindAncestorOfType<MainWindow>()!;

        protected override void OnDataContextChanged(EventArgs e)
        {
            _dataContextHandlers?.Dispose();

            base.OnDataContextChanged(e);

            _dataContextHandlers = [];

            ObservableCollection<EditorViewModel> documents = ViewModel.Documents;
            documents.CollectionChanged += DocumentsOnCollectionChanged;
            _dataContextHandlers.Add(
                Disposable.Create(() => documents.CollectionChanged -= DocumentsOnCollectionChanged));

            ViewModel.OpenFileInteraction.RegisterHandler(OpenFileHandler).DisposeWith(_dataContextHandlers);
            ViewModel.ShutdownInteraction.RegisterHandler(ShutDownHandler).DisposeWith(_dataContextHandlers);

            return;

            void DocumentsOnCollectionChanged(object? sender,
                NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        EditorViewModel newDocument =
                            notifyCollectionChangedEventArgs.NewItems!.Cast<EditorViewModel>().Single();
                        AddDocument(newDocument);
                        break;
                    case NotifyCollectionChangedAction.Remove:

                        EditorViewModel editorViewModelToClose = notifyCollectionChangedEventArgs.OldItems!
                            .Cast<EditorViewModel>()
                            .Single();

                        var windowToClose = (ManagedWindow)WindowHost.Windows.Single(control =>
                            control.DataContext == editorViewModelToClose);
                        windowToClose.Close();


                        ViewMenuItem.Items.Remove(ViewMenuItem.Items.OfType<MenuItem>()
                            .Single(menuItem => menuItem.DataContext == editorViewModelToClose));
                        break;
                    default:
                        throw new NotSupportedException();
                }

                //BindingOperations.GetBindingExpressionBase(subMenu, IsEnabledProperty)?.UpdateTarget();
            }
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            foreach (EditorViewModel editorViewModel in ViewModel.Documents)
            {
                AddDocument(editorViewModel);
            }
        }

        private void AddDocument(EditorViewModel newDocument)
        {
            // if current maximized - also opening maximized
            bool maximize = ActiveWindow?.WindowState is WindowState.Maximized or null;

            var editorView = new EditorView
            {
                DataContext = newDocument,
                AnimateWindow = true,
                WindowState = WindowState.Normal,
                // together with normal setup correct size to return from maximized
                Width = Bounds.Width * 2 / 3 - ViewModel.Documents.Count,
                Height = Bounds.Height * 2 / 3 - ViewModel.Documents.Count
            };

            
            if ((string?)this.FindResource(AutoThemeStylesBase.ConsoloniaThemeFamilyKey) == AutoThemeStylesBase.TurboVisionThemeKey)
            {
                editorView.Padding = new Thickness(0, 0, -1, -1);
            }

            editorView.Activated += async (activeWindow, _) =>
            {
                var editorViewModel = (EditorViewModel)((EditorView)activeWindow!).DataContext!;
                ViewModel.ActiveDocument = editorViewModel;

                foreach (MenuItem menuItem in ViewMenuItem.Items.OfType<MenuItem>().ToArray())
                {
                    menuItem.IsChecked = menuItem.DataContext == editorViewModel;
                }

                await editorViewModel.FocusEditorInteraction.Handle(Unit.Default);
            };

            editorView.Show(WindowHost);
            if (maximize)
            {
                editorView.MaximizeCommand.Execute(null);
            }

            var menuItem = new MenuItem
            {
                DataContext = newDocument,
                IsChecked = true
            };
            menuItem.Click += (sender, args) =>
            {
                ((EditorViewModel)((MenuItem)sender).DataContext).ActivateInteraction.Handle(Unit.Default).Wait();
                args.Handled = true;
            };

            editorView.WhenValueChanged(view => view.Title).BindTo(menuItem, item => item.Header);
            ViewMenuItem.Items.Add(menuItem);
        }


        internal MdiViewModel ViewModel => (MdiViewModel)DataContext!;

        private EditorView? ActiveWindow => (EditorView)WindowHost.ActiveWindow;

        private AvaloniaEdit.TextEditor? ActiveEditor => ActiveWindow.Editor;

        private async void MenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            await new AboutWindow().ShowModalAsync(this);
            await ActiveWindow.FocusInternal();
        }

        private async void OnShowSettings(object? sender, RoutedEventArgs e)
        {
            var dlg = new EditSettingsDialog(ViewModel!.Settings.SerializedCopy());
            await dlg.ShowModalAsync(this);
            Settings? newSettings = dlg.Result;
            if (newSettings != null) ViewModel.Settings = newSettings;
            await ActiveWindow.FocusInternal();
        }

        private void EditMenu_OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            AvaloniaEdit.TextEditor editor = ActiveEditor;
            UndoMenuItem.IsEnabled = editor.CanUndo;
            RedoMenuItem.IsEnabled = editor.CanRedo;
            CutMenuItem.IsEnabled = editor is { SelectionLength: > 0 };
            CopyMenuItem.IsEnabled = editor is { SelectionLength: > 0 };
            PasteMenuItem.IsEnabled = true;
        }

        private void UndoMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveEditor.Undo();
        }

        private void RedoMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveEditor.Redo();
        }

        private void CutMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveEditor.Cut();
        }

        private void CopyMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveEditor.Copy();
        }

        private void PasteMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveEditor.Paste();
        }

        private void SelectAllMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveEditor.SelectAll();
        }

        private async Task OpenFileHandler(IInteractionContext<Unit, string?> interactionContext)
        {
            IStorageProvider storageProvider = MainWindow.StorageProvider;
            IReadOnlyList<IStorageFile> files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation =
                    await storageProvider.TryGetFolderFromPathAsync(Directory.GetCurrentDirectory()),
                Title = "Open File"
            });

            // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract todo: check why we declare not to be null while returning null
            if (files?.Count > 0)
                // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            {
                IStorageFile file = files[0];
                interactionContext.SetOutput(file.Path.AbsolutePath);
            }
            else
            {
                interactionContext.SetOutput(null);
            }
        }

        private static void ShutDownHandler(IInteractionContext<Unit, Unit> context)
        {
            ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown();
            context.SetOutput(Unit.Default);
        }

        private void MaximizeMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveWindow!.MaximizeCommand.Execute(null);
        }

        private void RestoreMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveWindow!.RestoreCommand.Execute(null);
        }

        private void CloseMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ActiveWindow!.CloseCommand.Execute(null);
        }
    }
}