using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Simplicity.SControls
{
    public partial class DItemActivated : Form
    {
        public DItemActivated()
        {
            InitializeComponent();
            SimpMain.ApplySize(this);
            this.Text = Config.AppName;
            LoadColors();
            this.list.PageItemCount = Config.Default.itemsPerPage;
            this.list.ActivateItemOnClick = Config.Default.activateOnClick;
            this.list.ImageResolver = new FItemImageResolver(new FItemImageResolver.DItemPathResolver(this.GetItemPath));
            this.list.ActivateOnSpaceKey = true;
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Config.IsDialogCloseKey(e.KeyCode))
            {
                e.Handled = true;
                this.DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            base.OnKeyDown(e);
        }

        private string GetItemPath(FItemData it)
        {
            if (!it.IsUpFolder)
            {
                return it.Text;
            }
            AppItem app = it.Tag as AppItem;
            if (app == null) return it.Text;
            return BrowserControl.CleanPath(System.IO.Path.GetFullPath(app.IconPath));
        }

        public DialogResult ShowLaunchers(IWin32Window w)
        {
            List<SList.ItemData> data = new List<SList.ItemData>();
            FItemData it = null;
            it = new FItemData(Config.Default.Str(Config.StrCancel));
            it.IsUpFolder = false;
            it.IsSpecial = true;
            it.Tag = SpecialAction.Cancel;
            data.Add(it);

            if ((Config.Default.launchers != null) && (Config.Default.launchers.Count > 0))
            {
                it = new FItemData(Config.Default.Str(Config.StrKillApps));
                it.IsUpFolder = false;
                //it.IsSpecial = true;
                it.IsFolder = true;
                it.Tag = SpecialAction.KillAll;
                data.Add(it);
            }

            it = new FItemData(Config.Default.Str(Config.StrReloadLaunchers));
            it.IsUpFolder = false;
            //it.IsSpecial = true;
            it.IsFolder = true;
            it.Tag = SpecialAction.ReloadApps;
            data.Add(it);
        
            int startIndex = 0;
            
            if (Config.Default.launchers != null)
            {
                for (int i = 0; i < Config.Default.launchers.Count; i++)
                {
                    AppItem app = Config.Default.launchers[i];
                    if (!app.IsValid || app.IsShellPath)
                    {
                        continue;
                    }
                    it = new FItemData(app.Name);
                    it.IsUpFolder = true; // used as flag
                    it.IsFolder = false;
                    it.Tag = app;
                    data.Add(it);
                    if ((startIndex == 0) && app.IsSame(Config.Default.lastAppItemLauncherUsed))
                    {
                        startIndex = data.Count - 1;
                    }
                }
            }

            this.list.SetItems(data, startIndex);
            return ShowDialog(w);
        }

        private string runPath = null;
        private bool runPathIsDir = false;
        private bool runPathIsUpDir = false;
        public DialogResult ShowOpen(IWin32Window w, string path, bool isDir, bool isUpDir)
        {
            runPath = BrowserControl.CleanPath(path);
            runPathIsDir = isDir;
            runPathIsUpDir = isUpDir;
            if (string.IsNullOrEmpty(path)) return DialogResult.Cancel;
            List<SList.ItemData> data = new List<SList.ItemData>();
            FItemData it = null;

            string fname = BrowserControl.GetFileName(path);

            if (isDir) 
            {
                string dname = fname;
                if (isUpDir) 
                {
                    dname = BrowserControl.GetFileName(Path.GetDirectoryName(path));
                    if (string.IsNullOrEmpty(dname)) dname = Config.Default.Str(Config.StrComputer);
                }

                it = new FItemData(Config.Default.Str(Config.StrExplore) + " [" + dname + "]");
                it.IsUpFolder = false;
                it.IsFolder = true;
                //it.IsSpecial = true;
                it.Tag = SpecialAction.ExploreDir;
                data.Add(it);
            }

            it = new FItemData(Config.Default.Str(Config.StrOpen) + " [" + fname + "]");
            it.IsUpFolder = false;
            //it.IsFolder = true;
            it.IsSpecial = true;
            it.Tag = SpecialAction.Default;
            data.Add(it);

            it = new FItemData(Config.Default.Str(Config.StrOpenPathDetails));
            it.IsUpFolder = false;
            //it.IsFolder = true;
            it.IsSpecial = true;
            it.Tag = SpecialAction.ShowPath;
            data.Add(it);

            it = new FItemData(Config.Default.Str(Config.StrCancel));
            it.IsUpFolder = false;
            //it.IsFolder = true;
            it.IsSpecial = true;
            it.Tag = SpecialAction.Cancel;
            data.Add(it);

            int startIndex = 0;

            if(Config.Default.launchers != null)
            {
                for (int i = 0; i < Config.Default.launchers.Count; i++)
                {
                    AppItem app = Config.Default.launchers[i];
                    if (!app.AppliesTo(runPath, runPathIsDir))
                    {
                        continue;
                    }
                    it = new FItemData(app.Name);
                    it.IsUpFolder = true; // used as flag
                    it.IsFolder = false;
                    it.Tag = app;
                    data.Add(it);
                    if ((startIndex == 0) && app.IsSame(Config.Default.lastAppItemUsed))
                    {
                        startIndex = data.Count - 1;
                    }
                }
            }

            this.list.SetItems(data, startIndex);
            return ShowDialog(w);
        }

        public enum SpecialAction { None, Default, Cancel, ExploreDir, ShowPath, KillAll, ReloadApps };

        private void list_ItemActivated(int itemIndex, bool isContextMenu)
        {
            try
            {
                if (isContextMenu) return;
                FItemData it = this.list.GetItemByIndex(itemIndex) as FItemData;
                if (it == null)
                {
                    this.DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }
                SpecialAction action = SpecialAction.Cancel;
                if (it.IsUpFolder)
                {
                    action = SpecialAction.None;
                }
                else
                {
                    action = (SpecialAction)it.Tag;
                }
                Action = action;
                switch (action)
                {
                    case SpecialAction.Cancel:
                        this.DialogResult = DialogResult.Cancel;
                        Close();
                        return;
                    case SpecialAction.KillAll:
                        Config.Default.KillAppItemInstances();
                        this.DialogResult = DialogResult.Cancel;
                        Close();
                        return;
                    case SpecialAction.ReloadApps:
                        Config.Default.LoadApps();
                         this.DialogResult = DialogResult.Cancel;
                        Close();
                        return;
                    default:
                        this.DialogResult = DialogResult.OK;
                        Close();
                        return;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        public SpecialAction Action
        {
            get;
            private set;
        }

        public void RunPath()
        {
            RunPath(false);
        }

        public void RunPath(bool noPath)
        {
            if (!noPath)
            {
                if (string.IsNullOrEmpty(runPath)) return;
            }
            Application.DoEvents();
            SpecialAction action = SpecialAction.Cancel;
            FItemData it = this.list.GetItemByIndex(this.list.CurrentItemIndex) as FItemData;
            AppItem appItem = null;
            if (it.IsUpFolder)
            {
                action = SpecialAction.None;
                appItem = it.Tag as AppItem;
            }
            else
            {
                action = (SpecialAction)it.Tag;
            }
            if(!string.IsNullOrEmpty(runPath))
            {
                if(Directory.Exists(runPath) && !runPath.EndsWith("\\"))
                {
                    runPath += "\\";
                }
            }
            switch (action)
            {
                case SpecialAction.Default:
                    if (!noPath && !string.IsNullOrEmpty(runPath))
                    {
                        Config.Default.lastAppItemUsed = null;
                        Config.Default.Log("Launch: " + runPath);
                        System.Diagnostics.Process p = System.Diagnostics.Process.Start(runPath);
                        if (p != null)
                        {
                            AppItem.MaximizeProcessWin(p);
                        }
                    }
                    break;
                default:
                    if (appItem == null)
                    {
                        return;
                    }
                    Config.Default.lastAppItemUsed = appItem;
                    Config.Default.lastAppItemLauncherUsed = appItem;
                    appItem.Run(runPath);
                    break;
            }
        }

        private void DItemActivated_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
        }

    }
}
