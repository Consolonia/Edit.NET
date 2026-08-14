using Avalonia.Input;
using Consolonia.NUnit;
using EditNET.Tests.Base;
using NUnit.Framework;

namespace EditNET.Tests
{
    [TestFixture]
    internal class EditorContextMenuTests : EditNetTestsBase
    {
        [Test]
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