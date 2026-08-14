using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;

namespace EditNET.Tests.Base
{
    /// <summary>
    ///     Base class for Edit.NET end-to-end (UI) tests.
    /// </summary>
    internal abstract class EditNetTestsBase : ConsoloniaAppTestBase<App>
    {
        protected EditNetTestsBase() : base(new PixelBufferSize(100, 30))
        {
            Args = [];
        }
    }
}