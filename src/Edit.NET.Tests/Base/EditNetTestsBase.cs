using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;
using EditNET.Views;

namespace EditNET.Tests.Base
{
    /// <summary>
    ///     Base class for Edit.NET end-to-end (UI) tests.
    /// </summary>
    internal abstract class EditNetTestsBase : ConsoloniaAppTestBase<App>
    {
        static EditNetTestsBase()
        {
            EditorView.Animate = false;
        }

        protected EditNetTestsBase(params string[] args) : base(new PixelBufferSize(100, 30))
        {
            Args = args;
        }
    }
}
