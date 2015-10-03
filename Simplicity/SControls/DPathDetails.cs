using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class DPathDetails : Form
    {
        public DPathDetails()
        {
            InitializeComponent();
            SimpMain.ApplySize(this);
            this.Text = Config.AppName;
            LoadColors();
            Action = SpecialAction.Cancel;
            this.list.PageItemCount = Config.Default.itemsPerPage;
            this.list.ActivateItemOnClick = Config.Default.activateOnClick;
            this.list.ImageResolver = new FItemImageResolver(null);
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

        public DialogResult ShowPath(IWin32Window parent, bool isRoot, FItemData actualItem, string path, bool focusExit)
        {
            Action = SpecialAction.Cancel;
            selectedPath = path;
            int startIndex = 0;
            List<SList.ItemData> data = new List<SList.ItemData>();
            FItemData it = null;
            if (!string.IsNullOrEmpty(path))
            {
                it = new FItemData(Config.Default.Str(Config.StrOpenLauncher));
                it.IsUpFolder = false;
                //it.IsFolder = true;
                it.IsSpecial = true;
                it.Tag = SpecialAction.OpenLauncher;
                data.Add(it);

                string[] parts = path.Trim('\\').Split('\\');
                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    parts[i] = BrowserControl.CleanPath(parts[i]);
                    if (string.IsNullOrEmpty(parts[i])) continue;
                    it = new FItemData("> " + parts[i]);
                    it.IsUpFolder = true; // used as flag here
                    it.IsFolder = true;
                    string itemPath = MakePath(parts, i);
                    it.Tag = itemPath;
                    data.Add(it);
                }
            }

            it = new FItemData("> " + Config.Default.Str(Config.StrComputer) + " [" + Environment.MachineName + "]");
            it.IsUpFolder = false;
            it.IsFolder = true;
            it.Tag = SpecialAction.Computer;
            data.Add(it);
            string dp = BrowserControl.DesktopPath;
            if (!string.IsNullOrEmpty(dp))
            {
                it = new FItemData("> " + Config.Default.Str(Config.StrDesktop));
                it.IsUpFolder = false;
                it.IsFolder = true;
                it.Tag = SpecialAction.Desktop;
                data.Add(it);
            }
            if (!string.IsNullOrEmpty(path))
            {
                if (!((actualItem == null) || isRoot || actualItem.IsSpecial || actualItem.IsUpFolder))
                {
                    it = new FItemData(Config.Default.Str(Config.StrDelete));
                    it.IsUpFolder = false;
                    //it.IsFolder = true;
                    it.IsSpecial = true;
                    it.Tag = SpecialAction.Delete;
                    data.Add(it);

                    it = new FItemData(Config.Default.Str(Config.StrRename));
                    it.IsUpFolder = false;
                    //it.IsFolder = true;
                    it.IsSpecial = true;
                    it.Tag = SpecialAction.Rename;
                    data.Add(it);
                }

                it = new FItemData(Config.Default.Str(Config.StrPathInfo));
                it.IsUpFolder = false;
                //it.IsFolder = true;
                it.IsSpecial = true;
                it.Tag = SpecialAction.Info;
                data.Add(it);
            }

            if (!string.IsNullOrEmpty(path) || Config.Default.HasFavorites)
            {
                it = new FItemData(Config.Default.Str(Config.StrFavorites));
                it.IsUpFolder = false;
                //it.IsFolder = true;
                it.IsSpecial = true;
                it.Tag = SpecialAction.Favorites;
                data.Add(it);
            }

            if (Config.Default.canShowExit)
            {
                it = new FItemData(Config.Default.Str(Config.StrExit));
                it.IsUpFolder = false;
                //it.IsFolder = true;
                it.IsSpecial = true;
                it.Tag = SpecialAction.Exit;
                data.Add(it);
                if (focusExit)
                {
                    startIndex = data.Count - 1;
                }
            }

            it = new FItemData(Config.Default.Str(Config.StrSystem));
            it.IsUpFolder = false;
            //it.IsFolder = true;
            it.IsSpecial = true;
            it.Tag = SpecialAction.SystemMenu;
            data.Add(it);
            if (!Config.Default.canShowExit)
            {
                if (focusExit)
                {
                    startIndex = data.Count - 1;
                }
            }

            it = new FItemData(Config.Default.Str(Config.StrCancel));
            it.IsUpFolder = false;
            //it.IsFolder = true;
            it.IsSpecial = true;
            it.Tag = SpecialAction.Cancel;
            data.Add(it);

            this.list.SetItems(data, startIndex);

            return this.ShowDialog(parent);
        }

        private string MakePath(string[] parts, int maxIndex)
        {
            if (parts == null) return string.Empty;
            if (maxIndex >= parts.Length)
            {
                maxIndex = parts.Length - 1;
            }
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i <= maxIndex; i++)
            {
                sb.Append(parts[i]).Append("\\");
            }
            return BrowserControl.CleanPath(sb.ToString());
        }

        public enum SpecialAction { None, Desktop, Computer, Exit,
        Cancel, Info, OpenLauncher, Delete, Rename, SystemMenu, Favorites
        };

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
                SpecialAction action = SpecialAction.None;
                if ((it.Tag != null) && !it.IsUpFolder) // used as flag
                {
                    action = (SpecialAction)it.Tag;
                }
                switch (action)
                {
                    case SpecialAction.Cancel:
                        this.DialogResult = DialogResult.Cancel;
                        Close();
                        return;
                    case SpecialAction.Desktop:
                        this.selectedPath = BrowserControl.DesktopPath;
                        break;
                    case SpecialAction.Computer:
                        this.selectedPath = string.Empty;
                        break;
                }
                Action = action;
                if (it.IsUpFolder)
                {
                    this.selectedPath = (string)it.Tag;
                }
                this.DialogResult = DialogResult.OK;
                Close();
                return;
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

        private string selectedPath = null;
        public string Path
        {
            get
            {
                return selectedPath;
            }
        }

        private void DPathDetails_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
        }
    }
}
