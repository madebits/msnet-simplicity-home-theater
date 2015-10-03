using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class DSound : Form
    {
        public DSound()
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

        private string VolumeStr(int volume)
        {
            return " [" + volume + "]";
        }

        public DialogResult ShowSound(IWin32Window w)
        {
            List<SList.ItemData> data = new List<SList.ItemData>();
            FItemData it = null;

            int volume = 0;
            try
            {
                
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            string vstr = VolumeStr(volume);

            it = new FItemData(Config.Default.Str(Config.StrSoundUp) + vstr);
            it.IsUpFolder = false;
            it.IsSpecial = true;
            data.Add(it);

            it = new FItemData(Config.Default.Str(Config.StrSoundDown) + vstr);
            it.IsUpFolder = false;
            it.IsSpecial = true;
            data.Add(it);

            it = new FItemData(Config.Default.Str(Config.StrCancel));
            it.IsUpFolder = false;
            it.IsSpecial = true;
            data.Add(it);

            this.list.SetItems(data, 0);
            return this.ShowDialog(w);
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
                switch(itemIndex)
                {
                    case 0:
                        ChangeVolume(true);
                        return;
                    case 1:
                        ChangeVolume(false);
                        return;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        public void ChangeVolume(bool up)
        {
        }

        private void DSound_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
        }

    }
}
