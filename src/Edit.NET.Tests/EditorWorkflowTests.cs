using Avalonia.Input;
using Consolonia.NUnit;
using EditNET.Tests.Base;
using NUnit.Framework;

namespace EditNET.Tests
{
    // also relevant: https://github.com/Consolonia/Consolonia/issues/679
    // CLAUDE: 
    // The whole scenario is intentionally kept as a single test method in a single fixture: the
    // Consolonia test app is a shared static singleton for the whole test run, so having several
    // test methods (even in several fixtures) means the app/document state leaks between them
    // unless it's carefully reset. Instead, since there's only one app instance anyway, we just
    // drive it through the entire scenario in one place, one step at a time.
    [TestFixture]
    internal class EditorWorkflowTests : EditNetTestsBase
    {
        [Test]
        public async Task FullEditorWorkflow()
        {
            // The app starts with an empty, unnamed, unmodified document.
            await UITest.AssertHasText("File", "Edit", "Help");
            await UITest.AssertHasText("Len 0");
            await UITest.AssertHasText("Ln 1, Col 1");
            await UITest.AssertHasText("Saved");

            // Typing updates the length and the modified status.
            // The editor is focused automatically once the main window is loaded.
            await UITest.StringInput("Hello Edit.NET");

            await UITest.AssertHasText("Hello Edit.NET");
            await UITest.AssertHasText("Len 14");
            await UITest.AssertHasText("Modified");

            // The "New" command on an unsaved document prompts and can be cancelled.
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
            // complete before interacting with the menu again.
            await Task.Delay(600); // todo: introduce UITest.WaitAnimation();

            // Clear the document (select all + delete) so we can continue with a clean document
            // for the next steps (cut/undo/copy, line numbers, context menu), without going
            // through the "New" command (and its confirmation dialog) again.
            await UITest.KeyInput(Key.A, RawInputModifiers.Control); // select all
            await UITest.KeyInput(Key.Delete);

            await UITest.AssertHasText("Len 0");

            // Line numbers are shown on the left side and grow with each new line.
            await UITest.AssertHasText("1");

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

            // Select all and cut remove the text, and undo restores it.
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

            // Copy selection leaves the document unchanged.
            await UITest.KeyInput(Key.A, RawInputModifiers.Control); // select all
            await UITest.KeyInput(Key.C, RawInputModifiers.Control); // copy

            // Unlike Cut, Copy must not modify the document in any way.
            await UITest.AssertHasText("first line");
            await UITest.AssertHasText("second line");
            await UITest.AssertHasText("third line");
            await UITest.AssertHasText("Len 33");

            // The editor's right-click context menu offers Cut/Copy/Paste/Select All. Open it
            // via the keyboard (the "Apps"/"Menu" key, the standard keyboard equivalent of a
            // right mouse click).
            await UITest.KeyInput(Key.Apps);

            await UITest.AssertHasText("Copy");
            await UITest.AssertHasText("Cut");
            await UITest.AssertHasText("Paste");
            await UITest.AssertHasText("Select All");

            // Dismiss the context menu without picking anything.
            await UITest.KeyInput(Key.Escape);
            await UITest.AssertHasNoText("Select All");

            // Exiting with unsaved changes asks and can be cancelled.
            // Make sure the document still has unsaved changes.
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
        }
    }
}