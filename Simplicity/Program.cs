using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace Simplicity
{
    static class Program
    {
        public static class Win32
        {

            //Import the FindWindow API to find our window
            [DllImportAttribute("User32")]
            public static extern IntPtr FindWindow(String ClassName, String WindowName);

            //Import the SetForeground API to activate it
            [DllImportAttribute("User32")]
            public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

            [DllImportAttribute("User32")]
            public static extern int ShowWindow(IntPtr hWnd, int state);

            [DllImportAttribute("User32")]
            public static extern int CloseWindow(IntPtr hWnd);

            public const int SW_SHOWMAXIMIZED = 3;
            public const int SW_SHOWDEFAULT = 10;
            public const int SW_RESTORE = 9; 

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MEMORYSTATUSEX
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
                public MEMORYSTATUSEX()
                {
                    this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                }
            }
            
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                private int _Left;
                private int _Top;
                private int _Right;
                private int _Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct WINDOWINFO
            {
                public uint cbSize;
                public RECT rcWindow;
                public RECT rcClient;
                public uint dwStyle;
                public uint dwExStyle;
                public uint dwWindowStatus;
                public uint cxWindowBorders;
                public uint cyWindowBorders;
                public ushort atomWindowType;
                public ushort wCreatorVersion;

                public WINDOWINFO(Boolean? filler)
                    : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
                {
                    cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
                }

            }

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

            public static void ActivateWin(IntPtr hWnd)
            {
                if (hWnd == IntPtr.Zero) return;
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }

            public static void MaximizeWin(IntPtr hWnd)
            {
                if (hWnd == IntPtr.Zero) return;
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
                if (CanMaximizeWin(hWnd))
                {
                    ShowWindow(hWnd, Program.Win32.SW_SHOWMAXIMIZED);
                    System.Threading.Thread.Sleep(150);
                    SetForegroundWindow(hWnd);
                }
            }

            public static bool CanMaximizeWin(IntPtr hwnd)
            {
                if (hwnd == IntPtr.Zero) return false;
                WINDOWINFO info = new WINDOWINFO();
                info.cbSize = (uint)Marshal.SizeOf(info);
                const uint WS_MAXIMIZEBOX = 0x00010000;
                if(GetWindowInfo(hwnd, ref info))
                {
                    if((info.dwStyle & WS_MAXIMIZEBOX) > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Mutex aloneMutex = null;
            try
            {
                SetExceptionHandler();
                string[] nargs = CleanArgs(args); // imp before lock
                if (!Lock(ref aloneMutex, 500))
                {
                    try
                    {
                        IntPtr hWnd = Win32.FindWindow(null, Config.AppName);
                        if (hWnd != IntPtr.Zero)
                        {
                            Win32.SetForegroundWindow(hWnd);
                        }
                    }
                    catch { }
                    return;
                }
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SimpMain(nargs));
                if (aloneMutex != null)
                {
                    aloneMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                string m = ex == null ? "error" : ex.Message;
                MessageBox.Show(null, m, "Error - " + Config.AppName);
            }
        }

        public const string fullScreenPrefix = "/fullscreen";
        public const string pidPrefix = "/s.pid";
        public const string userSelPrefix = "/s.usersel";

        private static string[] CleanArgs(string[] args)
        {
            bool isAutoStart = AutoStart.IsAutoStart;
            Config.Default.userFullscreen = isAutoStart;
            Config.Default.canShowExit = !isAutoStart;

            if (args == null) return null;
            const string starterPathPrefix = "/s.wspath";
            List<string> l = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    if (args[i].StartsWith(starterPathPrefix))
                    {
                        Config.Default.starterPath = args[i].Substring(starterPathPrefix.Length);
                    }
                    else if (args[i].StartsWith(fullScreenPrefix))
                    {
                        Config.Default.userFullscreen = true;
                    }
                    else if (args[i].StartsWith(userSelPrefix))
                    {
                        Config.Default.userInitialPathIsUpFolder = true;
                    }
                    else if (args[i].StartsWith(pidPrefix)) // handle restart
                    {
                        try
                        {
                            int pid = Convert.ToInt32(args[i].Substring(pidPrefix.Length));
                            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                            if(p != null)
                            {
                                p.Kill();
                                p.Close();
                                p = null;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        l.Add(args[i].Trim('\"'));
                    }
                }
                catch { }
            }
            return l.ToArray();
        }

        private static bool Lock(ref Mutex aloneMutex, int waitTime)
        {

            bool createdNew = true;
            if (aloneMutex == null)
            {
                string id = "SFB_EA938C47-0BE0-4AD2-8F18-0CDE3DF35380";
                aloneMutex = new Mutex(false, id, out createdNew);
            }
            bool ok = false;
            try
            {
                ok = aloneMutex.WaitOne(waitTime);
            }
            catch (AbandonedMutexException ex)
            {
                if (ex != null)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                ok = true; // we own mutext here
            }
            return ok;
        }

        private static void SetExceptionHandler()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(UnhandledException);
                Application.ThreadException +=
                    new System.Threading.ThreadExceptionEventHandler(ThreadUnhandledException);
            }
            catch { }
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        static void ThreadUnhandledException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
        }
    }
}
