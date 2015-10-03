using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class DConfirm : Form
    {
        public DConfirm()
        {
            InitializeComponent();
            SimpMain.ApplySize(this);
            LoadColors();
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

        private bool cancelFirst = true;
        public DialogResult ShowConfirm(IWin32Window w, string[] confirmText, bool cancelFirst)
        {
            return ShowConfirm(w, confirmText, cancelFirst, true, null);
        }

        public bool checkItemType = false; // can be slow

        public DialogResult ShowConfirm(IWin32Window w, string[] confirmText, bool cancelFirst, bool areFolders, SList.ItemImageResolver imgResolver) 
        {
            Application.DoEvents();
            if (imgResolver != null)
            {
                this.list.ImageResolver = imgResolver;
            }
            this.cancelFirst = cancelFirst;
            if ((confirmText == null) || (confirmText.Length <= 0))
            {
                return DialogResult.Cancel;
            }
            
            List<SList.ItemData> data = new List<SList.ItemData>();
            FItemData it = null;

            if (this.cancelFirst)
            {
                it = new FItemData(Config.Default.Str(Config.StrCancel));
                it.IsUpFolder = false;
                it.IsSpecial = true;
                data.Add(it);
            }

            for (int i = 0; i < confirmText.Length; i++)
            {
                if (string.IsNullOrEmpty(confirmText[i]))
                {
                    continue;
                }
                it = new FItemData(confirmText[i]);
                it.IsUpFolder = false;
                if (checkItemType)
                {
                    bool isDir = false;
                    bool isFile = false;
                    try
                    {
                        isDir = System.IO.Directory.Exists(confirmText[i]);
                    }
                    catch { }
                    try
                    {
                        isFile = System.IO.File.Exists(confirmText[i]);
                    }
                    catch { }
                    it.IsFolder = isDir;
                    if(!isDir)
                    {
                        it.IsSpecial = !isFile;
                    }
                }
                else
                {
                    it.IsFolder = areFolders;
                    it.IsSpecial = !areFolders;
                }
                it.Tag = confirmText[i]; // user as flag
                data.Add(it);
            }

            if (!this.cancelFirst)
            {
                it = new FItemData(Config.Default.Str(Config.StrCancel));
                it.IsUpFolder = false;
                it.IsSpecial = true;
                data.Add(it);
            }

            this.list.SetItems(data, 0);
            return this.ShowDialog(w);
        }

        public int SelectedItemIndex
        {
            get
            {
                int idx = this.list.CurrentItemIndex;
                if(cancelFirst)
                {
                    if (idx == 0) return -1;
                    idx--;
                }
                else
                {
                    if(idx >= this.list.Items.Count - 1)
                    {
                        return -1;
                    }
                }
                return idx; 
            }
        }

        public string SelectedItemText
        {
            get
            {
                return this.list.Items[this.list.CurrentItemIndex].Text;
            }
        }

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
                if (it.Tag != null)
                {
                    this.DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DConfirm_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
        }
    }
}
