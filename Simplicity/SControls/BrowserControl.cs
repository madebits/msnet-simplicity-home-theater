using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class BrowserControl : UserControl
    {
        private bool inited = false;
        private SGraphics graphics = new SGraphics();
        private string baseLocation = string.Empty; // os path, null or empty means root
        //private string previousBaseLocation = string.Empty;
        private bool showNextButton = false;

        public BrowserControl()
        {
            InitializeComponent();
            LoadColors();
            this.list.ActivateOnSpaceKey = true;
            this.list.PageItemCount = Config.Default.itemsPerPage - 1;
            this.list.ActivateItemOnClick = Config.Default.activateOnClick;
            this.sbtnLocation.MouseWheel += new MouseEventHandler(sbtnLocation_MouseWheel);
            this.Disposed += new System.EventHandler(BrowserControl_Disposed);
            this.list.ItemActivated += new SList.ItemActivatedHandler(BrowserControl_ItemActivated);
            this.list.ItemSelected += new SList.ItemSelectedHandler(BrowserControl_ItemSelected);
            this.list.ItemKeyDown += new KeyEventHandler(list_ItemKeyDown);
            inited = true;
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
            this.Invalidate();
        }

        private void BrowserControl_Load(object sender, EventArgs e)
        {
            LoadColors();
            //ResizeControls();
        }

        private void BrowserControl_Resize(object sender, EventArgs e)
        {
            ResizeControls();
        }

        private int statusHeight = 10;

        private void ResizeControls()
        {
            try
            {
                this.SuspendLayout();
                Size s = this.Size;
                int height = s.Height / (this.list.PageItemCount + 1);
                if (height < 10) height = 10;
                //if (height > 128) height = 128;
                statusHeight = height;
                this.list.Size = new Size(this.Width, this.Height - height);
                this.sbtnLocation.Size = new Size(this.Width, height);
                this.list.Location = new Point(0, 0);
                this.sbtnLocation.Location = new Point(0, this.Height - height);
            }
            finally
            {
                this.ResumeLayout();
            }
            this.Invalidate();
            this.Refresh();
        }

        private void BrowserControl_Disposed(object sender, System.EventArgs e)
        {
            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }
        }

        private void RawSetBaseLocation(string newBaseLocation)
        {
            //previousBaseLocation = baseLocation;
            baseLocation = newBaseLocation;
        }

        private void BrowserControl_ItemActivated(int itemIndex, bool isContextMenu)
        {
            FItemData it = this.list.GetItemByIndex(itemIndex) as FItemData;
            if (it == null) return;
            if (IsRoot && it.IsSpecial)
            {
                this.ShowPathList(-1, true);
                return;
            }

            if (isContextMenu)
            {
                /*
                if (it.IsUpFolder)
                {
                    return;
                }
                */
                this.ProcessItemActivation(itemIndex, isContextMenu);
                return;
            }
            string newLocation = null;
            if (it.IsFolder || it.IsUpFolder)
            {
                if (it.IsUpFolder)
                {
                    if (itemIndex != 0)
                    {
                        newLocation = NextDir(baseLocation);
                        if(string.IsNullOrEmpty(newLocation))
                        {
                            newLocation = Path.GetDirectoryName(baseLocation);
                        }
                    }
                    else
                    {
                        newLocation = Path.GetDirectoryName(baseLocation);
                    }
                }
                else
                {
                    string path = GetItemText(it);
                    if (IsRoot)
                    {
                        newLocation = path;
                    }
                    else
                    {
                        newLocation = Path.Combine(baseLocation, path);
                    }
                }
                try
                {
                    this.SuspendLayout();
                    string startFolderItemText = null;
                    if (it.IsUpFolder && (itemIndex == 0))
                    {
                        try
                        {
                            startFolderItemText = GetFileName(baseLocation);
                        }
                        catch { }
                    }
                    this.SetBaseLocation(newLocation, startFolderItemText);
                }
                finally
                {
                    this.ResumeLayout();
                }
            }
            else
            {
                this.ProcessItemActivation(itemIndex, isContextMenu);
            }
        }

        private static string GetItemText(FItemData it)
        {
            if (it == null) return null;
            if (it.Text == null) return null;
            string p = it.Text;
            if (p.StartsWith("..")) return "..";
            if(p.IndexOf(':') == 1)
            {
                return p.Substring(0, 2) + "\\";
            }
            return p;
        }

        public bool IsUpFolderItem
        {
            get
            {
                FItemData it = this.list.GetItemByIndex(this.list.CurrentItemIndex) as FItemData;
                if (it == null) return false;
                return it.IsUpFolder;
            }
        }

        private delegate void DProcessItemActivation(int itemIndex, bool isCtxMenu);

        private void ProcessItemActivation(int itemIndex, bool isContextMenu)
        {
            try
            {
                FItemData it = this.list.GetItemByIndex(itemIndex) as FItemData;
                if (it == null) return;
                string path = GetItemPath(it);
                DItemActivated da = new DItemActivated();
                SetChildFormSize(da);
                if (da.ShowOpen(this, path, it.IsFolder, it.IsUpFolder) == DialogResult.Cancel)
                {
                    return;
                }
                switch (da.Action)
                {
                    case DItemActivated.SpecialAction.ExploreDir:
                        if (it.IsUpFolder)
                        {
                            path = Path.GetDirectoryName(path);
                        }
                        this.SetBaseLocation(path, null);
                        break;
                    case DItemActivated.SpecialAction.ShowPath:
                        //ShowPathList(itemIndex, false);
                        this.BeginInvoke(new DShowPathList(ShowPathList), new object[] { itemIndex, false });
                        break;
                    default:
                        da.RunPath();
                        break;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            finally
            {
                this.Invoke(new MethodInvoker(FocusList));
            }
        }

        private void FocusList()
        {
            try
            {
                this.Focus();
                this.list.Focus();
            }
            catch { }
        }

        private void BrowserControl_ItemSelected(int itemIndex)
        {
            this.sbtnLocation.Text = "> " + GetCurrentPath();
            this.sbtnLocation.Invalidate();
            this.sbtnLocation.Refresh();
        }

        private string GetCurrentPath()
        {
            if (!this.list.HasData) return string.Empty;
            FItemData it = this.list.GetItemByIndex(this.list.CurrentItemIndex) as FItemData;
            return GetItemPath(it);
        }

        private string GetItemPath(FItemData it)
        {
            if (it == null) return null;
            if (IsRoot && it.IsSpecial)
            {
                return null;
            }
            string newLocation = null;
            if (it.IsUpFolder)
            {
                newLocation = baseLocation;
            }
            else
            {
                string path = GetItemText(it);
                if (IsRoot)
                {
                    newLocation = path;
                }
                else
                {
                    newLocation = Path.Combine(baseLocation, path);
                }
            }
            return CleanPath(newLocation);
        }

        public string BrowseLocation
        {
            get
            {
                return GetCurrentPath();
            }
            private set
            {
                this.SetBaseLocation(value, null);
            }
        }

        public string BaseLocation
        {
            get { return baseLocation; }
        }

        public void GoTo(string location, string startDirText)
        {
            if (!string.IsNullOrEmpty(location))
            {
                if (!(File.Exists(location) || Directory.Exists(location)))
                {
                    location = string.Empty;
                }
            }
            this.SetBaseLocation(location, startDirText);
        }

        public bool IsRoot
        {
            get { return string.IsNullOrEmpty(baseLocation); }
        }

        private static bool IsFile(string ospath)
        {
            try
            {
                return (File.Exists(ospath) && !Directory.Exists(ospath));
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return false;
        }

        public static bool IsSystemHidden(string file)
        {
            return IsSystemHidden(File.GetAttributes(file));
        }

        private static bool IsSystemHidden(FileAttributes fa)
        {
            if ((fa & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return true;
            }
            if ((fa & FileAttributes.System) == FileAttributes.System)
            {
                return true;
            }
            return false;
        }

        public static string GetFileName(string upFolderName)
        {
            if (!string.IsNullOrEmpty(upFolderName))
            {
                if (upFolderName.Length > 3)
                {
                    upFolderName = upFolderName.Trim('/', '\\');
                }
                int idx = upFolderName.LastIndexOf('\\');
                if (upFolderName.Length > 3)
                {
                    upFolderName = upFolderName.Substring(idx + 1);
                }
            }
            return upFolderName;
        }

        private void SetBaseLocation(string f, string startFolderItemText)
        {
            try
            {
                if (!inited) return;
                Application.DoEvents();
                if (this.list.ImageResolver == null)
                {
                    this.list.ImageResolver = new FItemImageResolver(new FItemImageResolver.DItemPathResolver(GetItemPath));
                }
                string path = CleanPath(f);
                string baseDir = f;
                string baseFile = string.Empty;
                int fileStartIndex = -1;

                List<SList.ItemData> data = new List<SList.ItemData>();
                if (string.IsNullOrEmpty(path)) // root
                {
                    FItemImageResolver fir = this.list.ImageResolver as FItemImageResolver;
                    if (fir != null)
                    {
                        fir.ClearFileBitmaps();
                    }
                    RawSetBaseLocation(null);
                    string[] drives = System.Environment.GetLogicalDrives();
                    for (int i = 0; i < drives.Length; i++)
                    {
                        string t = drives[i];
                        try
                        {
                            DriveInfo di = new DriveInfo(t);
                            if((di != null) && (di.IsReady))
                            {
                                string l = di.VolumeLabel;
                                if(!string.IsNullOrEmpty(l))
                                {
                                    t += (" [" + l + "]");
                                }
                                try
                                {
                                    DriveType dt = di.DriveType;
                                    if ((dt == DriveType.Fixed) || (dt == DriveType.Removable) || (dt == DriveType.Ram))
                                    {
                                        t += (" " + Config.SizeStrNoSpace(di.TotalSize) + " > "
                                            + Config.SizeStrNoSpace(di.TotalFreeSpace) + " " + Config.Default.Str(Config.StrFree));
                                    }
                                }
                                catch (Exception ex) { Config.Default.Error(ex); }
                             }
                        }
                        catch (Exception ex) { Config.Default.Error(ex); }
                        FItemData it = new FItemData(t);
                        it.IsFolder = true;
                        data.Add(it);
                    }
                    FItemData it1 = new FItemData("..."); //Config.Default.Str(Config.StrExit));
                    it1.IsSpecial = true;
                    it1.IsFolder = true;
                    data.Add(it1);
                }
                else
                {
                    if (IsFile(path))
                    {
                        baseDir = Path.GetDirectoryName(f);
                        baseFile = Path.GetFileName(f);
                    }

                    string upFolderName = null;
                    try
                    {
                        upFolderName = Path.GetDirectoryName(baseDir);
                        if (!string.IsNullOrEmpty(upFolderName))
                        {
                            upFolderName = GetFileName(upFolderName);
                        }
                        else
                        {
                            upFolderName = Config.Default.Str(Config.StrComputer);
                        }
                        if (!string.IsNullOrEmpty(upFolderName))
                        {
                            string bName = GetFileName(baseDir);
                            upFolderName = upFolderName + " < " + bName;
                        }
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    FItemData uit = new FItemData(".." + ((upFolderName != null) ? " [" + upFolderName + "]" : string.Empty));
                    uit.IsUpFolder = true;
                    uit.IsFolder = true;
                    uit.IsSpecial = true;
                    data.Add(uit);
                    
                    string[] files = null;

                    try
                    {
                        files = Directory.GetDirectories(baseDir);
                        if ((files != null) && (files.Length >= 0))
                        {
                            Array.Sort(files, ns.StringLogicalComparer.Default);
                            for (int i = 0; i < files.Length; i++)
                            {
                                try
                                {
                                    string file = files[i];
                                    if (IsSystemHidden(file))
                                    {
                                        continue;
                                    }
                                    /*
                                    if(!Config.Default.CanProcessFolder(file))
                                    {
                                        continue;
                                    }*/
                                    FItemData it = new FItemData(Path.GetFileName(file));
                                    it.IsUpFolder = false;
                                    it.IsFolder = true;
                                    data.Add(it);
                                }
                                catch (Exception ex) { Config.Default.Error(ex); }
                            }
                        }
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    try
                    {
                        files = Directory.GetFiles(baseDir);
                        if ((files != null) && (files.Length >= 0))
                        {
                            fileStartIndex = data.Count;
                            Array.Sort(files, ns.StringLogicalComparer.Default);
                            for (int i = 0; i < files.Length; i++)
                            {
                                try
                                {
                                    string file = files[i];
                                    if (IsSystemHidden(file))
                                    {
                                        continue;
                                    }
                                    if (!Config.Default.CanProcessFile(file))
                                    {
                                        continue;
                                    }
                                    FItemData it = new FItemData(Path.GetFileName(file));
                                    it.IsUpFolder = false;
                                    it.IsFolder = false;
                                    data.Add(it);
                                }
                                catch (Exception ex) { Config.Default.Error(ex); }
                            }
                        }
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    if (showNextButton)
                    {
                        if (data.Count > 1)
                        {
                            uit = new FItemData(">");
                            uit.IsUpFolder = true;
                            uit.IsFolder = true;
                            uit.IsSpecial = true;
                            data.Add(uit);
                        }
                    }
                    // set after to be sure it worked
                    RawSetBaseLocation(baseDir);
                }

                int startIndex = 0;
                if ((fileStartIndex > 0) && !string.IsNullOrEmpty(baseFile))
                {
                    startIndex = GetNamedItemIndex(data, baseFile, fileStartIndex);
                }
                else if(!string.IsNullOrEmpty(startFolderItemText))
                {
                    startIndex = GetNamedItemIndex(data, startFolderItemText, 0);
                }
                this.list.SetItems(data, startIndex);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private static int GetNamedItemIndex(List<SList.ItemData> data, string baseFile, int fileStartIndex)
        {
            if ((data == null) || (data.Count <= 0)) return -1;
            if (string.IsNullOrEmpty(baseFile)) return -1;
            for (int i = fileStartIndex; i < data.Count; i++)
            {
                if (GetItemText(data[i] as FItemData).Equals(baseFile, StringComparison.CurrentCultureIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string CleanPath(string f)
        {
            if (string.IsNullOrEmpty(f)) return f;
            if (f.StartsWith("\\") || f.StartsWith("/")) return null;
            f = f.Replace("/", "\\");
            f = f.Replace("\\\\", "\\");
            f = f.TrimEnd('\\');
            if (f.EndsWith(":")) f += "\\";
            return f;
        }

        public static string DesktopPath
        {
            get
            {
                try
                {
                    string dp = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    return CleanPath(dp);
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                return null;
            }
        }

        private void sbtnLocation_Click(object sender, EventArgs e)
        {
            FItemData it = this.list.GetItemByIndex(this.list.CurrentItemIndex) as FItemData;
            if (it == null) return;
            bool special = it.IsSpecial && IsRoot;
            ShowPathList(special ? -1 : this.list.CurrentItemIndex, special);
        }

        private void ShowPathList(bool focusExit)
        {
            ShowPathList(this.list.CurrentItemIndex, focusExit);
        }

        private delegate void DShowPathList(int itemIndex, bool focusExit);

        private void ShowPathList(int itemIndex, bool focusExit)
        {
            try
            {
                FItemData it = null;
                string path = null;
                string opath = null;
                if (itemIndex >= 0)
                {
                    it = this.list.GetItemByIndex(itemIndex) as FItemData;
                    if (it == null) return;
                    path = this.GetItemPath(it);
                    opath = path;
                    if (!it.IsFolder && !it.IsUpFolder)
                    {
                        path = Path.GetDirectoryName(path);
                    }
                }
                DPathDetails dp = new DPathDetails();
                SetChildFormSize(dp);
                if (dp.ShowPath(this, this.IsRoot, it, path, focusExit) == DialogResult.Cancel)
                {
                    return;
                }
                DInfo di = null;
                DConfirm dc = null;
                switch (dp.Action)
                {
                    case DPathDetails.SpecialAction.Info:
                        if (itemIndex >= 0)
                        {
                            di = new DInfo();
                            SetChildFormSize(di);
                            di.ShowInfo(this, opath, this.statusHeight, this.sbtnLocation.sg.font);
                            di = null;
                        }
                        break;
                    case DPathDetails.SpecialAction.OpenLauncher:
                        //ProcessItemActivation(itemIndex, false);
                        this.BeginInvoke(new DProcessItemActivation(ProcessItemActivation), new object[]{ itemIndex, false});
                        break;
                    case DPathDetails.SpecialAction.Delete:
                        if(it.IsUpFolder || it.IsSpecial || IsRoot)
                        {
                        }
                        else
                        {
                            string deleteText = Config.Default.Str(Config.StrDelete);
                            if (it.IsFolder)
                            {
                                deleteText += " <" + GetFileName(opath) + ">";
                            }
                            else
                            {
                                deleteText += " [" + GetFileName(opath) + "]";
                            }
                            dc = new DConfirm();
                            SetChildFormSize(dc);
                            if (dc.ShowConfirm(this, new string[]{deleteText}, true) == DialogResult.Cancel)
                            {
                                return;
                            }
                            this.DeleteItem(itemIndex);
                            dc = null;
                        }
                        break;
                    case DPathDetails.SpecialAction.Rename:
                        if (it.IsUpFolder || it.IsSpecial || IsRoot)
                        {
                        }
                        else
                        {
                            DInput dip = new DInput();
                            SetChildFormSize(dip);
                            string fname = opath;
                            if (dip.ShowEditInput(this, fname, this.statusHeight, this.sbtnLocation.sg.font) == DialogResult.Cancel) 
                            {
                                return;
                            }
                            this.RenameItem(itemIndex, dip.UserText);
                            dip = null;
                        }
                        break;
                    case DPathDetails.SpecialAction.Favorites:
                        ProcessFavorites(itemIndex, it, opath);
                        break;
                    case DPathDetails.SpecialAction.SystemMenu:
                        ProcessSystem(itemIndex, it, opath);
                        break;
                    case DPathDetails.SpecialAction.Exit:
                        dc = new DConfirm();
                        SetChildFormSize(dc);
                        string[] eops = new string[]
                        {
                            Config.Default.Str(Config.StrExitExit),
                            Config.Default.Str(Config.StrExitRestart),
                            //Config.Default.userFullscreen ? null : Config.Default.Str(Config.StrExitRestartFullscr),
                        };
                        if(dc.ShowConfirm(this, eops, false) == DialogResult.Cancel)
                        {
                            return;
                        }
                        switch(dc.SelectedItemIndex)
                        {
                            case 0:
                                Application.Exit();
                                return;
                            case 1:
                                Config.Default.Restart(Config.Default.userFullscreen, this.BrowseLocation);
                                break;
                            case 2:
                                Config.Default.Restart(true, this.BrowseLocation);
                                break;
                        }
                        break;
                    default:
                        this.SetBaseLocation(dp.Path, null);
                        break;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            finally
            {
                this.Invoke(new MethodInvoker(FocusList));
            }
        }

        public void ProcessFavorites(int itemIndex, FItemData it, string path)
        {
            Config.Default.LoadFavorites();
            DConfirm dc = null;
            DConfirm sdc = new DConfirm();
            SetChildFormSize(sdc);
            bool showAdd = ((itemIndex >= 0) && !string.IsNullOrEmpty(path));
            /*
            if (showAdd)
            {
                if (Config.Default.HasFavorites 
                    && Config.Default.favorites.Contains(path))
                {
                    showAdd = false;
                }
            }
            */
            string addText = Config.Default.Str(Config.StrFavoritesAdd);
            if (showAdd)
            {
                addText += " [" + GetFileName(path) + "]";
            }
            if (sdc.ShowConfirm(this, new string[] 
                {
                Config.Default.HasFavorites ? Config.Default.Str(Config.StrFavoritesExplore) : null,
                (showAdd ? addText : null),
                Config.Default.HasFavorites ? Config.Default.Str(Config.StrFavoritesDelete) : null,
                Config.Default.HasFavorites ? Config.Default.Str(Config.StrFavoritesDeleteNonExisting) : null,
                Config.Default.HasFavorites ? Config.Default.Str(Config.StrFavoritesDeleteAll) : null,
                }, false, true, null) == DialogResult.Cancel)
            {
                return;
            }
            if (sdc.SelectedItemText == Config.Default.Str(Config.StrFavoritesExplore)) 
            {
                if (!Config.Default.HasFavorites) return;
                dc = new DConfirm();
                dc.checkItemType = true;
                SetChildFormSize(dc);
                if (dc.ShowConfirm(this, Config.Default.favorites.ToArray(), true, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                this.SetBaseLocation(dc.SelectedItemText, null);
                dc = null;
            }
            else if (sdc.SelectedItemText == addText)
            {
                Config.Default.AddFavorite(path);
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrFavoritesDelete))
            {
                if (!Config.Default.HasFavorites) return;
                dc = new DConfirm();
                dc.checkItemType = true;
                SetChildFormSize(dc);
                if (dc.ShowConfirm(this, Config.Default.favorites.ToArray(), true, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                Config.Default.DeleteFavorite(dc.SelectedItemIndex);
                dc = null;
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrFavoritesDeleteNonExisting))
            {
                if (!Config.Default.HasFavorites) return;
                Config.Default.DeleteNonExistingFavorites();
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrFavoritesDeleteAll))
            {
                if (!Config.Default.HasFavorites) return;
                Config.Default.DeleteFavorites();
            }
        }

        public void ProcessSystem(int itemIndex, FItemData it, string path)
        {
            DConfirm dc = null;
            DConfirm sdc = new DConfirm();
            SetChildFormSize(sdc);
            if (sdc.ShowConfirm(this, new string[] 
                {
                Config.Default.Str(Config.StrShutdown),
                Config.Default.Str(Config.StrWindows),
                Config.Default.Str(Config.StrLaunchers),
                Config.Default.Str(Config.StrClipInput),
                Config.Default.Str(Config.StrAutoStart),
                Config.Default.Str(Config.StrShowInfo),
                }, false, true, null) == DialogResult.Cancel)
            {
                return;
            }
            if (sdc.SelectedItemText == Config.Default.Str(Config.StrLaunchers)) 
            {
                DItemActivated da = new DItemActivated();
                SetChildFormSize(da);
                if(da.ShowLaunchers(this) == DialogResult.Cancel)
                {
                    return;
                }
                da.RunPath(true);
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrAutoStart))
            {
                //bool on = AutoStart.CurrentState.Equals(AutoStart.AutoStartState.OnMax);
                bool on = AutoStart.ShellCurrentState.Equals(AutoStart.ShellStartState.On);
                string[] asops = new string[]{ on ? Config.Default.Str(Config.StrAutoStartOff) : Config.Default.Str(Config.StrAutoStartOn) };
                dc = new DConfirm();
                SetChildFormSize(dc);
                if (dc.ShowConfirm(this, asops, true, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                on = !on;
                //AutoStart.CurrentState = (on ? AutoStart.AutoStartState.OnMax : AutoStart.AutoStartState.Off);
                AutoStart.ShellCurrentState = (on ? AutoStart.ShellStartState.On : AutoStart.ShellStartState.Off);
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrShowInfo))
            {
                /*
                di = new DInfo();
                SetChildFormSize(di);
                di.ShowTime(this, this.statusHeight, this.sbtnLocation.sg.font);
                di = null;
                */
                dc = new DConfirm();
                SetChildFormSize(dc);
                if (dc.ShowConfirm(this, SysInfo.Default.GetInfo(), true, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                SysInfo.Default.HandleSelect(dc.SelectedItemIndex);
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrWindows))
            {
                DWins dw = new DWins();
                SetChildFormSize(dw);
                if (dw.ShowList(this) == DialogResult.Cancel)
                {
                    return;
                }
                string name = " [" + dw.WinName + "]";
                string pname = " [" + dw.ProcessName + "]";
                dc = new DConfirm();
                SetChildFormSize(dc);
                if (dc.ShowConfirm(this, new string[] { 
                        Config.Default.Str(Config.StrActivate) + name,
                        Config.Default.Str(Config.StrKill) + name,
                        Config.Default.Str(Config.StrKillAll) + pname
                    }, false, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                switch (dc.SelectedItemIndex)
                {
                    case 0:
                        dw.ActivateApp(DWins.ActivateAction.Maximize);
                        break;
                    case 1:
                        dw.ActivateApp(DWins.ActivateAction.Kill);
                        break;
                    case 2:
                        dw.ActivateApp(DWins.ActivateAction.KillAll);
                        break;
                }
                dc = null;
                dw = null;
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrClipInput))
            {
                dc = new DConfirm();
                SetChildFormSize(dc);
                string[] options = new string[]{
                        Config.Default.Str(Config.StrClipInputEdit),
                        Config.Default.Str(Config.StrClipInputNew),
                        ((itemIndex >= 0) ? Config.Default.Str(Config.StrClipInputCopyPath) : null),
                         ((itemIndex >= 0) ? Config.Default.Str(Config.StrClipInputCopyPathName) : null),
                        Config.Default.Str(Config.StrClipInputExplorePath),
                        };
                if (dc.ShowConfirm(this, options, false, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                bool edit = true;
                string so = dc.SelectedItemText;
                if (so == Config.Default.Str(Config.StrClipInputEdit))
                {
                    edit = true;
                }
                else if (so == Config.Default.Str(Config.StrClipInputNew))
                {
                    edit = false;
                }
                else if (so == Config.Default.Str(Config.StrClipInputCopyPath))
                {
                    edit = true;
                    try
                    {
                        string t = path;
                        if (string.IsNullOrEmpty(t)) t = string.Empty;
                        Clipboard.SetText(t);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
                else if (so == Config.Default.Str(Config.StrClipInputCopyPathName))
                {
                    edit = true;
                    try
                    {
                        string t = path;
                        if (string.IsNullOrEmpty(t))
                        {
                            t = string.Empty;
                        }
                        else
                        {
                            if(!IsRoot)
                            {
                                t = GetFileName(t);
                            }
                        }
                        Clipboard.SetText(t);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
                else if (so == Config.Default.Str(Config.StrClipInputExplorePath))
                {
                    edit = true;
                    string tp = string.Empty;
                    try
                    {
                        tp = Clipboard.GetText();
                        tp = BrowserControl.CleanPath(tp);
                        if (string.IsNullOrEmpty(tp)) tp = string.Empty;
                        Clipboard.SetText(tp);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
                DInput dci = new DInput();
                SetChildFormSize(dci);
                dci.ShowInput(this, this.statusHeight, this.sbtnLocation.sg.font, edit);
                dc = null;
                dci = null;
                if (so == Config.Default.Str(Config.StrClipInputExplorePath))
                {
                    string tp = string.Empty;
                    try
                    {
                        tp = Clipboard.GetText();
                        if (string.IsNullOrEmpty(tp)) tp = string.Empty;
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    this.GoTo(tp, null);
                }
            }
            else if (sdc.SelectedItemText == Config.Default.Str(Config.StrShutdown))
            {
                dc = new DConfirm();
                SetChildFormSize(dc);
                if (dc.ShowConfirm(this,
                    new string[]{
                            Config.Default.Str(Config.StrShutdownPowerOff),
                            Config.Default.Str(Config.StrShutdownLogOff),
                            Config.Default.Str(Config.StrShutdownRestart),
                        }, false, true, null) == DialogResult.Cancel)
                {
                    return;
                }
                switch (dc.SelectedItemIndex)
                {
                    case 0:
                        cm.ExitWindows.Shutdown(cm.ExitWindows.ShutdownMethod.ShutDown);
                        break;
                    case 1:
                        cm.ExitWindows.Shutdown(cm.ExitWindows.ShutdownMethod.LogOff);
                        break;
                    case 2:
                        cm.ExitWindows.Shutdown(cm.ExitWindows.ShutdownMethod.Reboot);
                        break;
                }
                Application.Exit();
                return;
            }
            sdc = null;
            dc = null;
        }

        private void SetChildFormSize(Form f)
        {
            f.KeyPreview = true;
            f.ShowInTaskbar = false;
            f.ShowIcon = false;
            f.FormBorderStyle = FormBorderStyle.FixedSingle; //.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.Manual;
            Size s = this.ParentForm.Size;
            if (Config.Default.IsFullscreen)
            {
                f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
            //f.WindowState = FormWindowState.Normal;
            f.WindowState = this.ParentForm.WindowState;
            f.MaximizeBox = true;
            f.Size = new Size(s.Width, s.Height);
            f.Location = this.ParentForm.Location;
            if (Config.Default.mainIcon != null)
            {
                f.Icon = Config.Default.mainIcon;
                f.ShowIcon = true;
            }
            try
            {
                f.Cursor = new System.Windows.Forms.Cursor(typeof(SimpMain), "Cursor.cur");
            }
            catch { }
            f.Text = Config.AppName;
            f.Load += new EventHandler(f_Load);
        }

        void f_Load(object sender, EventArgs e)
        {
            try
            {
                Form f = sender as Form;
                if (f == null) return;
                SimpMain.ApplySize(f);
                f.WindowState = this.ParentForm.WindowState;
                //f.WindowState = FormWindowState.Maximized;
                f.MaximizeBox = false;
            }
            catch { }
        }

        public static void ChildFormLoad(Form f)
        {
        }

        private void BrowserControl_KeyDown(object sender, KeyEventArgs e)
        {
            this.HandleKeyDown(e);
        }

        private void sbtnLocation_KeyDown(object sender, KeyEventArgs e)
        {
            this.HandleKeyDown(e);
        }

        private void sbtnLocation_MouseWheel(object sender, MouseEventArgs e)
        {
            //this.list.HandleMouseWheel(e);
        }

        private void list_KeyDown(object sender, KeyEventArgs e)
        {
            this.HandleKeyDown(e);
        }

        void list_ItemKeyDown(object sender, KeyEventArgs e)
        {
            this.HandleKeyDown(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (SListItem.IsSInputKey(keyData))
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }

        private void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Shift || e.Alt || e.Control)
            {
                if (e.KeyCode == Keys.Space) 
                {
                    ShowPathList(false);
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = HandleKey(e.KeyCode);
        }

        private bool HandleKey(Keys k)
        {
            bool handled = false; // this.list.HandleKey(k);
            if (handled)
            {
                return true;
            }
            switch (k)
            {
                case Keys.SelectMedia:
                    //ShowPathList(false);
                    //handled = true;
                    break;
                case Keys.Left:
                    ShowPathList(false);
                    handled = true;
                    break;
                case Keys.Back:
                    HandleEsc();
                    handled = true;
                    break;
                case Keys.MediaStop:
                    HandleEsc();
                    handled = true;
                    break;
                case Keys.Escape:
                    HandleEsc();
                    handled = true;
                    break;
                case Keys.F5:
                    RefreshLocation();
                    handled = true;
                    break;
            }
            return handled;
        }

        private void RefreshLocation()
        {
            try
            {
                string path = this.BrowseLocation;
                string sdir = null;
                if (this.list.HasData)
                {
                    FItemData it = this.list.GetItemByIndex(this.list.CurrentItemIndex) as FItemData;
                    if ((it != null) && it.IsFolder && !it.IsUpFolder)
                    {
                        sdir = GetFileName(path);
                        path = Path.GetDirectoryName(path);
                    }
                }
                FItemImageResolver fir = this.list.ImageResolver as FItemImageResolver;
                if(fir != null)
                {
                    fir.ClearFileBitmaps();
                }
                SetBaseLocation(path, sdir);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void HandleEsc()
        {
            if (IsRoot)
            {
                ShowPathList(true);
            }
            else
            {
                this.BrowserControl_ItemActivated(0, false);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            //HandleKey(e.KeyCode);
        }

        private string NextDir(string baseDir)
        {
            try
            {
                if (string.IsNullOrEmpty(baseDir)) return null;
                while (true)
                {
                    string parentDir = Path.GetDirectoryName(baseDir);
                    if (string.IsNullOrEmpty(parentDir)) return null;
                    if (parentDir.Equals(baseDir, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return null;
                    }
                    string[] files = null;
                    try
                    {
                        files = Directory.GetDirectories(parentDir);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    if ((files != null) && (files.Length >= 0))
                    {
                        Array.Sort(files, ns.StringLogicalComparer.Default);
                        for (int i = 0; i < files.Length; i++)
                        {
                            string file = files[i];
                            if (IsSystemHidden(file))
                            {
                                continue;
                            }
                            if (file.Equals(baseDir, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (i < (files.Length - 1))
                                {
                                    for (int j = i + 1; j < files.Length; j++)
                                    {
                                        if (!IsSystemHidden(files[j]))
                                        {
                                            return files[j];
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    // none found, move up
                    baseDir = parentDir;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return null;
        }

        public static Point GetDirFiles(string dir)
        {
            Point p = new Point(0, 0);
            if (string.IsNullOrEmpty(dir)) return p;
            try
            {
                string[] files = null;
                try
                {
                    files = Directory.GetDirectories(dir);
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                if ((files != null) && (files.Length >= 0))
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        if (IsSystemHidden(file))
                        {
                            continue;
                        }
                        p.X++;
                    }
                }
                try
                {
                    files = Directory.GetFiles(dir);
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                if ((files != null) && (files.Length >= 0))
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        if (IsSystemHidden(file))
                        {
                            continue;
                        }
                        if (!Config.Default.CanProcessFile(file))
                        {
                            continue;
                        }
                        p.Y++;
                    }
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return p;
        }

        private void DeleteItem(int itemIndex)
        {
            try
            {
                if (IsRoot) return;
                FItemData it = this.list.GetItemByIndex(itemIndex) as FItemData;
                if (it == null) return;
                if (it.IsSpecial || it.IsUpFolder)
                {
                    return;
                }
                string path = this.GetItemPath(it);
                if(it.IsFolder)
                {
                    try
                    {
                        Directory.Delete(path, true);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
                else
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
                if (!(File.Exists(path) || Directory.Exists(path)))
                {
                    this.list.DeleteItem(itemIndex);
                }
            }
            catch (Exception ex)
            {
                Config.Default.Error(ex);
            }
        }

        private void RenameItem(int itemIndex, string newName)
        {
            try
            {
                if (IsRoot) return;
                FItemData it = this.list.GetItemByIndex(itemIndex) as FItemData;
                if (it == null) return;
                if (it.IsSpecial || it.IsUpFolder)
                {
                    return;
                }
                string path = this.GetItemPath(it);

                if (string.IsNullOrEmpty(newName)) return;
                newName = newName.Trim(':', '?', '*', '\\', '/', '<', '>', '|', '\"');
                if (string.IsNullOrEmpty(newName)) return;

                if (!Path.IsPathRooted(newName)) 
                {
                    string p = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(p)) return;
                    newName = Path.Combine(p, newName);
                }

                if (newName.Substring(0, 2).ToLower() != path.Substring(0, 2).ToLower())
                {
                    // different volume is not supported
                    return;
                }
                if (newName == path)
                {
                    return;
                }

                if(File.Exists(newName) || Directory.Exists(newName))
                {
                    return;
                }

                bool removeItem = false;
                string newFolder = Path.GetDirectoryName(newName);
                string currentFolder = Path.GetDirectoryName(path);
                if (newFolder.ToLower() != currentFolder.ToLower()) 
                {
                    removeItem = true;
                }
                bool createdNewFolder = false;
                if (!Directory.Exists(newFolder)) 
                {
                    Directory.CreateDirectory(newFolder);
                    createdNewFolder = true;
                }

                if (it.IsFolder)
                {
                    try
                    {
                        Directory.Move(path, newName);
                    }
                    catch (Exception ex)
                    { 
                        Config.Default.Error(ex);
                        removeItem = false;
                        if (createdNewFolder)
                        {
                            Directory.Delete(newFolder);
                        }
                    }
                }
                else
                {
                    try
                    {
                        File.Move(path, newName);
                    }
                    catch (Exception ex) 
                    {
                        Config.Default.Error(ex);
                        removeItem = false;
                        if (createdNewFolder)
                        {
                            Directory.Delete(newFolder);
                        }
                    }
                }
                if (removeItem)
                {
                    if (!(File.Exists(path) || Directory.Exists(path)))
                    {
                        this.list.DeleteItem(itemIndex);
                    }
                }
                else
                {
                    this.GoTo(newFolder, GetFileName(newName));
                }
            }
            catch (Exception ex)
            {
                Config.Default.Error(ex);
            }
        }

      }//EOC
}