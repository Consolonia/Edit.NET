using Avalonia.Input;
using Consolonia.NUnit;
using EditNET.Tests.Base;
using NUnit.Framework;

namespace EditNET.Tests
{
    [TestFixture]
    internal class EditorWorkflowTests : EditNetTestsBase
    {
        [Test]
        [Order(1)]
        public async Task AppStartsWithEmptyUnnamedDocument()
        {
            await UITest.AssertHasText("File", "Edit", "Help");
            await UITest.AssertHasText("Len 0");
            await UITest.AssertHasText("Ln 1, Col 1");
            await UITest.AssertHasText("Saved");
        }

        [Test]
        [Order(2)]
        public async Task TypingTextUpdatesLengthAndModifiedStatus()
        {
            // The editor is focused automatically once the main window is loaded.
            await UITest.StringInput("Hello Edit.NET");

            await UITest.AssertHasText("Hello Edit.NET");
            await UITest.AssertHasText("Len 14");
            await UITest.AssertHasText("Modified");
        }

        [Test]
        [Order(3)]
        public async Task NewCommandOnAnUnsavedDocumentPromptsAndCanBeCancelled()
        {
            // Open the "File" menu and activate "New" (the first item).
            await UITest.KeyInput(Key.F, RawInputModifiers.Alt);
            await UITest.KeyInput(Key.Down); // highlight "New"
            await UITest.KeyInput(Key.Enter);

            // There are unsaved changes, so a confirmation dialog is shown; cancel it and keep
            // editing the current, still unsaved, document.
            await UITest.AssertHasText("Unsaved", "unsaved changes");
            await UITest.KeyInput(Key.Escape);

            await UITest.AssertHasNoText("unsaved changes");
            await UITest.AssertHasText("Hello Edit.NET");
            await UITest.AssertHasText("Modified");

            // Give the (delayed) focus-restoration triggered by the dialog's dismissal time to
            // complete before the next test interacts with the menu again.
            await Task.Delay(600); // todo: introduce UITest.WaitAnimation();
        }

        [Test]
        [Order(4)]
        public async Task ExitingWithUnsavedChangesAsksAndCanBeCancelled()
        {
            // Make sure the document has unsaved changes.
            await UITest.StringInput("!");
            await UITest.AssertHasText("Modified");

            // Activate "Exit" via the "File" menu (New, [Open, Save, SaveAs, separator], Exit
            // is the last item), without any mouse or direct code invocation.
            await UITest.KeyInput(Key.F, RawInputModifiers.Alt);
            await UITest.AssertHasText("Exit");
            await UITest.KeyInput(Key.Down, Key.Down, Key.Down, Key.Down, Key.Down);
            await UITest.KeyInput(Key.Enter);
            await UITest.WaitRendered(); // let the confirmation dialog finish rendering

            await UITest.AssertHasText("Unsaved", "unsaved changes");

            await UITest.KeyInput(Key.Escape); // Cancel, don't close and don't save

            await UITest.AssertHasNoText("unsaved changes");
            await UITest.AssertHasText("File", "Edit", "Help"); // app is still running

            // Give the (delayed) focus-restoration triggered by the dialog's dismissal time to
            // complete before the next test types into the editor again.
            await Task.Delay(600);  // todo: introduce UITest.WaitAnimation();
        }
        
        private static async Task ClearDocumentAsync()
        {
            await UITest.KeyInput(Key.A, RawInputModifiers.Control); // select all
            await UITest.KeyInput(Key.Delete);
            await UITest.AssertHasText("Len 0");
        }

        [Test]
        [Order(50)]
        public async Task LineNumbersAreShownOnTheLeftSideAndGrowWithEachNewLine()
        {
            await ClearDocumentAsync();

            // Starting from a clean, single-line document, the gutter should only show line "1".
            await UITest.AssertHasText("1");

            // Add a couple more lines and check the gutter grows accordingly.
            await UITest.StringInput("first line");
            await UITest.KeyInput(Key.Enter);
            await UITest.StringInput("second line");
            await UITest.KeyInput(Key.Enter);
            await UITest.StringInput("third line");

            await UITest.AssertHasText("Ln 3, Col 11");
            await UITest.AssertHasText("first line");
            await UITest.AssertHasText("second line");
            await UITest.AssertHasText("third line");

            // Every line's number must be present in the gutter, in order, one per line.
            await UITest.AssertHasMatch(@"1[^\r\n]*first line[\s\S]*2[^\r\n]*second line[\s\S]*3[^\r\n]*third line");
        }

        [Test]
        [Order(60)]
        public async Task SelectAllAndCutRemoveTheTextAndUndoRestoresIt()
        {
            // Continues from the 3-line document left by the previous test.
            // Note: an actual OS clipboard is not available in this headless console, so the
            // Cut/Paste round-trip is verified through Undo (which Cut records as one action)
            // rather than by reading the text back from the clipboard.
            await UITest.KeyInput(Key.A, RawInputModifiers.Control); // select all
            await UITest.KeyInput(Key.X, RawInputModifiers.Control); // cut

            await UITest.AssertHasText("Len 0");
            await UITest.AssertHasNoText("first line");

            await UITest.KeyInput(Key.Z, RawInputModifiers.Control); // undo the cut

            await UITest.AssertHasText("first line");
            await UITest.AssertHasText("second line");
            await UITest.AssertHasText("third line");
            await UITest.AssertHasText("Ln 3, Col 11");
        }

        [Test]
        [Order(70)]
        public async Task CopySelectionLeavesTheDocumentUnchanged()
        {
            // Continues from the 3-line document left by the previous test.
            await UITest.KeyInput(Key.A, RawInputModifiers.Control); // select all
            await UITest.KeyInput(Key.C, RawInputModifiers.Control); // copy

            // Unlike Cut, Copy must not modify the document in any way.
            await UITest.AssertHasText("first line");
            await UITest.AssertHasText("second line");
            await UITest.AssertHasText("third line");
            await UITest.AssertHasText("Len 33");
        }

        [Test]
        [Order(80)]
        public async Task ContextMenuOverTheEditorOffersCutCopyPasteAndSelectAll()
        {
            // Open the editor's right-click context menu via the keyboard (the "Apps"/"Menu"
            // key, the standard keyboard equivalent of a right mouse click).
            await UITest.KeyInput(Key.Apps);

            await UITest.AssertHasText("Copy");
            await UITest.AssertHasText("Cut");
            await UITest.AssertHasText("Paste");
            await UITest.AssertHasText("Select All");

            // Dismiss the context menu without picking anything.
            await UITest.KeyInput(Key.Escape);
            await UITest.AssertHasNoText("Select All");
        }
    }
}
