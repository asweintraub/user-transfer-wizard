using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace UserTransferWizard
{
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";
        private const string User32_DllName = "user32.dll";

        [DllImport(User32_DllName)]
        private static extern bool CloseWindow(IntPtr hwnd);

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        public static void CloseConsole()
        {
            IntPtr ActiveWindowHandle = GetConsoleWindow();
            FreeConsole();
            CloseWindow(ActiveWindowHandle);
        }
    }

}
