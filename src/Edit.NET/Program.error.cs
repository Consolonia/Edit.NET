using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EditNET.Helpers;

namespace EditNET
{
    public static partial class Program
    {
        public const int ShutdownTimeoutSeconds = 60;

        static Program()
        {
            System.Runtime.ExceptionServices.ExceptionHandling.SetUnhandledExceptionHandler(exception =>
            {
                if (Thread.CurrentThread == _mainThread)
                    return false;

                if (exception is CrashAppException)
                {
                    Debug.WriteLine("Application is going to crash");
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    return false;
                }

                _ = App.ShowApplicationError(exception);
#if HANDLE_CRASH
                // preventing process from crash in RELEASE
                return true;
#else
                return false;
#endif
            });

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                if (args.Exception.InnerException is CrashAppException)
                    ThreadPool.QueueUserWorkItem(_ => throw new CrashAppException(args.Exception));

                args.SetObserved();

                if (args.Exception.InnerException!.Data.Contains(App.HandledExceptionHackMark))
                    return;

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine("An Unhandled exception was thrown.");
                    Debugger.Break();
                }

                _ = App.ShowApplicationError(args.Exception);
            };
        }
    }
}