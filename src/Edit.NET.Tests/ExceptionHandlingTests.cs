using System.Reflection;
using Avalonia.Input;
using Avalonia.Threading;
using Consolonia.NUnit;
using EditNET.Tests.Base;
using NUnit.Framework;

namespace EditNET.Tests
{
    [TestFixture]
    [Ignore("Test must be run in RELEASE - this must be fixed in Consolonia")] //todo:
    internal class ExceptionHandlingTests() : EditNetTestsBase("--debug")
    {
        static ExceptionHandlingTests()
        {
            App.HandleProcessExceptions(null);
        }

        [Test]
        public async Task ExceptionFromDispatcherIsShown()
        {
            await SelectThrowMenuItem(0);

            try
            {
                await UITest.KeyInput(Key.Enter);
                Assert.Fail(
                    "Input in UITest must be Dispatched with Dispatcher. This will fail once fixed. Then remove this and try/catch.");
            }
            catch (TargetInvocationException exception) when (exception.InnerException is InvalidOperationException)
            {
            }

            await AssertErrorDialogShowsException(@"exception\W+thrown\W+from\W+dispatcher");
        }

        [Test]
        public async Task ExceptionFromThreadPoolIsShown()
        {
            await SelectThrowMenuItem(1);
            await UITest.KeyInput(Key.Enter);

            await AssertErrorDialogShowsException(@"exception\W+thrown\W+from\W+thread\W+pool");
        }

        [Test]
        public async Task ExceptionFromUnobservedTaskIsShown()
        {
            await SelectThrowMenuItem(2);
            await UITest.KeyInput(Key.Enter);

            await AssertErrorDialogShowsException(@"exception\W+thrown\W+from\W+unobserved\W+task");
        }

        private async Task SelectThrowMenuItem(int index)
        {
            await WaitForText("Debug");
            await UITest.WaitRendered();

            await UITest.AssertHasText("File", "Edit", "Help");
            await UITest.AssertHasText("1 document(s)");

            await UITest.WaitRendered();

            await UITest.KeyInput(Key.D, RawInputModifiers.Alt);
            await UITest.AssertHasText("Exceptions");
            await UITest.KeyInput(Key.Right);
            await UITest.AssertHasText("Throw from within dispatcher", "Throw from thread pool",
                "Throw from unobserved task");
            await UITest.KeyInput(index, Key.Down);
        }

        private async Task AssertErrorDialogShowsException(string messagePattern)
        {
            await UITest.WaitRendered(); // dialog will be shown by dispatching new cycle

            await WaitForText("Unexpected Error"); // threadpool and dispatcher will show error at random moment

            await UITest.AssertHasText("Unexpected Error", "An internal error has occurred");
            await UITest.AssertHasNoText("InvalidOperationException"); // details are collapsed
            await UITest.AssertHasNoMatch(messagePattern);

            // Focus is on the "report" link initially, the next one is the "Exception Details" expander.
            await UITest.KeyInput(Key.Tab);
            await UITest.KeyInput(Key.Space);

            await UITest.AssertHasText("InvalidOperationException");
            await UITest.AssertHasMatch(messagePattern);

            // "Ignore" closes the dialog and the application keeps running.
            await UITest.KeyInput(Key.Escape);
            await UITest.AssertHasNoText("Exception Details");
            await UITest.AssertHasText("File", "Edit", "Help");
        }

        private async Task WaitForText(string text)
        {
            using var cancellationTokenSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(20)); //1 min we can wait for all tests
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                bool found = await Dispatcher.UIThread.InvokeAsync(() =>
                    UITest.PixelBuffer.PrintBuffer().Contains(text, StringComparison.Ordinal));
                if (found)
                    break;

                await UITest.WaitRendered();
            }

            await UITest.AssertHasText(text);
        }
    }
}