// DUPFINDER_ignore

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Consolonia.Core.Infrastructure;
using Consolonia.PlatformSupport;

namespace EditNET
{
    /// <summary>
    ///     Encapsulates the workaround required to keep Edit.NET usable when its standard input is
    ///     redirected/piped (e.g. "env | Edit.NET")
    /// </summary>
    internal static class PipingWorkaround
    {
        public static string? PipedInputContent { get; private set; }
        
        public static void ReadPipedInputAndReattachStdInFromTerminalIfRedirected()
        {
            PipedInputContent = null;

            if (!Console.IsInputRedirected)
                return;

            string pipedContent = Console.In.ReadToEnd();
            PipedInputContent = pipedContent;

            if (Environment.OSVersion.Platform is not (PlatformID.Unix or PlatformID.MacOSX))
                return;
            
            const int stdinFileDescriptor = 0;
            const int openReadOnly = 0;

            int ttyFileDescriptor = NativeMethods.Open("/dev/tty", openReadOnly);
            if (ttyFileDescriptor < 0)
                throw new IOException(
                    "Failed to open /dev/tty to reattach standard input after reading piped content: " +
                    new Win32Exception(Marshal.GetLastWin32Error()).Message);

            if (NativeMethods.Dup2(ttyFileDescriptor, stdinFileDescriptor) < 0)
                throw new IOException(
                    "Failed to reattach standard input (fd 0) to /dev/tty via dup2: " +
                    new Win32Exception(Marshal.GetLastWin32Error()).Message);

            if (NativeMethods.Close(ttyFileDescriptor) < 0)
                throw new IOException(
                    "Failed to close /dev/tty file descriptor after reattaching standard input: " +
                    new Win32Exception(Marshal.GetLastWin32Error()).Message);
        }

        private static class NativeMethods
        {
            // todo: DefaultDllImportSearchPaths is by Claude. I don't have idea what it does
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.SafeDirectories)]
            [DllImport("libc", EntryPoint = "open", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int Open(string pathname, int flags);

            [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.SafeDirectories)]
            [DllImport("libc", EntryPoint = "dup2", SetLastError = true)]
            public static extern int Dup2(int oldFileDescriptor, int newFileDescriptor);

            [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.SafeDirectories)]
            [DllImport("libc", EntryPoint = "close", SetLastError = true)]
            public static extern int Close(int fileDescriptor);
        }
    }
}
