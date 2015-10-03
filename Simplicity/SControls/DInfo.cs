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
    public partial class DInfo : Form
    {
        public DInfo()
        {
            InitializeComponent();
            SimpMain.ApplySize(this);
            this.Text = Config.AppName;
            LoadColors();
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
            this.lblText.ForeColor = Config.Default.foreColor;
            this.lblText.BackColor = Config.Default.backColor;
            this.lblSize.ForeColor = Config.Default.foreColor;
            this.lblSize.BackColor = Config.Default.backColor;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Config.IsDialogCloseKey(e.KeyCode)
                    || (e.KeyCode == Keys.Space)
                    || (e.KeyCode == Keys.Enter)
                    || (e.KeyCode == Keys.MediaPlayPause)
                    || (e.KeyCode == Keys.Play))
            {
                e.Handled = true;
                this.DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            base.OnKeyDown(e);
        }

        int statusHeight = 10;
        string path = null;
        public DialogResult ShowInfo(IWin32Window w, string path, int statusHeight, Font font) 
        {
            if (string.IsNullOrEmpty(path)) return DialogResult.Cancel;
            this.statusHeight = statusHeight;
            this.path = path;
            if (font != null)
            {
                this.lblSize.Font = (Font)font.Clone();
                this.lblText.Font = (Font)font.Clone();
            }
            this.lblText.Text = path;
            ResizeControls();
            return ShowDialog(w);
        }

        private void ResizeControls() 
        {
            try
            {
                this.lblSize.Size = new Size(this.Width, this.statusHeight);
                /*
                this.lblText.Size = new Size(this.Width, this.Height - statusHeight);
                this.lblSize.Size = new Size(this.Width, this.statusHeight);
                this.lblText.Location = new Point(0, 0);
                this.lblSize.Location = new Point(0, this.Height - statusHeight); */
                //this.lblText.Dock = DockStyle.Fill;
                //this.lblSize.Dock = DockStyle.Bottom;
            }
            finally 
            {
                //this.ResumeLayout();
            }
            this.Invalidate();
            this.Refresh();
        }

        private void DInfo_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
            //LoadColors();
            //ResizeControls();
            try
            {
                Application.DoEvents();
                FillSize();
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void FillSize() 
        {
             if (string.IsNullOrEmpty(path)) return;
             if (Directory.Exists(path)) 
             {
                 Point p = BrowserControl.GetDirFiles(path);
                 this.lblSize.Text = Config.Default.Str(Config.StrFolders) + ": " + p.X
                     + " " + Config.Default.Str(Config.StrFiles) + ": " + p.Y;
             }
             else if (File.Exists(path))
             {
                 long l = 0;
                 string date = string.Empty;
                 try
                 {
                     FileInfo fi = new FileInfo(path);
                     l = fi.Length;
                     date = fi.LastWriteTime.ToString("yyyy-MM-dd"); // HH:mm:ss
                     fi = null;
                 }
                 catch (Exception ex) { Config.Default.Error(ex); }
                 string t = Config.Default.Str(Config.StrSize) + ": " + Config.SizeStr(l);
                 if(!string.IsNullOrEmpty(date))
                 {
                     t += " " + Config.Default.Str(Config.StrDateModified) + ": " + date;
                 }
                 this.lblSize.Text = t;
             }
        }

        private void lblSize_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lblText_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DInfo_Resize(object sender, EventArgs e)
        {
            ResizeControls();
        }

        private void timerTime_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                string t = now.ToString("HH:mm");
                string d = now.ToString("yyyy-MM-dd");
                t = t + Environment.NewLine + d;
                if(t != this.lblText.Text)
                {
                    this.lblText.Text = t;
                    this.lblText.Invalidate();
                    this.lblText.Refresh();
                }
                /*
                if (d != this.lblSize.Text)
                {
                    this.lblSize.Text = d;
                    this.lblSize.Invalidate()();
                }
                */
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        public DialogResult ShowTime(IWin32Window w, int statusHeight, Font font)
        {
            this.statusHeight = statusHeight;
            if (font != null)
            {
                this.lblSize.Font = (Font)font.Clone();
                this.lblText.Font = (Font)font.Clone();
            }
            this.lblText.TextAlign = ContentAlignment.MiddleCenter;
            this.lblSize.TextAlign = ContentAlignment.MiddleCenter;
            ResizeControls();
            timerTime.Enabled = true;
            return ShowDialog(w);
        }

        private void DInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerTime.Enabled = false;
        }

        public DialogResult ShowText(IWin32Window w, int statusHeight, Font font, string text1, string text2, bool center)
        {
            this.statusHeight = statusHeight;
            if (font != null)
            {
                this.lblSize.Font = (Font)font.Clone();
                this.lblText.Font = (Font)font.Clone();
            }
            if (center)
            {
                this.lblText.TextAlign = ContentAlignment.MiddleCenter;
                this.lblSize.TextAlign = ContentAlignment.MiddleCenter;
            }
            if (text1 == null) text1 = string.Empty;
            if (text2 == null) text2 = string.Empty;
            this.lblText.Text = text1;
            this.lblSize.Text = text2;
            ResizeControls();
            return ShowDialog(w);
        }
    }
}
