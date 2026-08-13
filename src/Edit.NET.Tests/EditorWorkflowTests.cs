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
            await Task.Delay(600); // todo: sleep driven development
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
        }
    }
}
