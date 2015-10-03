using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Simplicity
{
    class AppItem : IComparable<AppItem>
    {
        private string name = null;
        private string path = null;
        private string iconPath = null;
        public string args = null;
        public string[] appliesTo = null;
        public bool appliesToFolders = false;
        public bool appliesToAllFiles = false;

        public int CompareTo(AppItem other) 
        {
            if (IsSame(other)) return 0;
            int r = ns.StringLogicalComparer.Default.Compare(this.name, other.name);
            if (r != 0) return r;
            r = ns.StringLogicalComparer.Default.Compare(this.path, other.path);
            return r;
        }

        public bool IsSame(AppItem other) 
        {
            if (other == null) return false;
            bool same = (this.name == other.name)
                && (this.path == other.path)
                && (this.iconPath == other.iconPath)
                && (this.args == other.args)
                && (appliesToFolders == other.appliesToFolders)
                && (appliesToAllFiles == other.appliesToAllFiles);
            if (!same) return same;
            if ((this.appliesTo == null) && (other.appliesTo != null))
            {
                return false;
            }
            if ((this.appliesTo != null) && (other.appliesTo == null))
            {
                return false;
            }
            if ((this.appliesTo != null) && (other.appliesTo != null))
            {
                if (this.appliesTo.Length != other.appliesTo.Length)
                {
                    return false;
                }
                for (int i = 0; i < this.appliesTo.Length; i++) 
                {
                    if (this.appliesTo[i] != other.appliesTo[i]) 
                    {
                        return false;
                    }
                }
            }
            return same;
        }

        public string Path
        {
            get { return path; }
            set 
            {
                if (value == "*")
                {
                    path = value;
                }
                else
                {
                    path = SControls.BrowserControl.CleanPath(value);
                }
                if (!IsShellPath && (!string.IsNullOrEmpty(path)))
                {
                    if (!File.Exists(path) || Directory.Exists(path))
                    {
                        path = null;
                    }
                }
            }
        }

        public string Name
        {
            get
            {
                SetNameFromPath();
                return name;
            }
            set
            {
                name = value;
            }
        }

        public void SetNameFromPath() 
        {
            if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(path) && !IsShellPath)
            {
                try
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path);
                }
                catch (Exception ex) { Config.Default.Error(ex); }
            }
        }

        public string IconPath
        {
            get
            {
                if (!string.IsNullOrEmpty(this.iconPath))
                {
                    return this.iconPath;
                }
                return this.path;
            }
            set 
            {
                this.iconPath = SControls.BrowserControl.CleanPath(value);
                if (!string.IsNullOrEmpty(iconPath))
                {
                    if (!File.Exists(iconPath) || Directory.Exists(iconPath))
                    {
                        iconPath = null;
                    }
                }
            }
        }

        public void SetFilter(string v) 
        {
            appliesTo = null;
            if (string.IsNullOrEmpty(v))
            {
                return;
            }
            appliesTo = v.ToUpper().Split(',');
            for (int j = 0; j < appliesTo.Length; j++)
            {
                appliesTo[j] = appliesTo[j].Trim();
                if (appliesTo[j] == "*")
                {
                    appliesToFolders = true;
                }
                else if (appliesTo[j] == "?")
                {
                    appliesToAllFiles = true;
                }
                else
                {
                    appliesTo[j] = appliesTo[j].Replace("*", string.Empty).Trim();
                    appliesTo[j] = appliesTo[j].Replace("?", string.Empty).Trim();
                }
            }
        }

        public bool IsShellPath
        {
            get { return ((this.path != null) && (this.path == "*")); }
        }

        public bool IsValid
        {
            get 
            {
                return (!string.IsNullOrEmpty(name) 
                    && !string.IsNullOrEmpty(path));
            }
        }

        public bool AppliesTo(string path, bool isFolder)
        {
            if ((appliesTo == null) || (appliesTo.Length <= 0))
            {
                return true;
            }
            if (appliesToAllFiles && !isFolder) 
            {
                return true;
            }
            if (appliesToFolders && isFolder)
            {
                return true;
            }
            string p = path.ToUpper();
            for (int i = 0; i < appliesTo.Length; i++)
            {
                if (string.IsNullOrEmpty(appliesTo[i])) continue;
                if(p.EndsWith(appliesTo[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasSpace(string path) 
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            return (path.IndexOfAny(new char[]{' ', '\t'}) >= 0);
        }

        public void Run(string fileOrFolder)
        {
            if(!IsValid) throw new Exception();
            if (fileOrFolder == null)
            {
                fileOrFolder = string.Empty;
            }
            string pargs = ReplaceMeta(args, fileOrFolder);
            string log = "Launch: " + this.path;
            System.Diagnostics.Process p = null;
            if (IsShellPath)
            {
                if (!string.IsNullOrEmpty(pargs))
                {
                    log += " " + pargs;
                    Config.Default.Log(log);
                    p = System.Diagnostics.Process.Start(pargs);
                }
            }
            else
            {
                System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
                ps.FileName = this.path;
                if (!string.IsNullOrEmpty(pargs))
                {
                    log += " " + pargs;
                    ps.Arguments = pargs;
                }
                Config.Default.Log(log);
                p = System.Diagnostics.Process.Start(ps);
            }
            if (p != null)
            {
                MaximizeProcessWin(p);
            }
        }

        private static string CleanStr(string s, bool removeDirEndSeparator)
        {
            if (s == null) return string.Empty;
            if (removeDirEndSeparator)
            {
                if (s.Length > 3)
                {
                    if (s.EndsWith("\\")) s = s.Substring(0, s.Length - 1);
                }
            }
            return s;
        }

        private static string ReplaceMeta(string s, string fileOrFolder)
        {
            fileOrFolder = CleanStr(fileOrFolder, true);
            string name = string.Empty;
            string cleanName = string.Empty;
            string dir = fileOrFolder;
            if (!string.IsNullOrEmpty(fileOrFolder))
            {
                name = SControls.BrowserControl.GetFileName(fileOrFolder).Trim('\\', '/', ':');
                cleanName = MovieName.Clean(name);
                try
                {
                    if (File.Exists(fileOrFolder) && !Directory.Exists(fileOrFolder))
                    {
                        dir = System.IO.Path.GetDirectoryName(fileOrFolder);
                        if (dir == null) dir = string.Empty;
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
            }
            if (!string.IsNullOrEmpty(dir))
            {
                if (!dir.EndsWith("\\")) dir += "\\";
            }
            System.Collections.Hashtable h = new System.Collections.Hashtable();
            h.Add(":f:", CleanStr(fileOrFolder, true));
            h.Add(":F:", CleanStr(dir, false));
            h.Add(":n:", CleanStr(name, true));
            //h.Add(":N:", CleanStr(cleanName));
            return ReplaceMeta(s, h);
        }

        private static string ReplaceMeta(string s, System.Collections.Hashtable h) 
        {
            if(string.IsNullOrEmpty(s))
            {
                return s;
            }
            foreach (string k in h.Keys)
            {
                if ((s.IndexOf(k) > 0) && (string.IsNullOrEmpty((string)h[k])))
                {
                    return string.Empty;
                }
                // add remove quotes as needed
                if (!HasSpace((string)h[k]))
                {
                    s = s.Replace("\"" + k + "\"", k);
                }
                else
                {
                    if (s.Equals(k)) s = "\"" + k + "\"";
                }
                s = s.Replace(k, (string)h[k]);
            }
            /*
            if (!string.IsNullOrEmpty(s)) 
            {
                if(string.IsNullOrEmpty(fileOrFolder))
                {
                    if (s.IndexOf(":f:") >= 0) return string.Empty;
                    if (s.IndexOf(":F:") >= 0) return string.Empty;
                    if (s.IndexOf(":n:") >= 0) return string.Empty;
                }

                if (!HasSpace(fileOrFolder))
                {
                    s = s.Replace("\":f:\"", ":f:");
                }
                else
                {
                    if (s.Equals(":f:")) s = "\":f:\"";
                }
                if (!HasSpace(name))
                {
                    s = s.Replace("\":n:\"", ":n:");
                }
                else
                {
                    if (s.Equals(":n:")) s = "\":n:\"";
                }
                if (!HasSpace(dir))
                {
                    s = s.Replace("\":F:\"", ":F:");
                }
                else
                {
                    if (s.Equals(":F:")) s = "\":F:\"";
                }
                s = s.Replace(":f:", fileOrFolder);
                s = s.Replace(":F:", dir);
                s = s.Replace(":n:", name);
            }
            */
            return s;
        }

        public static void MaximizeProcessWin(System.Diagnostics.Process p)
        {
            try
            {
                if (p == null) return;
                System.Threading.ThreadPool.QueueUserWorkItem(
                    new System.Threading.WaitCallback(MaximizeProcessWindows), (object)p);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private static void MaximizeProcessWindows(object o)
        {
            try
            {
                System.Diagnostics.Process p = (System.Diagnostics.Process)o;
                if (p == null) return;
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        //p.WaitForInputIdle(100);
                        IntPtr h = IntPtr.Zero;
                        try
                        {
                            h = p.MainWindowHandle;
                        }
                        catch { }
                        if (h != IntPtr.Zero)
                        {
                            Program.Win32.MaximizeWin(h);
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                        p.Refresh();
                    }
                }
                catch { }
                /*
                if(SimpMain.MainForm != null)
                {
                    p.WaitForExit();
                    Program.Win32.ActivateWin(SimpMain.MainForm.Handle);
                }
                */ 
                p.Close();
                p = null;
            }
            catch { }
        }

        public void KillInstances() 
        {
            try
            {
                if (!IsValid) return;
                if (IsShellPath) return;
                if (!File.Exists(this.path)) return;
                KillInstances(System.IO.Path.GetFileNameWithoutExtension(this.path));
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        public static void KillInstances(string processName)
        {
            try
            {
                if (string.IsNullOrEmpty(processName)) return;
                System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(processName);
                if (p == null) return;
                for (int i = 0; i < p.Length; i++)
                {
                    try
                    {
                        p[i].Kill();
                        p[i].Close();
                        p[i] = null;
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

    }//EOC
}
