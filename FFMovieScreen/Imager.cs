using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace MediaPreview
{
    class Imager
    {

        public static Image GetDiskImage(string file, Size imageSize)
        {
            Image b = null;
            try
            {
                if (string.IsNullOrEmpty(file))
                {
                    return null;
                }
                FileInfo fi = new FileInfo(file);
                if ((fi.Length <= 0) || (fi.Length > (1024 * 1024 * 64)))
                {
                    fi = null;
                    return null;
                }
                fi = null;
                using (Stream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024))
                {
                    using (Bitmap bmp = new Bitmap(fs))
                    {
                        if (bmp == null) return null;
                        Size thumbSize = FitSize(bmp.Size, imageSize);
                        if ((thumbSize.Width < 128) || (thumbSize.Height < 128))
                        {
                            b = bmp.GetThumbnailImage(thumbSize.Width, thumbSize.Height, null, IntPtr.Zero);
                        }
                        else
                        {
                            if (Program.autoRotateImages)
                            {
                                try
                                {
                                    System.Drawing.RotateFlipType r = AutoRotate.GetThumbAutoRotate(bmp.PropertyItems);
                                    if (r != System.Drawing.RotateFlipType.RotateNoneFlipNone)
                                    {
                                        bmp.RotateFlip(r);
                                    }
                                }
                                catch { }
                            }
                            b = GetFitImage(bmp, imageSize);
                        }
                    }
                }
            }
            catch { }
            return b;
        }

        private static Image GetFitImage(Bitmap b, Size imageSize) 
        {
            if (b == null) return null;
            Size thumbSize = FitSize(b.Size, imageSize);
            if((thumbSize.Width == b.Width) && (thumbSize.Height == b.Height))
            {
                return (Image)b.Clone();
            }
            Bitmap nb = new Bitmap(thumbSize.Width, thumbSize.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, 0, 0, nb.Width, nb.Height);
            }
            return nb;
        }

        public static Bitmap GetIcon(string file) 
        {
            Bitmap b = null;
            string ext = "*";
            if (!string.IsNullOrEmpty(file))
            {
                ext = Path.GetExtension(file);
            }
            if (string.IsNullOrEmpty(ext)) ext = "*";
            ext = ext.ToLower();
            switch (ext)
            {
                case ".exe":
                case ".ico":
                    using (Icon c = BlackFox.Win32.Icons.IconFromShell(file, BlackFox.Win32.Icons.SystemIconSize.Large))
                    {
                        if (c != null)
                        {
                            try
                            {
                               b = c.ToBitmap();
                            }
                            catch { }
                        }
                    }
                    break;
                default:
                    using (Icon c = BlackFox.Win32.Icons.IconFromExtensionShell(ext, BlackFox.Win32.Icons.SystemIconSize.Large))
                    {
                        if (c != null)
                        {
                            try
                            {
                                b = c.ToBitmap();
                            }
                            catch { }
                        }
                    }
                    break;
            }
            return b;
        }

        public static Image GetImage(string outFile, Size imageSize)
        {
            if (string.IsNullOrEmpty(outFile)) return null;
            if (File.Exists(outFile))
            {
                using (Stream s = new FileStream(outFile, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024))
                {
                    try
                    {
                        using (Bitmap bmp = new Bitmap(s))
                        {
                            if (bmp != null)
                            {
                                return GetFitImage(bmp, imageSize);
                            }
                        }
                    }
                    catch { }
                }
                try
                {
                    File.Delete(outFile);
                }
                catch { }
            }
            return null;
        }


        public static Size FitSize(Size original, Size container)
        {
            if ((original.Width > container.Width)
                || (original.Height > container.Height))
            {
                return ScaleSize(original, container);
            }
            return original;
        }

        public static Size ScaleSize(Size original, Size container)
        {
            int w = 0, h = 0;
            ScaleSize(original.Width, original.Height, container.Width, container.Height, ref w, ref h);
            return new Size(w, h);
        }

        private static void ScaleSize(int bw, int bh, int pw, int ph, ref int nbw, ref int nbh)
        {
            nbw = 0;
            nbh = 0;
            bool bhBigger = false;
            if (bh > bw) bhBigger = true;
            if (bhBigger)
            {
                nbh = ph;
                nbw = ((nbh * bw) / bh);
                if (nbw > pw)
                {
                    nbw = pw;
                    nbh = ((nbw * bh) / bw);
                }
            }
            else
            {
                nbw = pw;
                nbh = ((nbw * bh) / bw);
                if (nbh > ph)
                {
                    nbh = ph;
                    nbw = ((nbh * bw) / bh);
                }
            }
        }

        public static string GetTempFile(bool deleteExisting)
        {
            const string outFileName = "mediapreview";
            string outFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            outFile = Path.Combine(outFile, "Simplicity Home Theater"); //MP{B89828E6-847A-43D9-B41C-A49170B49DD0}");
            if (!Directory.Exists(outFile))
            {
                Directory.CreateDirectory(outFile);
            }
            outFile = Path.Combine(outFile, outFileName + ".jpg");
            if (deleteExisting)
            {
                try
                {
                    if (File.Exists(outFile))
                    {
                        File.Delete(outFile);
                    }
                }
                catch { }
            }
            return outFile;
        }

        public static string GetScreenShot(string inFileName, string timeStamp)
        {
            try
            {
                if (!Program.hasffmpeg) return null;
                if (string.IsNullOrEmpty(timeStamp))
                {
                    timeStamp = "10";
                }
                string outFile = GetTempFile(true);
                //ffmpeg -ss 00:20:00 -vframes 1 -i file.avi -y -f image2 out.png
                System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
                ps.ErrorDialog = false;
                ps.RedirectStandardOutput = true;
                ps.RedirectStandardError = true;
                ps.FileName = Program.ffmpeg;
                //ps.FileName = @"d:\temp1\tools\ffmpeg.exe";
                //ps.Arguments = @"-ss 00:20:00 -vframes 1 -i ""D:\movies\guliver.avi"" -y -f image2 ""C:\Users\D7\AppData\Local\MP{B89828E6-847A-43D9-B41C-A49170B49DD0}\lastscreen.jpg""";
                ps.Arguments = GetCmd(inFileName, outFile, timeStamp);
                System.Diagnostics.Debug.WriteLine("ff: " + ps.Arguments);
                //ps.Arguments = cmd;
                ps.UseShellExecute = false;
                ps.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                ps.CreateNoWindow = true;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(ps);
                if (p != null)
                {
                    using (System.IO.StreamReader errOut = p.StandardError)
                    {
                        if (p.WaitForExit(6000))
                        {
                            if (p.HasExited)
                            {
                                string output = errOut.ReadToEnd();
                                System.Diagnostics.Debug.WriteLine(output);
                            }
                        }
                        else 
                        {
                            p.Kill();
                        }
                    }
                    p.Close();
                    p = null;
                }
                return outFile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
            return null;
        }

        private static string GetCmd(string inFileName, string outFile, string time)
        {
            const string cmd = @"-ss %T% -vframes 1 -i ""%M%"" -y -f image2 ""%O%""";
            string t = cmd;
            t = t.Replace("%T%", time);
            t = t.Replace("%M%", inFileName);
            t = t.Replace("%O%", outFile);
            return t;
        }

        public static TimeSpan GetLength(string movie)
        {
            try
            {
                if (!Program.hasffmpeg) return TimeSpan.MaxValue;
                if (!CouldBeMovie(movie)) return TimeSpan.MaxValue;
                if (!File.Exists(movie)) return TimeSpan.MaxValue;
                System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
                ps.ErrorDialog = false;
                ps.RedirectStandardOutput = true;
                ps.RedirectStandardError = true;
                ps.FileName = Program.ffmpeg;
                if (ps.FileName != Program.ffmpeg)
                {
                    for (int i = 0; i < ps.FileName.Length; i++)
                    {
                        System.Diagnostics.Debug.WriteLine(i + " [" + ps.FileName[i] + "]");
                    }
                    for (int i = 0; i < Program.ffmpeg.Length; i++)
                    {
                        System.Diagnostics.Debug.WriteLine(i + " [" + Program.ffmpeg[i] + "]");
                    }
                }
                ps.Arguments = "-i \"" + movie + "\"";
                System.Diagnostics.Debug.WriteLine("ff: " + ps.Arguments);
                ps.UseShellExecute = false;
                ps.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                ps.CreateNoWindow = true;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(ps);
                if (p != null)
                {
                    using (System.IO.StreamReader errOut = p.StandardError)
                    {
                        if (p.WaitForExit(6000))
                        {
                            if (p.HasExited)
                            {
                                string output = errOut.ReadToEnd();
                                //System.Diagnostics.Debug.WriteLine(output);
                                return ParseTimeSpan(output);
                            }
                        }
                        else
                        {
                            p.Kill();
                        }
                    }
                    p.Close();
                    p = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
            return TimeSpan.MaxValue;
        }

        private static TimeSpan ParseTimeSpan(string output)
        {
            if (string.IsNullOrEmpty(output))
            {
                return TimeSpan.MaxValue;
            }
            string[] lines = output.Split('\n');
            TimeSpan ts = TimeSpan.MaxValue;
            bool foundVideo = false;
            foreach (string line in lines)
            {
                string l = line.Trim('\n', '\r', ' ', '\t');
                if (l.StartsWith("Input #0, tty"))
                {
                    return TimeSpan.MaxValue;
                }
                else if (l.StartsWith("Duration: "))
                {
                    l = l.Substring(10);
                    int idx = l.IndexOfAny(new char[] { ',', ' ' });
                    if (idx > 0)
                    {
                        string time = l.Substring(0, idx);
                        if (!string.IsNullOrEmpty(time) && (time != "N/A"))
                        {
                            ts = TimeSpan.Parse(time.Trim(':', ',', ' ', '\t'));
                        }
                    }
                }
                else if (l.IndexOf("Video:") > 0)
                {
                    foundVideo = true;
                    if (ts != TimeSpan.MaxValue)
                    {
                        break;
                    }
                }
            }
            if (!foundVideo)
            {
                return TimeSpan.MaxValue;
            }
            return ts;
        }

        private static bool CouldBeMovie(string f)
        {
            if (string.IsNullOrEmpty(f)) return false;
            string ext = Path.GetExtension(f).ToLower();
            switch (ext)
            {
                case ".bmp":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".png":
                case ".tiff":
                case ".wmf":
                case ".emf":
                case ".ico":
                case ".cur":
                case ".ani":
                case ".exe":
                case ".com":
                case ".msi":
                case ".dll":
                case ".txt":
                case ".bat":
                case ".ini":
                case ".log":
                case ".cpp":
                case ".xml":
                case ".xsl":
                case ".dtd":
                case ".cat":
                case ".ocx":
                case ".cpl":
                case ".drv":
                case ".sys":
                case ".inf":
                case ".nfo":
                case ".sub":
                case ".srt":
                case ".cmd":
                case ".vbs":
                case ".olb":
                case ".tlb":
                case ".nls":
                case ".zip":
                case ".rar":
                case ".tar":
                case ".7z":
                case ".gz":
                case ".bz2":
                case ".lnk":
                case ".pdf":
                case ".chm":
                case ".htm":
                case ".html":
                case ".url":
                    return false;
            }
            return true;
        }

        public static string GetTime(int i, int max, TimeSpan tsMax, bool shortTime)
        {
            TimeSpan time = new TimeSpan(0, 0, (i * 4) + 1);
            if (tsMax != TimeSpan.MaxValue)
            {
                long seconds = 0;
                long delta = 1;
                if (max <= 1)
                {
                    if (shortTime)
                    {
                        seconds = (long)tsMax.TotalSeconds;
                    }
                    else
                    {
                        seconds = (long)tsMax.TotalSeconds / 2;
                    }
                }
                else
                {
                    long period = (long)tsMax.TotalSeconds / (max - 1);
                    delta = period / 10;
                    if (delta > 10) delta = 10;
                    if (delta < 1) delta = 1;

                    if (i == 0) seconds = delta;
                    else if (i == (max - 1)) 
                    {
                        if (shortTime)
                        {
                            seconds = (long)tsMax.TotalSeconds;
                        }
                        else
                        {
                            seconds = (long)tsMax.TotalSeconds - delta;
                        }
                    }
                    else
                    {
                        seconds = period * (long)i;
                    }
                }
                if (seconds <= 0)
                {
                    seconds = delta;
                }
                if (seconds > (long)tsMax.TotalSeconds)
                {
                    seconds = (long)tsMax.TotalSeconds;
                }
                time = new TimeSpan(seconds * TimeSpan.TicksPerSecond);
            }
            if (shortTime)
            {
                string h = time.Hours.ToString();
                if (h.Length == 1) h = "0" + h;
                string m = time.Minutes.ToString();
                if (m.Length == 1) m = "0" + m;
                string s = time.Seconds.ToString();
                if (s.Length == 1) s = "0" + s;
                return h + ":" + m + ":" + s;
            }
            return time.TotalSeconds.ToString();
        }

    }//EOC
}
