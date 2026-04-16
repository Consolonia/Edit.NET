using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Consolonia.Controls;
using Consolonia.Modal;
using EditNET.Helpers;
using EditNET.Views;

namespace EditNET
{
    public partial class App
    {
        /// <summary>
        ///     Avalonia rethrows an exception to unobserved task, which we also handles. Thus marking this exception to know it
        ///     was handled already
        ///     Why not just to show it from UnobservedTask? Because we don't know when that will happen (finalizer call)
        /// </summary>
        internal static readonly object HandledExceptionHackMark = new();

        private static void HandleDispatcherExceptions()
        {
#if HANDLE_CRASH
            Dispatcher.UIThread.UnhandledExceptionFilter += async (_, args) =>
            {
                args.RequestCatch = true;
                args.Exception.Data[HandledExceptionHackMark] = null;
                await ShowApplicationError(args.Exception);
            };

            Dispatcher.UIThread.UnhandledException += (_, args) => { args.Handled = true; };
#endif
        }

        /// <summary>
        ///     Try to show a message box with the error and suggest to restart the application.
        ///     If the dialog is not closed within 30 seconds or if it's throws an exception the application will do a regular
        ///     crash
        /// </summary>
        /// <param name="exception">Exception to report</param>
        internal static async Task ShowApplicationError(Exception exception)
        {
            //todo: there is no much sense to show the exception to users. We should report it via telemetry ourselves

            var tsc = new CancellationTokenSource();
            var wrappedException = new CrashAppException(exception);

            _ = Task.Delay(TimeSpan.FromSeconds(Program.ShutdownTimeoutSeconds), tsc.Token)
                .ContinueWith(_ =>
                    {
                        // this does not write exception to output: Environment.FailFast(null, exception);
                        // crashing the application because dialog can not be shown
                        CrashViaThreadPool();
                    }, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            try
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                        Window? mainWindow = desktop?.MainWindow;

                        if (mainWindow != null)
                            await new ErrorWindow(exception).ShowModalAsync(mainWindow);
                        else
                            CrashViaThreadPool();
                    }
                    catch (Exception secondaryException)
                    {
                        // crashing the application because dialog can not be shown
                        wrappedException = new CrashAppException(new AggregateException(exception, secondaryException));
                        CrashViaThreadPool();
                        throw;
                    }
                });
            }
            catch (Exception secondaryException)
            {
                // crashing the application because dialog can not be shown
                wrappedException = new CrashAppException(new AggregateException(exception, secondaryException));
                CrashViaThreadPool();
                throw;
            }

            await tsc.CancelAsync();

            return;

            void CrashViaThreadPool()
            {
                // ReSharper disable once AccessToModifiedClosure  It's intentionally!! Yes, we want to modify captured variable, why...
                ThreadPool.QueueUserWorkItem(_ => throw wrappedException);
            }
        }
    }
}