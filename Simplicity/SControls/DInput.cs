using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Simplicity.SControls
{
    public partial class DInput : Form
    {
        static class Win32
        {
            [DllImport("user32")]
            public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
            [DllImport("user32")]
            public static extern bool ShowCaret(IntPtr hWnd);
            [DllImport("User32")]
            public static extern bool SetCaretPos(int x, int y);
            [DllImport("user32")]
            public static extern bool DestroyCaret();
        }

        public DInput()
        {
            InitializeComponent();
            SimpMain.ApplySize(this);
            this.txtText.GotFocus += new EventHandler(txtText_GotFocus);
            this.txtText.LostFocus += new EventHandler(txtText_LostFocus);
            this.Text = Config.AppName;
            LoadColors();
            try
            {
                this.txtText.Cursor = new System.Windows.Forms.Cursor(typeof(SimpMain), "Cursor.cur");
            }
            catch { }
        }

        void txtText_LostFocus(object sender, EventArgs e)
        {
            try
            {
                Win32.DestroyCaret();
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        void txtText_GotFocus(object sender, EventArgs e)
        {
            ChangeCaret(this.txtText.Handle);
        }

        private void ChangeCaret(IntPtr handle)
        {
            try
            {
                if (Win32.CreateCaret(handle, IntPtr.Zero, 10, this.statusHeight))
                {
                    bool ok = Win32.ShowCaret(handle);
                }
                Win32.SetCaretPos(0, 0);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
            this.txtText.ForeColor = Config.Default.foreColor;
            this.txtText.BackColor = Config.Default.backColor;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool isConfirm = (
                    (e.KeyCode == Keys.Enter)
                    || (e.KeyCode == Keys.MediaPlayPause)
                    || (e.KeyCode == Keys.Play));
            bool isCancel = ((e.KeyCode == Keys.Escape)
                  || (e.KeyCode == Keys.MediaStop));

            if (isConfirm || isCancel)
            {
                e.Handled = true;
                this.DialogResult = isConfirm ? DialogResult.OK : DialogResult.Cancel;
                Close();
                return;
            }
            base.OnKeyDown(e);
        }

        private bool isEditTextMode = false;
        public DialogResult ShowEditInput(IWin32Window w, string text, int statusHeight, Font font)
        {
            isEditTextMode = true;
            this.statusHeight = statusHeight;
            if (font != null)
            {
                this.txtText.Font = (Font)font.Clone();
            }
            if (text != null)
            {
                this.txtText.Text = text;
                this.txtText.SelectionStart = 0;
                this.txtText.SelectionLength = this.txtText.Text.Length;
            }
            return ShowDialog(w);
        }

        public string UserText 
        {
            get { return this.txtText.Text; }
        }

        private int statusHeight = 20;
        public DialogResult ShowInput(IWin32Window w, int statusHeight, Font font, bool editExisting)
        {
            this.statusHeight = statusHeight;
            if (font != null)
            {
                this.txtText.Font = (Font)font.Clone();
            }
            if (editExisting)
            {
                try
                {
                    this.txtText.Text = Clipboard.GetText();
                    this.txtText.SelectionStart = this.txtText.MaxLength - 1;
                    this.txtText.SelectionLength = 0;
                }
                catch (Exception ex) { Config.Default.Error(ex); }
            }
            return ShowDialog(w);
        }

        private void txtText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void txtText_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                }
                Close();
            }
        }

        private void CloseInput()
        {
            if (isEditTextMode)
            {
                return;
            }
            try
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    Clipboard.SetText(this.txtText.Text);
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void DInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseInput();
        }

        private void DInput_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
        }

        

    }
}
