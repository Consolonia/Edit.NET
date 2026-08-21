// DUPFINDER_ignore

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

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
                // reproducing same condition as PlatformSupportExtensions of Consolonia
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
            // Claude: DefaultDllImportSearchPaths restricts native library probing to safe/system directories,
            // avoiding a DLL search-order hijacking risk (fixes code-analysis warning CA5392).
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.SafeDirectories)]
#pragma warning disable CA2101
            // Claude:
            // Open must NOT use CharSet.Unicode: that marshals "pathname" as a UTF-16 string, but libc's
            // open() expects a plain null-terminated byte string (char*). With CharSet.Unicode, the native
            // call only sees the first byte of "/dev/tty" followed by a 0x00 byte (the low byte of the
            // UTF-16 encoding of the second character), i.e. effectively just "/". open("/", O_RDONLY)
            // succeeds (it's a valid directory), so no error is ever surfaced - but the resulting file
            // descriptor is a directory, not a terminal, so reads on it (e.g. via get_wch) never return
            // usable input and the app appears to hang silently. Using the default/ANSI marshaling (UTF-8
            // on Unix .NET) passes the correct, fully null-terminated "/dev/tty" string.
            [DllImport("libc", EntryPoint = "open", SetLastError = true)]
#pragma warning restore CA2101
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