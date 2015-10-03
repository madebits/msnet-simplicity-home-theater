using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace Simplicity
{
    public partial class SimpMain : Form
    {
        public static SimpMain MainForm = null;

        private string[] args = null;
        public SimpMain(string[] args)
        {
            this.args = args;
            InitializeComponent();
            ApplySize();
            Config.Default.Load();
            this.Text = Config.AppName;
            try
            {
                this.Cursor = new System.Windows.Forms.Cursor(typeof(SimpMain), "Cursor.cur");
            }
            catch { }
            MainForm = this;
        }

        private void ApplySize()
        {
            ApplySize(this);
        }

        public static void ApplySize(Form f) 
        {
            try
            {
                Screen scr = Screen.FromControl(f);
                f.Size = new System.Drawing.Size(scr.WorkingArea.Width, scr.WorkingArea.Height);
                f.Location = new System.Drawing.Point(scr.WorkingArea.X, scr.WorkingArea.Y);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void Simplicity_Load(object sender, EventArgs e)
        {
            try
            {
                this.SuspendLayout();
                Config.Default.Load();
                LoadColors();
                if (Config.Default.IsFullscreen)
                {
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                }
                ApplySize();
                this.WindowState = FormWindowState.Maximized;
                this.MaximizeBox = false;
                Config.Default.mainIcon = this.Icon;

                bool pathIsFromConfigFile = !Config.Default.initialPathIsUpFolder;
                string startDirText = null;
                string path = Config.Default.initialPath;
                try
                {
                    if ((this.args != null) && (args.Length >= 1))
                    {
                        pathIsFromConfigFile = Config.Default.userInitialPathIsUpFolder;
                        path = SControls.BrowserControl.CleanPath(this.args[0]);
                    }
                    if (!string.IsNullOrEmpty(path))
                    {
                        bool fileExist = File.Exists(path);
                        bool dirExist = Directory.Exists(path);
                        if (!fileExist && !dirExist)
                        {
                            path = null;
                        }
                        else
                        {
                            if (dirExist && pathIsFromConfigFile)
                            {
                                startDirText = SControls.BrowserControl.GetFileName(path);
                                if (!string.IsNullOrEmpty(startDirText))
                                {
                                    if (startDirText.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        path = null;
                                    }
                                    else
                                    {
                                        string t = Path.GetDirectoryName(path);
                                        if (string.IsNullOrEmpty(t)
                                            || t.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            startDirText = null;
                                        }
                                        else
                                        {
                                            path = t;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                this.browser.GoTo(path, startDirText);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            finally
            {
                this.ResumeLayout();
            }
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
        }

        private void SimpMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if((e.CloseReason == CloseReason.UserClosing) && Config.Default.IsFullscreen)
                {
                    e.Cancel = true;
                    return;
                }
                string path = string.Empty;
                try
                {
                    path = this.browser.BrowseLocation;
                    /*
                    if (File.Exists(path) && !Directory.Exists(path))
                    {

                    }
                    else
                    {
                        path = this.browser.BaseLocation;
                    }
                    */ 
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                if (path == null) path = string.Empty;
                Config.Default.initialPath = path;
                Config.Default.initialPathIsUpFolder = this.browser.IsUpFolderItem;
                Config.Default.SaveSettings();
                Config.Default.Close();
                SimpMain.MainForm = null;
            }
            catch { }
        }

        class CursorVisibility
        {
            private static volatile bool visible = true;

            public static bool Visible
            {
                get { return visible; }
                set
                {
                    if (visible == value) return;
                    visible = value;
                    if (visible) Cursor.Show();
                    else Cursor.Hide();
                }
            }
        }
    }
}