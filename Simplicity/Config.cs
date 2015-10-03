using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Simplicity
{
    class Config
    {
        public const string AppVersion = "1.0.0";
        public const string AppNameShort = "Simplicity Home Theater";
        public const string AppUrl = "http://madebits.com";
        public const string AppName = AppNameShort + " " + AppVersion + " - " + AppUrl;
        public static Config Default = new Config();
        public List<AppItem> launchers = new List<AppItem>();
        public System.Collections.Generic.Dictionary<string, string> strings = new System.Collections.Generic.Dictionary<string, string>();
        private bool inited = false;
        public Color foreColor = Color.WhiteSmoke; //SystemColors.ControlText;
        public Color backColor = Color.Black; //SystemColors.Control;
        public Color dirColor = Color.Orange;
        public Color specialColor = Color.FromArgb(0, 162, 232);
        public string fontFamily = "Arial";
        //private bool fullScreen = false;
        public bool canLogErrors = false;
        
        public System.Collections.Hashtable fileFilter = new System.Collections.Hashtable();
        public List<string> favorites = null;

        public int itemsPerPage = 11;
        public bool activateOnClick = true;
        public string initialPath = string.Empty;
        public bool initialPathIsUpFolder = false;
        public Icon mainIcon = null;
        public string starterPath = null;
        bool favoritesInited = false;

        public bool canShowExit = true;
        public bool userFullscreen = false;
        public bool userInitialPathIsUpFolder = false;
        public ulong errorCount = 0;

        private StreamWriter logger = null;
        private volatile bool loggerInited = false;

        public bool IsFullscreen
        {
            get { return userFullscreen; } //return fullScreen || 
        }

        public AppItem lastAppItemUsed = null;
        public AppItem lastAppItemLauncherUsed = null;

        public void KillAppItemInstances() 
        {
            if (launchers == null) return;
            for (int i = 0; i < launchers.Count; i++)
            {
                if (launchers[i] != null)
                {
                    launchers[i].KillInstances();
                }
            }
        }

        public void Load()
        {
            if (inited) return;
            DeleteLog();
            LoadSettings();
            LoadStrings();
            LoadApps();
            inited = true;
        }

        public bool canSaveSettings = true;
        public void SaveSettings()
        {
            if (!canSaveSettings) return;
            try
            {
                string path = GetConfigFilePath("config.txt");
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("color.foreground=" + Color2Str(foreColor));
                    sw.WriteLine("color.background=" + Color2Str(backColor));
                    sw.WriteLine("color.folder=" + Color2Str(dirColor));
                    sw.WriteLine("color.option=" + Color2Str(specialColor));
                    sw.WriteLine("activateOnClick=" + (activateOnClick ? "1" : "0"));
                    sw.WriteLine("itemsPerPage=" + itemsPerPage);
                    sw.WriteLine("fontFamily=" + fontFamily);
                    //sw.WriteLine("fullScreen=" + (fullScreen ? "1" : "0"));
                    //sw.WriteLine("showExitOption=" + (canShowExit ? "1" : "0"));
                    sw.WriteLine("filefilter=" + GetFileFilter());
                    sw.WriteLine("debugLog=" + (canLogErrors ? "1" : "0"));
                    sw.WriteLine("lastPath=" + initialPath);
                    sw.WriteLine("lastPath.isUpFolder=" + (initialPathIsUpFolder ? "1" : "0"));
                    
                    
                }
                SaveStrings();
            }
            catch (Exception ex) { Error(ex); }
        }

        public void Close()
        {
            try
            {
                if(this.logger != null)
                {
                    logger.Close();
                    logger = null;
                }
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                string path = GetConfigFilePath("config.txt");
                if (!File.Exists(path))
                {
                    SaveSettings();
                    return;
                }
                using (StreamReader sw = new StreamReader(path))
                {
                    for (string line = sw.ReadLine(); line != null; line = sw.ReadLine())
                    {
                        try
                        {
                            string[] data = ParseKeyValue(line);
                            if (data == null) continue;
                            string k = data[0];
                            string v = data[1];
                            switch (k.ToLower())
                            {
                                case "color.foreground":
                                    this.foreColor = Str2Color(v, this.foreColor);
                                    break;
                                case "color.background":
                                    this.backColor = Str2Color(v, this.backColor);
                                    break;
                                case "color.folder":
                                    this.dirColor = Str2Color(v, this.dirColor);
                                    break;
                                case "color.option":
                                    this.specialColor = Str2Color(v, this.specialColor);
                                    break;
                                case "activateonclick":
                                    this.activateOnClick = Str2Int(v, activateOnClick ? 1 : 0) == 0 ? false : true;
                                    break;
                                case "itemsperpage":
                                    this.itemsPerPage = Str2Int(v, itemsPerPage);
                                    if (itemsPerPage < 3) itemsPerPage = 3;
                                    if (itemsPerPage > 128) itemsPerPage = 128;
                                    break;
                                case "fontfamily":
                                    this.fontFamily = v;
                                    break;
                                case "filefilter":
                                    InitFileFilter(v);
                                    break;
                                case "lastpath":
                                    try
                                    {
                                        this.initialPath = SControls.BrowserControl.CleanPath(v);
                                    }
                                    catch (Exception ex) { Error(ex); }
                                    break;
                                case "lastpath.isupfolder":
                                    this.initialPathIsUpFolder = Str2Int(v, initialPathIsUpFolder ? 1 : 0) == 0 ? false : true;
                                    break;
                                //case "fullscreen":
                                //    this.fullScreen = Str2Int(v, fullScreen ? 1 : 0) == 0 ? false : true;
                                //    break;
                                case "debuglog":
                                    this.canLogErrors = Str2Int(v, this.canLogErrors ? 1 : 0) == 0 ? false : true;
                                    break;
                                //case "showexitoption":
                                //    this.canShowExit = Str2Int(v, canShowExit ? 1 : 0) == 0 ? false : true;
                                //    break;
                                default:
                                    Error("Not a config option: " + k);
                                    break;
                            }
                        }
                        catch (Exception ex) { Error(ex); }
                    }
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        private static string Color2Str(Color c)
        {
            return c.R + "," + c.G + "," + c.B;
        }

        private Color Str2Color(string s, Color def)
        {
            if (string.IsNullOrEmpty(s)) return def;
            string[] p = s.Split(',');
            if (p.Length < 3) return def;
            try
            {
                Color c = Color.FromArgb(
                    Convert.ToInt32(p[0].Trim()),
                    Convert.ToInt32(p[1].Trim()),
                    Convert.ToInt32(p[2].Trim()));
                return c;
            }
            catch (Exception ex) { Error(ex); }
            return def;
        }

        private int Str2Int(string s, int def)
        {
            if (string.IsNullOrEmpty(s)) return def;
            try
            {
                    return Convert.ToInt32(s);
            }
            catch (Exception ex) { Error(ex); }
            return def;
        }

        public const string StrOpen = "open";
        public const string StrCancel = "cancel";
        public const string StrComputer = "computer";
        public const string StrDesktop = "desktop";
        public const string StrExit = "exit";
        public const string StrExitExit = "exitapp";
        public const string StrExitRestart = "exitrestart";
        //public const string StrExitRestartFullscr = "exitrestartfullsrc";
        public const string StrShutdown = "shutdown";
        public const string StrShutdownRestart = "shutdownrestart";
        public const string StrShutdownPowerOff = "shutdownpoweroff";
        public const string StrShutdownLogOff = "shutdownlogoff";
        public const string StrFree = "free";
        public const string StrPathInfo = "pathinfo";
        public const string StrDelete = "delete";
        public const string StrRename = "rename";
        public const string StrFolders = "folders";
        public const string StrFiles = "files";
        public const string StrSize = "size";
        public const string StrDateModified = "modified";
        public const string StrKillApps = "killcustomapps";
        public const string StrOpenLauncher = "openlauncher";
        public const string StrLaunchers = "launchers";
        public const string StrReloadLaunchers = "reloadlaunchers";
        public const string StrOpenPathDetails = "openpathdetails";
        public const string StrShowInfo = "sysinfo";
        public const string StrExplore = "explore";
        public const string StrWindows = "programwindows";
        public const string StrSound = "sndvol";
        public const string StrSoundUp = "sndvolup";
        public const string StrSoundDown = "sndvoldown";
        public const string StrClipInput = "cliptext";
        public const string StrClipInputNew = "new";
        public const string StrClipInputEdit = "edit";
        public const string StrClipInputCopyPath = "copypath";
        public const string StrClipInputCopyPathName = "copyname";
        public const string StrClipInputExplorePath = "explorepath";
        public const string StrActivate = "activate";
        //public const string StrMaximize = "maximize";
        public const string StrKill = "killapp";
        public const string StrKillAll = "killprocess";
        public const string StrSystem = "system";
        public const string StrAutoStart = "autostart";
        public const string StrAutoStartOn = "turnon";
        public const string StrAutoStartOff = "turnoff";
        public const string StrFavorites = "favorites";
        public const string StrFavoritesAdd = "favoritesadd";
        public const string StrFavoritesDelete = "favoritesdelete";
        public const string StrFavoritesExplore = "favoritesexplore";
        public const string StrFavoritesDeleteNonExisting = "favoritesdeleteanonexisting";
        public const string StrFavoritesDeleteAll = "favoritesdeleteall";

        private void LoadStrings()
        {
            try
            {
                strings.Add(StrOpen, "Open");
                strings.Add(StrCancel, "Cancel");
                strings.Add(StrComputer, "Computer");
                strings.Add(StrDesktop, "Desktop");
                strings.Add(StrExit, "Exit Application");
                strings.Add(StrExitExit, "Exit");
                strings.Add(StrExitRestart, "Restart");
                //strings.Add(StrExitRestartFullscr, "Restart Full Screen");
                strings.Add(StrShutdown, "Shutdown System");
                strings.Add(StrShutdownRestart, "Restart");
                strings.Add(StrShutdownPowerOff, "Shutdown");
                strings.Add(StrShutdownLogOff, "Log Off");
                strings.Add(StrFree, "Free");
                strings.Add(StrPathInfo, "View Path");
                strings.Add(StrDelete, "Delete");
                strings.Add(StrRename, "Rename");
                strings.Add(StrFolders, "Folders");
                strings.Add(StrFiles, "Files");
                strings.Add(StrSize, "Size");
                strings.Add(StrDateModified, "Modified");
                strings.Add(StrKillApps, "Kill Custom Launchers");
                strings.Add(StrReloadLaunchers, "Reload Custom Launchers");
                strings.Add(StrOpenLauncher, "Show Launcher");
                strings.Add(StrLaunchers, "Custom Launchers");
                strings.Add(StrOpenPathDetails, "Show Details");
                strings.Add(StrShowInfo, "Information");
                strings.Add(StrExplore, "Explore");
                strings.Add(StrWindows, "Running Applications");
                //strings.Add(StrSound, "Sound Volume");
                //strings.Add(StrSoundUp, "Volume Up");
                //strings.Add(StrSoundDown, "Volume Down");
                strings.Add(StrClipInput, "Clipboard Text");
                strings.Add(StrClipInputNew, "New");
                strings.Add(StrClipInputEdit, "Edit");
                strings.Add(StrClipInputCopyPath, "Copy Current Path");
                strings.Add(StrClipInputCopyPathName, "Copy Current Path Name");
                strings.Add(StrClipInputExplorePath, "Go To Path");
                strings.Add(StrActivate, "Switch To");
                //strings.Add(StrMaximize, "Maximize");
                strings.Add(StrKill, "Kill");
                strings.Add(StrKillAll, "Kill All");
                strings.Add(StrSystem, "System");
                strings.Add(StrAutoStart, "Auto Start");
                strings.Add(StrAutoStartOn, "Turn On");
                strings.Add(StrAutoStartOff, "Turn Off");

                strings.Add(StrFavorites, "Favorites");
                strings.Add(StrFavoritesAdd, "Add");
                strings.Add(StrFavoritesDelete, "Delete");
                strings.Add(StrFavoritesExplore, "Explore Favorites");
                strings.Add(StrFavoritesDeleteNonExisting, "Delete Non Existing");
                strings.Add(StrFavoritesDeleteAll, "Delete All");  

                string path = GetConfigFilePath("strings.txt");
                if (!File.Exists(path))
                {
                    SaveStrings();
                    return;
                }
                using (StreamReader sw = new StreamReader(path))
                {
                    for (string line = sw.ReadLine(); line != null; line = sw.ReadLine())
                    {
                        try
                        {
                            string[] data = ParseKeyValue(line);
                            if (data == null) continue;
                            string k = data[0];
                            string v = data[1];
                            strings[k.ToLower()] = v.Trim();
                        }
                        catch (Exception ex) { Error(ex); }
                    }
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        private void SaveStrings() 
        {
            try
            {
                string path = GetConfigFilePath("strings.txt");
                using (StreamWriter sw = new StreamWriter(path))
                {
                    foreach (string id in strings.Keys)
                    {
                        sw.WriteLine(id + "=" + strings[id]);
                    }
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        public string Str(string id)
        {
            if (string.IsNullOrEmpty(id) || !strings.ContainsKey(id)) return "?";
            string v = strings[id];
            return v;
        }

        public void LoadApps()
        {
            try
            {
                string path = GetConfigFilePath("launchers.txt");
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = new StreamWriter(path))
                    {
                        sw.WriteLine("#custom launchers");
                        sw.WriteLine(string.Empty);
                        sw.WriteLine(@"app=C:\windows\system32\notepad.exe");
                        sw.WriteLine(@"text=Notepad");
                        sw.WriteLine(@"args="":f:""");
                        sw.WriteLine(@"filter=.txt,.ini,.log");
                    }
                    return;
                }
                this.launchers = null;
                using (StreamReader sw = new StreamReader(path))
                {
                    AppItem app = null;
                    System.Collections.Hashtable variables = new System.Collections.Hashtable();
                    string appDir = Path.GetDirectoryName(ExePath);
                    if(!appDir.EndsWith("\\")) appDir += "\\";
                    variables.Add("$simplicity", appDir);
                    for (string line = sw.ReadLine(); line != null; line = sw.ReadLine())
                    {
                        try
                        {
                            string[] data = ParseKeyValue(line);
                            if (data == null) continue;
                            string k = data[0];
                            string v = data[1];
                            if (k.StartsWith("$"))
                            {
                                variables[k] = v;
                            }
                            else
                            {
                                foreach (string vk in variables.Keys)
                                {
                                    v = v.Replace(vk, (string)variables[vk]);
                                }

                                switch (k.ToLower())
                                {
                                    case "app":
                                        if (app != null)
                                        {
                                            // add previous
                                            app.SetNameFromPath();
                                            if (app.IsValid)
                                            {
                                                if (launchers == null)
                                                {
                                                    launchers = new List<AppItem>();
                                                }
                                                if (!launchers.Contains(app))
                                                {
                                                    app.Name = app.Name;
                                                    launchers.Add(app);
                                                }
                                            }
                                        }
                                        app = new AppItem();
                                        app.Path = v;
                                        break;
                                    case "icon":
                                        if (app != null) { app.IconPath = v; }
                                        break;
                                    case "text":
                                        if (app != null) { app.Name = v; }
                                        break;
                                    case "args":
                                        if (app != null) { app.args = v; }
                                        break;
                                    case "filter":
                                        if (app != null) { app.SetFilter(v); }
                                        break;
                                    default:
                                        Config.Default.Error("? " + k);
                                        break;
                                }
                            }
                        }
                        catch (Exception ex) { Error(ex); }
                    }

                    // add last
                    if (app != null)
                    {
                        app.SetNameFromPath();
                        if (app.IsValid)
                        {
                            if (launchers == null)
                            {
                                launchers = new List<AppItem>();
                            }
                            if (launchers == null)
                            {
                                launchers = new List<AppItem>();
                            }
                            if (!launchers.Contains(app))
                            {
                                launchers.Add(app);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        public static string[] ParseKeyValue(string line) 
        {
            if (string.IsNullOrEmpty(line)) return null;
            line = line.Trim(' ', '\t', '\r', '\n');
            if (string.IsNullOrEmpty(line)) return null;
            if (line.StartsWith("#")) return null;
            if (line.Length <= 1) return null;
            int idx = line.IndexOf('=');
            if (idx <= 0) return null;
            if (line.Length <= idx) return null;
            string k = line.Substring(0, idx).Trim();
            string v = line.Substring(idx + 1).Trim();
            if (string.IsNullOrEmpty(k)) return null;
            if (string.IsNullOrEmpty(v)) return null;
            return new string[] { k, v };
        }

#region favs

        public bool HasFavorites
        {
            get 
            {
                return ((this.favorites != null) && (this.favorites.Count > 0));
            }
        }

        public void LoadFavorites()
        {
            try
            {
                if (favoritesInited) return;
                favoritesInited = true;
                string path = GetConfigFilePath("favorites.txt");
                if (!File.Exists(path))
                {
                    return;
                }
                this.favorites = null;
                using (StreamReader sw = new StreamReader(path))
                {
                    for (string line = sw.ReadLine(); line != null; line = sw.ReadLine())
                    {
                        try
                        {
                            line = line.Trim();
                            if(string.IsNullOrEmpty(line)) continue;
                            if (line.StartsWith("#")) continue;
                            if (this.favorites == null) this.favorites = new List<string>();
                            if (!this.favorites.Contains(line))
                            {
                                this.favorites.Add(line);
                            }
                        }
                        catch { }
                    }
                }
                if(this.favorites != null)
                {
                    this.favorites.Sort(StringComparer.Default);
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        class StringComparer : System.Collections.Generic.IComparer<string>
        {
            public static StringComparer Default = new StringComparer();
            public int Compare(string x, string y)
            {
                return ns.StringLogicalComparer.Compare(x, y);
            }
        }

        private void SaveFavorites()
        {
            try
            {
                string path = GetConfigFilePath("favorites.txt");
                try
                {
                    string pathbackup = path + ".bak";
                    if (File.Exists(path))
                    {
                        if (File.Exists(pathbackup))
                        {
                            File.Delete(pathbackup);
                        }
                        File.Move(path, pathbackup);
                    }
                }
                catch (Exception ex) { Error(ex); }
                using (StreamWriter sw = new StreamWriter(path))
                {
                    if (this.favorites == null) return;
                    for (int i = 0; i < favorites.Count; i++ )
                    {
                        sw.WriteLine(favorites[i]);
                    }
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        public void AddFavorite(string f)
        {
            if (string.IsNullOrEmpty(f)) return;
            if (this.favorites == null) this.favorites = new List<string>();
            if (!this.favorites.Contains(f))
            {
                this.favorites.Add(f);
                this.favorites.Sort(StringComparer.Default);
                SaveFavorites();
            }
        }

        public void DeleteFavorite(int idx)
        {
            try
            {
                if (!HasFavorites) return;
                if (idx < 0) return;
                if (idx >= this.favorites.Count) return;
                this.favorites.RemoveAt(idx);
                SaveFavorites();
            }
            catch (Exception ex) { Error(ex); }
        }

        public void DeleteNonExistingFavorites()
        {
            try
            {
                if (!HasFavorites) return;
                List<string> newFavs = new List<string>();
                for(int i = 0; i < this.favorites.Count; i++)
                {
                    bool fileExists = false;
                    bool dirExists = false;
                    try
                    {
                        fileExists = File.Exists(favorites[i]);
                    }
                    catch (Exception ex) { Error(ex); }
                    if(!fileExists)
                    {
                        try
                        {
                            dirExists = Directory.Exists(favorites[i]);
                        }
                        catch (Exception ex) { Error(ex); }
                    }
                    if(fileExists || dirExists)
                    {
                        newFavs.Add(favorites[i]);
                    }
                }
                if(newFavs.Count == favorites.Count)
                {
                    return;
                }
                favorites = newFavs;
                SaveFavorites();
            }
            catch (Exception ex) { Error(ex); }
        }

        public void DeleteFavorites()
        {
            try
            {
                if (!HasFavorites) return;
                this.favorites = null;
                SaveFavorites();
            }
            catch (Exception ex) { Error(ex); }
        }

#endregion favs

        public string ConfigFolder
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, AppNameShort);
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        private string GetConfigFilePath(string file) 
        {
            string dir = Path.GetDirectoryName(ExePath);
            string path = Path.Combine(dir, file);
            if (File.Exists(path)) return path;
            path = Path.Combine(ConfigFolder, file);
            return path;
        }

        public void Error(string s)
        {
            errorCount++;
            if (s == null) s = "<null>";
            Log("Error: " + s);
        }

        public void Error(Exception ex)
        {
            string e = null;
            if(ex != null)
            {
                e = ex.Message;
#if DEBUG
                e += " " + ex.StackTrace;
                //if (System.Diagnostics.Debugger.IsAttached) 
                //{
                //    System.Diagnostics.Debugger.Break();
                //}
#endif
            }
            Error(e);
        }

        public void Log(string s)
        {
            if (!canLogErrors) return;
            if (s == null) s = "<null>";
            System.Diagnostics.Debug.WriteLine(s);
            LogStr(s);
        }

        private void LogStr(string s)
        {
            if (!canLogErrors) return;
            try
            {
                if (!loggerInited)
                {
                    loggerInited = true;
                    try
                    {
                        string logFile = GetConfigFilePath("debuglog.txt");
                        if (this.logger == null)
                        {
                            this.logger = new StreamWriter(logFile);
                        }
                    }
                    catch { }
                }
                if (this.logger != null)
                {
                    if (s == null) s = "<null>";
                    this.logger.WriteLine(s);
                    this.logger.Flush();
                }
            }
            catch { }
        }

        private void DeleteLog()
        {
            try
            {
                string logFile = GetConfigFilePath("debuglog.txt");
                if (File.Exists(logFile)) File.Delete(logFile);
                using(Stream s = File.Create(logFile)){}
            }
            catch { }
        }

        public static string SizeStrNoSpace(long size)
        {
            return SizeStr(size, "{0:0}").Replace(" ", string.Empty);
        }

        public static string SizeStr(long size)
        {
            return SizeStr(size, null);
        }

        public static string SizeStr(long size, string format)
        {
            return SizeStr((ulong)size, format);
        }

        public static string SizeStr(ulong size, string format)
        {
            if (string.IsNullOrEmpty(format)) format = "{0:0.0}";
            if (size < 0) return string.Empty;
            double kb = (double)size / 1024.0;
            double mb = kb / 1024.0;
            double gb = mb / 1024.0;
            if (gb >= 1.0) return TrimZeroesString(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, gb)) + " GB";
            if (mb >= 1.0) return TrimZeroesString(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, mb)) + " MB";
            if (kb >= 1.0) return TrimZeroesString(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, kb)) + " KB";
            return ((size > 0) ? "1 KB" : "0 KB");
        }

        private static string TrimZeroesString(string r)
        {
            if (r.IndexOf('.') > 0)
            {
                r = r.TrimEnd('.', '0');
            }
            return r;
        }

        public static bool IsDialogCloseKey(Keys k) 
        {
            if ((k == Keys.Escape)
                    || (k == Keys.Back)
                    || (k == Keys.MediaStop)
                )
            {
                return true;
            }
            return false;
        }

        public bool FromStarter
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.starterPath) && File.Exists(starterPath) && !Directory.Exists(starterPath))
                    {
                        return true;
                    }
                }
                catch { }
                return false;
            }
        }

        public string ExePath
        {
            get 
            {
                if (FromStarter)
                {
                    return starterPath;
                }
                return Application.ExecutablePath;
            }
        }

        public void Restart(bool fullScreen, string path)
        {
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
                string args = Program.pidPrefix + p.Id.ToString();
                p.Close();
                p = null;
                if (fullScreen) args += (" " + Program.fullScreenPrefix);
                if(!string.IsNullOrEmpty(path))
                {
                    args += (" " + Program.userSelPrefix);
                    args += (" \"" + path + "\"");
                }
                this.SaveSettings();
                this.Close();
                canSaveSettings = false;
                System.Diagnostics.Process.Start(ExePath, args);
            }
            catch (Exception ex) { Error(ex); }
        }

        public bool CanProcessFile(string file)
        {
            if ((fileFilter == null) || (fileFilter.Keys.Count <= 0)) return true;
            string f = System.IO.Path.GetExtension(file).ToLower().Trim('.', ' ');
            return (fileFilter[f] != null);
        }
        /*
        public bool CanProcessFolder(string dir)
        {
            if ((fileFilter == null) || (fileFilter.Keys.Count <= 0)) return true;
            bool hasDirs = false;
            bool hasFiles = false;
            string[] files = null;
            try
            {
                files = Directory.GetDirectories(dir);
                if ((files != null) && (files.Length >= 0))
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            string file = files[i];
                            if (SControls.BrowserControl.IsSystemHidden(file))
                            {
                                continue;
                            }
                            hasDirs = true;
                            break;
                        }
                        catch (Exception ex) { Config.Default.Error(ex); }
                    }
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            try
            {
                files = Directory.GetFiles(dir);
                if ((files != null) && (files.Length >= 0))
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            string file = files[i];
                            if (SControls.BrowserControl.IsSystemHidden(file))
                            {
                                continue;
                            }
                            if (!Config.Default.CanProcessFile(file))
                            {
                                continue;
                            }
                            hasFiles = true;
                            break;
                        }
                        catch (Exception ex) { Config.Default.Error(ex); }
                    }
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return hasDirs || hasFiles;
        }
        */
        private void InitFileFilter(string suffixes)
        {
            if (string.IsNullOrEmpty(suffixes)) return;
            if (this.fileFilter == null) fileFilter = new System.Collections.Hashtable();
            string[] p = suffixes.Split(',');
            if (p == null) return;
            for(int i = 0; i < p.Length; i++)
            {
                string s = p[i];
                s = s.Replace("*", string.Empty);
                s = s.Replace("?", string.Empty);
                s = s.Trim(',', ' ', '\t', '.');
                if(string.IsNullOrEmpty(s)) continue;
                fileFilter[s.ToLower()] = string.Empty;
            }
        }

        private string GetFileFilter()
        {
            if (this.fileFilter == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach(string k in this.fileFilter.Keys)
            {
                if (sb.Length > 0) sb.Append(',');
                sb.Append(k);
            }
            return sb.ToString();
        }

    }//EOC
}
