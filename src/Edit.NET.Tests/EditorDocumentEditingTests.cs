using Avalonia.Input;
using Consolonia.NUnit;
using EditNET.Tests.Base;
using NUnit.Framework;

namespace EditNET.Tests
{
    [TestFixture]
    internal class EditorDocumentEditingTests : EditNetTestsBase
    {
        [Test]
        public async Task LineNumbersCutUndoAndCopyWorkflow()
        {
            // Starting from a clean, single-line document, the gutter should only show line "1".
            await UITest.AssertHasText("1");

            // Line numbers are shown on the left side and grow with each new line.
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
        }
    }
}