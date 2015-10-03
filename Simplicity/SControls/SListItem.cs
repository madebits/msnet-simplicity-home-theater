using System;
using System.Drawing;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class SListItem : UserControl
    {
        public SListItem()
        {
            InitializeComponent();
            this.Disposed += new EventHandler(SListItem_Disposed);
            LoadColors();
            Selected = false;
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
            this.Invalidate();
        }

        public SGraphics sg = new SGraphics();

        void SListItem_Disposed(object sender, EventArgs e)
        {
            timerMarque.Enabled = false;
            if (screenBufferBitmap != null)
            {
                screenBufferBitmap.Dispose();
                screenBufferBitmap = null;
            }
            if (sg != null)
            {
                sg.Dispose();
                sg = null;
            }
            if (this.img != null)
            {
                this.img.Dispose();
                this.img = null;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (SListItem.IsSInputKey(keyData))
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }

        public static bool IsSInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Tab:
                    return true;
                case Keys.Down:
                    return true;
                case Keys.Up:
                    return true;
                case Keys.Home:
                    return true;
                case Keys.End:
                    return true;
                case Keys.PageDown:
                    return true;
                case Keys.PageUp:
                    return true;
                case Keys.Right:
                    return true;
                case Keys.Left:
                    return true;
            }
            return false;
        }

        public int VisibleIndex { get; set; }

        private Image img = null;

        public Image Image
        {
            get { return img; }
            set
            {
                if (this.img != null)
                {
                    this.img.Dispose();
                    this.img = null;
                }
                this.img = value;
            }
        }

        private volatile bool isSelected = false;

        public bool Selected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    Color c = this.BackColor;
                    this.BackColor = this.ForeColor;
                    this.ForeColor = c;
                    isSelected = value;
                    timerMarque.Enabled = isSelected;
                    ResetMarque();
                    this.Invalidate();
                    this.Refresh();
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            return;
            /*
            if(this.isSelected)
            {
                return;
            }
            base.OnPaintBackground(e);
            */ 
        }

        private const int offset = 2;
        private Bitmap screenBufferBitmap = null;
        private void SListItem_Paint(object sender, PaintEventArgs e)
        {
            SGraphics.SetGraphicProperies(e.Graphics);
            Size s = this.Size;
            sg.Init(e.Graphics, this.ForeColor, this.BackColor, s);
            if ((screenBufferBitmap == null) || (screenBufferBitmap.Width != s.Width) || (screenBufferBitmap.Height != s.Height))
            {
                if(screenBufferBitmap != null)
                {
                    screenBufferBitmap.Dispose();
                    screenBufferBitmap = null;
                }
                screenBufferBitmap = new Bitmap(s.Width, s.Height);
            }
            using(Graphics g = Graphics.FromImage(screenBufferBitmap))
            {
                g.Clear(this.BackColor);
                if (!isSelected || (lastPositionX == 0) || (!locationXModified))
                {
                    lastPositionX = s.Height - offset;
                }
                textSize = sg.DrawText(g, this.Text, s, lastPositionX);
                g.FillRectangle(sg.backBrush, new Rectangle(-offset, -offset, s.Height + offset, s.Height + offset));
                if (this.Image != null)
                {
                    try
                    {
                        g.DrawImage(this.Image,
                            offset, offset,
                            s.Height - 2 * offset, s.Height - 2 * offset);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                }
            }
            e.Graphics.DrawImage(screenBufferBitmap, 0, 0);
        }

        private volatile int lastPositionX = 0;
        private SizeF textSize = new SizeF(0, 0);
        private int keepLastPositionCount = 0;
        private int keepLastPositionMinCount = 0;
        private volatile bool locationXModified = false;
        private void timerMarque_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!isSelected) return;
                if (textSize.Width <= 0) return;
                Size s = this.Size;
                int originalStartX = s.Height - offset;
                int textWidth = (int)textSize.Width + s.Height + 15;
                int delta = textWidth - this.Width - originalStartX;
                if (delta <= 0)
                {
                    lastPositionX = 0;
                }
                else
                {
                    keepLastPositionCount--;
                    if (keepLastPositionCount < 0)
                    {
                        int minPosition = originalStartX - delta - 3 * s.Height; // s.Height buffer
                        int dx = (s.Height / 16);
                        if (dx < 10) dx = 10;
                        lastPositionX -= dx;
                        locationXModified = true;
                        if (lastPositionX < minPosition)
                        {
                            lastPositionX += dx; // reset
                            keepLastPositionMinCount--;
                            if (keepLastPositionMinCount < 0)
                            {
                                ResetMarque();
                            }
                        }
                        this.Invalidate();
                        this.Refresh();
                    }
                }
            }
            catch (Exception ex) { Config.Default.Error(ex);  }
        }

        private void ResetMarque()
        {
            try
            {
                keepLastPositionCount = 20;
                keepLastPositionMinCount = 5;
                lastPositionX = 0;
                locationXModified = false;
                textSize = new SizeF(0, 0);
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void SListItem_MouseDown(object sender, MouseEventArgs e)
        {
            this.BorderStyle = BorderStyle.Fixed3D;
            ResetMarque();
        }

        private void SListItem_MouseUp(object sender, MouseEventArgs e)
        {
            this.BorderStyle = BorderStyle.None;
            ResetMarque();
        }

        private void SListItem_Load(object sender, EventArgs e)
        {
            //LoadColors();
        }


    }
}