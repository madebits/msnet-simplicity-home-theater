using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class DWins : Form
    {
        public DWins()
        {
            InitializeComponent();
            SimpMain.ApplySize(this);
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
            try
            {
                if (it.Tag != null)
                {
                    int pid = (int)it.Tag;
                    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                    if (p != null)
                    {
                        System.Diagnostics.ProcessModule pm = p.MainModule;
                        if (pm != null)
                        {
                            string path = pm.FileName;
                            if (!string.IsNullOrEmpty(path))
                            {
                               return BrowserControl.CleanPath(path);
                            }
                        }
                    }
                    p.Close();
                    p = null;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return it.Text;
        }

        class ProcessComparator : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                try
                {
                    if (null == x && null == y) return 0;
                    if (null == x) return -1;
                    if (null == y) return 1;
                    if (x is System.Diagnostics.Process && y is System.Diagnostics.Process)
                    {
                        System.Diagnostics.Process px = (System.Diagnostics.Process)x;
                        System.Diagnostics.Process py = (System.Diagnostics.Process)y;
                        string tx = null;
                        int idx = -1;
                        try { tx = px.MainWindowTitle; idx = px.Id; } catch { }
                        string ty = null;
                        int idy = -1;
                        try { ty = py.MainWindowTitle; idy = py.Id; } catch{}
                        int r = ns.StringLogicalComparer.Default.Compare(tx, ty);
                        if (r != 0) return r;
                        return System.Collections.Comparer.Default.Compare(idx, idy);
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                return System.Collections.Comparer.Default.Compare(x, y);
            }
        }

        public DialogResult ShowList(IWin32Window w)
        {
            List<SList.ItemData> data = new List<SList.ItemData>();
            FItemData it = null;

            it = new FItemData(Config.Default.Str(Config.StrCancel));
            it.IsUpFolder = false;
            it.IsSpecial = true;
            data.Add(it);
            
            /*
            if ((Config.Default.launchers != null) && (Config.Default.launchers.Count > 0))
            {
                it = new FItemData(Config.Default.Str(Config.StrKillApps));
                it.IsUpFolder = true; // used as flag
                //it.IsFolder = true;
                it.IsSpecial = true;
                it.Tag = null;
                data.Add(it);
            }
            */

            try
            {
                System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcesses();
                Array.Sort(p, new ProcessComparator());
                for (int i = 0; i < p.Length; i++)
                {
                    try
                    {
                        if (p[i].Id == currentProcess.Id) continue;
                        if (p[i].MainWindowHandle == IntPtr.Zero)
                        {
                            continue;
                        }
                        System.Diagnostics.ProcessModule pm = p[i].MainModule;
                        if (pm == null) continue;
                        string path = pm.FileName;
                        if (string.IsNullOrEmpty(path)) continue;
                        string text = p[i].MainWindowTitle;
                        if (string.IsNullOrEmpty(text)) continue;

                        it = new FItemData(text);
                        it.IsUpFolder = false;
                        it.IsSpecial = false;
                        it.Tag = p[i].Id;
                        data.Add(it);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    try
                    {
                        p[i].Close();
                        p[i] = null;
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
                currentProcess.Close();
                currentProcess = null;
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            

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
                /*
                else if (it.IsUpFolder) // used as flag
                {
                    Config.Default.KillAppItemInstances();
                    this.DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }*/
                else if (it.Tag != null)
                {
                    WinName = it.Text;
                    ProcessName = GetModuleFileName(GetSelectProcess());
                    if(!string.IsNullOrEmpty(ProcessName))
                    {
                        ProcessName = System.IO.Path.GetFileName(ProcessName);
                    }
                    this.DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        public string WinName = null;
        public string ProcessName = null;

        public static string GetModuleFileName(System.Diagnostics.Process p)
        {
            try
            {
                if (p == null) return string.Empty;
                System.Diagnostics.ProcessModule pm = p.MainModule;
                if (pm == null) return string.Empty;
                string path = pm.FileName;
                return path;
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return string.Empty;
        }

        public System.Diagnostics.Process GetSelectProcess()
        {
            try
            {
                FItemData it = this.list.GetItemByIndex(this.list.CurrentItemIndex) as FItemData;
                if (it == null) return null;
                if (it.Tag == null) return null;
                int pid = (int)it.Tag;
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                return p;
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return null;
        }

        public enum ActivateAction{ Activate, Maximize, Kill, KillAll }

        public void ActivateApp(ActivateAction aa)
        {
            Application.DoEvents();
            //this.BeginInvoke(new DActivateApp(InnerActivateApp), new object[] { aa });
            InnerActivateApp(aa);
        }

        private delegate void DActivateApp(ActivateAction aa);
        private void InnerActivateApp(ActivateAction aa)
        {
            try
            {
                System.Diagnostics.Process p = GetSelectProcess();
                if (p == null)
                {
                    return;
                }
                switch(aa)
                {
                    case ActivateAction.Activate:
                        if (p.MainWindowHandle != IntPtr.Zero)
                        {
                            Program.Win32.ActivateWin(p.MainWindowHandle);
                        }
                        break;
                    case ActivateAction.Maximize:
                        if (p.MainWindowHandle != IntPtr.Zero)
                        {
                            Program.Win32.MaximizeWin(p.MainWindowHandle);
                        }
                        break;
                    case ActivateAction.Kill:
                        try
                        {
                            p.Kill();
                        }
                        catch (Exception ex) { Config.Default.Error(ex); }
                        break;
                    case ActivateAction.KillAll:
                        string path = GetModuleFileName(p);
                        if(!string.IsNullOrEmpty(path))
                        {
                            AppItem.KillInstances(System.IO.Path.GetFileNameWithoutExtension(path));
                        }
                        break;
                }
                p.Close();
                p = null;
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void DWins_Load(object sender, EventArgs e)
        {
            BrowserControl.ChildFormLoad(this);
        }

    }
}
