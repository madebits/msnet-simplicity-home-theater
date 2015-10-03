using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;

namespace MediaPreview
{
    static class Program
    {
        public static string ffmpeg = null;
        public static string movieFile = null;
        private static bool movieFileIsDir = false;
        public static string APPNAME = "Media Preview 1.0.0 - http://madebits.com";
        public static bool fullScreen = false;
        public static int maxRows = 2;
        public static bool hasffmpeg = false;
        public static int slideShowTimeSec = 5;
        public static bool showImageBorder = false;
        public static bool canLoop = false;
        public static bool startSlideShow = false;
        public static bool autoRotateImages = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                SetExceptionHandler();
                string defaultffmpegPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ffmpeg.exe");
                ffmpeg = defaultffmpegPath;
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        switch (args[i].ToLower())
                        {
                            case "/fullscreen": Program.fullScreen = true; break;
                            case "/ffmpeg": ffmpeg = args[++i]; break;
                            case "/border": showImageBorder = true; break;
                            case "/loop": canLoop = true; break;
                            case "/maxrows":
                                try
                                {
                                    maxRows = Convert.ToInt32(args[++i]);
                                    if( maxRows < 1)  maxRows = 1;
                                    if( maxRows > 10)  maxRows = 10;
                                }
                                catch{}
                                break;
                            case "/slideshow": startSlideShow = true; break;
                            case "/slidespeed":
                                try
                                {
                                    slideShowTimeSec = Convert.ToInt32(args[++i]);
                                    if (slideShowTimeSec < 3) slideShowTimeSec = 3;
                                    if (slideShowTimeSec > 60) slideShowTimeSec = 60;
                                }
                                catch { }
                                break;
                            case "/noautorotate": autoRotateImages = false; break;
                            default:
                                movieFile = args[i].Trim('"');
                                break;
                        }
                    }
                }
                if ((args == null) || string.IsNullOrEmpty(ffmpeg) || string.IsNullOrEmpty(movieFile))
                {
                    string msg = APPNAME + Environment.NewLine + Environment.NewLine;
                    msg += Path.GetFileName(Application.ExecutablePath);
                    msg += " [options] path" + Environment.NewLine;
                    msg += "Where [options] are:" + Environment.NewLine;
                    msg += "  /ffmpeg path to ffmpeg.exe" + Environment.NewLine;
                    msg += "  /fullscreen" + Environment.NewLine;
                    msg += "  /maxrows 2" + Environment.NewLine;
                    msg += "  /slideshow" + Environment.NewLine;
                    msg += "  /slidespeed 5" + Environment.NewLine;
                    msg += "  /loop" + Environment.NewLine;
                    msg += "  /noautorotate" + Environment.NewLine;
                    msg += "  /border" + Environment.NewLine;
                    MessageBox.Show(msg, APPNAME);
                    return;
                }

                if (string.IsNullOrEmpty(movieFile))
                {
                    MessageBox.Show("No path specified!", APPNAME);
                    return;
                }

                hasffmpeg = true;
                if (string.IsNullOrEmpty(ffmpeg))
                {
                    hasffmpeg = false;
                }
                else
                {
                    ffmpeg = Path.GetFullPath(ffmpeg);
                    if (!File.Exists(ffmpeg))
                    {
                        if (!string.IsNullOrEmpty(defaultffmpegPath) 
                            && (ffmpeg.ToLower() != defaultffmpegPath.ToLower()))
                        {
                            if (File.Exists(defaultffmpegPath))
                            {
                                ffmpeg = defaultffmpegPath;
                            }
                            else 
                            {
                                hasffmpeg = false;
                            }
                        }
                        else
                        {
                            hasffmpeg = false;
                        }
                    }
                }
                
                movieFileIsDir = Directory.Exists(movieFile);
                if (!(File.Exists(movieFile) || movieFileIsDir))
                {
                    MessageBox.Show("Path not found!", APPNAME);
                    return;
                }
                
                KillInstances();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Preview(args));
                KillFFMPEG();
            }
            catch (Exception ex) 
            {
                string m = ex == null ? "error" : ex.Message;
                MessageBox.Show(null, m, "Error - " + APPNAME);
            }
        }


        public static void KillInstances()
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                KillInstances(name);
            }
            catch { }
            KillFFMPEG();
        }

        public static void KillFFMPEG()
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(ffmpeg);
                KillInstances(name);
            }
            catch { }
        }

        public static void KillInstances(string name)
        {
            try
            {
                System.Diagnostics.Process cp = System.Diagnostics.Process.GetCurrentProcess();
                System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(name);
                if (p == null) return;
                for (int i = 0; i < p.Length; i++)
                {
                    try
                    {
                        if (p[i].Id == cp.Id) continue;
                        p[i].Kill();
                        p[i].Close();
                        p[i] = null;
                    }
                    catch { }
                }
                cp.Close();
                cp = null;
            }
            catch { }
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
