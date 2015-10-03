using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace MediaPreview
{
    public partial class Preview : Form
    {
        private Thread th = null;
        private ManualResetEvent shouldStop = new ManualResetEvent(false);
        private Files mfiles = new Files();

        public Preview(string[] args)
        {
            InitializeComponent();
            this.timerSlide.Interval = Program.slideShowTimeSec * 1000;
            this.MouseWheel += new MouseEventHandler(Preview_MouseWheel);
            this.Text = Program.APPNAME;
            if(!Program.hasffmpeg)
            {
                this.Text += " <<< Warning: No ffmpeg.exe found! Movie previews will not work! >>>";
            }
            this.WindowState = FormWindowState.Normal;
            ApplySize(this);
            try
            {
                this.Cursor = new System.Windows.Forms.Cursor(typeof(Preview), "Cursor.cur");
            }
            catch { }

            this.mfiles.stop = new Files.DShouldStop(this.ShouldStop);
            this.mfiles.SetStartPath(Program.movieFile);
            this.mfiles.canLoop = Program.canLoop;
            if (Program.startSlideShow) 
            {
                this.slideShow = true;
            }
        }

        public static void ApplySize(Form f)
        {
            try
            {
                Screen scr = Screen.FromControl(f);
                f.Size = new System.Drawing.Size(scr.WorkingArea.Width, scr.WorkingArea.Height);
                f.Location = new System.Drawing.Point(scr.WorkingArea.X, scr.WorkingArea.Y);
            }
            catch { }
        }     

        private bool ShouldStop() 
        {
            return shouldStop.WaitOne(0);
        }

        private void Preview_Load(object sender, EventArgs e)
        {
            try
            {
                if (Program.fullScreen)
                {
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.TopMost = true;
                }
                ApplySize(this);
                this.WindowState = FormWindowState.Maximized;
                this.MaximizeBox = false;
                StartThread();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        private void StartThread()
        {
            try
            {
                if (th != null)
                {
                    shouldStop.Set();
                }
                if (th != null)
                {
                    th.Join(1000);
                }
                if (th != null) 
                {
                    Program.KillFFMPEG();
                    if(th != null)
                    {
                        th.Join(6000);
                    }
                    if (th != null)
                    {
                        //th.Abort();
                        System.Diagnostics.Debug.WriteLine("thread");
                    }
                }
                th = null;
            }
            catch { }
            th = null;
            shouldStop.Reset();
            th = new Thread(new ThreadStart(DoWork));
            th.Name = "PreviewThread";
            th.IsBackground = true;
            th.Start();
        }

        private void DoWork()
        {
            try
            {
                if (ShouldStop()) return;
                int count = mfiles.Apply(this.IsSlideShow);
                if (ShouldStop()) return;
                if (count > 0)
                {
                    Point m = mfiles.RowCols;
                    if ((m.X > 0) && (m.Y > 0))
                    {
                        Size size = this.ClientSize;
                        Size imageSize = new Size(size.Width / m.Y, size.Height / m.X);
                        for (int i = 0; i < count; i++)
                        {
                            if (ShouldStop()) return;
                            MFile mf = mfiles.GetAt(i);
                            if (mf == null)
                            {
                                break;
                            }
                            if (!mf.InitImage(imageSize, new Files.DShouldStop(this.ShouldStop)))
                            {
                                if (!mfiles.StartPathIsDir)
                                {
                                    break;
                                }
                            }
                            SafeRefresh();
                        }
                    }
                }
                if (ShouldStop()) return;
                SafeRefresh();
            }
            catch { }
            finally
            {
                th = null;
                try
                {
                    if (IsSlideShow)
                    {
                        //this.Invoke(new MethodInvoker(EnableSSTimer));
                        SlideShow(true);
                    }
                }
                catch { }
            }
        }

        private void SafeRefresh()
        {
            if (ShouldStop()) return;
            this.Invalidate();
            //this.Invoke(new MethodInvoker(_SafeRefresh));
        }    

        #region paint

        private void ClearPainBuffers()
        {
            if (this.screenBuffer != null)
            {
                this.screenBuffer.Dispose();
                this.screenBuffer = null;
            }
            if (thumbFont != null)
            {
                thumbFont.Dispose();
                thumbFont = null;
            }
            if (movieFont != null)
            {
                movieFont.Dispose();
                movieFont = null;
            }
        }

        private Bitmap screenBuffer = null;
        private Font thumbFont = null;
        private int lastItemHeight = 0;
        private Font movieFont = null;
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Size s = this.ClientSize;
                int previewWidth = s.Width;
                int previewHeight = s.Height;
                Point m = mfiles.RowCols;
                double progressPercent = mfiles.DisplayProgressPercent;
                const int percentWidth = 4;
                if (progressPercent >= 0) 
                {
                    previewWidth -= percentWidth;
                }
                bool hasData = ((m.X > 0) && (m.Y > 0));
                bool isMoviePreview = false;
                const int movieTextHeight = 40;
                if (hasData && mfiles.isMoviePreview) 
                {
                    isMoviePreview = true;
                    previewHeight -= movieTextHeight;
                }
                int w = m.Y <= 0 ? 0 : previewWidth / m.Y; // cols
                int h = m.X <= 0 ? 0 : previewHeight / m.X; // rows

                if ((screenBuffer == null) 
                    || (screenBuffer.Width != s.Width)
                    || (screenBuffer.Height != s.Height))
                {
                    if (this.screenBuffer != null)
                    {
                        this.screenBuffer.Dispose();
                        this.screenBuffer = null;
                    }
                    screenBuffer = new Bitmap(s.Width, s.Height);
                    
                }
                if (!hasData)
                {
                    h = s.Height;
                }
                if ((thumbFont == null) || (lastItemHeight != h)) 
                {
                    lastItemHeight = h;
                    if (thumbFont != null)
                    {
                        thumbFont.Dispose();
                        thumbFont = null;
                    }
                    int fontMaxHeight = h / 20;
                    for (int i = 8; i < 20; i++)
                    {
                        if (thumbFont != null) thumbFont.Dispose();
                        thumbFont = new Font("Arial", i);
                        SizeF ss = e.Graphics.MeasureString("WMjh", thumbFont);
                        if (ss.Height >= fontMaxHeight)
                        {
                            break;
                        }
                    }
                }
                if (movieFont == null)
                {
                    movieFont = new Font("Arial", 20);
                }

                using (Graphics g = Graphics.FromImage(screenBuffer))
                {
                    g.Clear(Color.Black);
                    if (hasData)
                    {
                        if (progressPercent >= 0)
                        {
                            Rectangle rp = new Rectangle(
                                previewWidth + 1,
                                1,
                                percentWidth - 2, 
                                (int)((s.Height - 2) * (progressPercent) / 100.0));
                            g.FillRectangle(Brushes.White, rp);
                        }
                        if (isMoviePreview) 
                        {
                            MFile mf = mfiles.GetAt(8);
                            if (mf != null)
                            {
                                mf.DrawFileText(g, 0, 0, previewWidth, 30, movieFont, false);
                            }
                        }
                        if ((w >= 25) && (h >= 25))
                        {
                            Point offset = new Point((previewWidth - w * m.Y) / 2, (previewHeight - h * m.X) / 2);
                            if (isMoviePreview) offset.Y += movieTextHeight;
                            if (offset.X < 0) offset.X = 0;
                            if (offset.Y < 0) offset.Y = 0;
                            for (int i = 0; i < m.X; i++)
                            {
                                for (int j = 0; j < m.Y; j++)
                                {
                                    int idx = i * m.Y + j;
                                    int x = (j * w) + offset.X;
                                    int y = (i * h) + offset.Y;
                                    MFile mf = mfiles.GetAt(idx);
                                    if (mf != null)
                                    {
                                        mf.Draw(g, x, y, w, h, m, thumbFont);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string text = "no data";
                        SizeF ss = g.MeasureString(text, thumbFont);
                        int tx = (s.Width - (int)ss.Width) / 2;
                        int ty = (s.Height - (int)ss.Height) / 2;
                        g.DrawString(text, thumbFont, Brushes.DarkGray, tx + 1, ty + 1);
                        g.DrawString(text, thumbFont, Brushes.White, tx, ty);
                    }
                }
                e.Graphics.DrawImage(screenBuffer, 0, 0);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }        

        

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        #endregion paint

        private void Preview_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void Preview_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
        
        private void Preview_FormClosing(object sender, FormClosingEventArgs e)
        {
            SlideShow(false);
            shouldStop.Set();

            CursorVisibility.Visible = true;
            timerMouse.Enabled = false;
            try
            {
                if (selectedFile == null)
                {
                    selectedFile = mfiles.GetFileAt(mfiles.CurrentIndex);
                    if (selectedFile == null)
                    {
                        selectedFile = Program.movieFile;
                    }
                }
                if (selectedFile != null)
                {
                    System.Diagnostics.Debug.WriteLine(selectedFile);
                }
            }
            catch { }
            
            Imager.GetTempFile(true);
        }

        #region nav

        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool slideShow = IsSlideShow;
            SlideShow(false);
            if ((e.KeyCode == Keys.Escape)
                || (e.KeyCode == Keys.Back)
                || (e.KeyCode == Keys.Enter) 
                || (e.KeyCode == Keys.MediaStop))
            {
                e.Handled = true;
                Close();
                return;
            }
            switch(e.KeyCode)
            {
                case Keys.MediaPlayPause:
                    e.Handled = true;
                    if (!slideShow)
                    {
                        SlideShow(true);
                        MoveToNextFile(Files.MoveDirection.Down);
                    }
                    break;
                case Keys.Space:
                    e.Handled = true;
                    if(e.Shift || e.Control || e.Alt)
                    {
                        SlideShow(true);
                    }
                    MoveToNextFile(Files.MoveDirection.Down);
                    break;
                case Keys.Right:
                case Keys.Down:
                case Keys.MediaNextTrack:
                    e.Handled = true;
                    MoveToNextFile(Files.MoveDirection.Down);   
                    break;
                case Keys.Left:
                case Keys.Up:
                case Keys.MediaPreviousTrack:
                     e.Handled = true;
                     MoveToNextFile(Files.MoveDirection.Up);
                    break;
                case Keys.Home:
                    MoveToNextFile(Files.MoveDirection.Head);
                    break;
                case Keys.End:
                    MoveToNextFile(Files.MoveDirection.End);
                    break;
                case Keys.PageDown:
                    MoveToNextFile(Files.MoveDirection.PageDown);
                    break;
                case Keys.PageUp:
                    MoveToNextFile(Files.MoveDirection.PageUp);
                    break;
                default:
                     base.OnKeyDown(e);
                     break;
            }
        }


        private bool MoveToNextFile(Files.MoveDirection direction) 
        {
            bool ok = mfiles.MoveToNextFile(direction);
            if (ok)
            {
                StartThread();
            }
            else 
            {
                if (!mfiles.canLoop)
                {
                    switch (direction)
                    {
                        case Files.MoveDirection.Down:
                            Close();
                            break;
                    }
                }
            }
            return ok;
        }

        private string selectedFile = null;
        private void Preview_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                bool ss = IsSlideShow;
                SlideShow(false);
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        MoveToNextFile(Files.MoveDirection.Down);
                        /*
                        {
                            string cf = GetClickedItem(e.X, e.Y);
                            if (!string.IsNullOrEmpty(cf)) 
                            {
                                if (Directory.Exists(cf))
                                {
                                    if (mfiles.SetStartPath(cf))
                                    {
                                        this.StartThread();
                                    }
                                }
                            }
                        }*/
                        break;
                    case MouseButtons.Middle:
                        if (!ss)
                        {
                            SlideShow(true);
                            MoveToNextFile(Files.MoveDirection.Down);
                        }
                        break;
                    case MouseButtons.Right:
                        shouldStop.Set();
                        try
                        {
                            selectedFile = GetClickedItem(e.X, e.Y);
                        }
                        catch { }
                        Close();
                        break;
                }
            }
            catch { }
        }

        private string GetClickedItem(int mx, int my)
        {
            try
            {
                Size s = this.ClientSize;
                Point m = mfiles.RowCols;
                bool hasData = ((m.X > 0) && (m.Y > 0));
                if (!hasData) return null;

                int w = s.Width / m.Y; // cols
                int h = s.Height / m.X; // rows

                int ix = 0;
                int iy = 0;

                if (m.Y > 1)
                {
                    iy = mx / w;
                }
                if (m.X > 1)
                {
                    ix = my / h;
                }
                int di = ix * m.Y + iy;
                int idx = mfiles.CurrentIndex + di;
                return mfiles.GetFileAt(idx);
            }
            catch { }
            return null;
        }

        void Preview_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                SlideShow(false);
                if (e.Delta < 0)
                {
                    MoveToNextFile(Files.MoveDirection.Down);
                }
                else
                {
                    MoveToNextFile(Files.MoveDirection.Up);
                }
            }
            catch { }
        }

        #endregion nav
        
        #region drag

        private void Preview_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                e.Effect = (GetClipFile(e.Data) != null) ? DragDropEffects.All : DragDropEffects.None;
            }
            catch { }
        }

        private void Preview_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                GoTo(GetClipFile(e.Data));
            }
            catch { }
        }

        private static string GetClipFile(IDataObject data)
        {
            if (data == null) return null;
            if(data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] ss = (string[])data.GetData(DataFormats.FileDrop);
                if((ss != null) && (ss.Length > 0))
                {
                    return ss[0];
                }
            }
            else if (data.GetDataPresent(DataFormats.Text)) 
            {
                return (string)data.GetData(DataFormats.Text);
            }
            return null;
        }

        private void GoTo(string file)
        {
            try
            {
                if (mfiles.SetStartPath(file))
                {
                    StartThread();
                }
                else 
                {
                    this.Refresh();
                }
            }
            catch { }
        }

        #endregion drag

        private volatile bool slideShow = false;
        private void SlideShow(bool on)
        {
            if (!mfiles.HasData)
            {
                on = false;
            }
            slideShow = on;
            if (on)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(EnableSSTimer));
                }
                else
                {
                    EnableSSTimer();
                }
            }
            else
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(DisableSSTimer));
                }
                else
                {
                    DisableSSTimer();
                }
            }
        }

        private bool IsSlideShow 
        {
            get 
            {
                try
                {
                    return slideShow;
                }
                catch { }
                return false;
            }
        }

        private void EnableSSTimer()
        {
            try
            {
                timerSlide.Enabled = true;
            }
            catch { }
        }

        private void DisableSSTimer()
        {
            try
            {
                timerSlide.Enabled = false;
            }
            catch { }
        }

        private void timerSlide_Tick(object sender, EventArgs e)
        {
            try
            {
                if (MoveToNextFile(Files.MoveDirection.Down))
                {
                    this.Invoke(new MethodInvoker(DisableSSTimer));
                }

            }
            catch { }
        }

        private void Preview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                CursorVisibility.Visible = true;
                timerMouse.Interval = 5000;
            }
            catch { }
        }

        private void timerMouse_Tick(object sender, EventArgs e)
        {
            try
            {
                CursorVisibility.Visible = false;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
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
