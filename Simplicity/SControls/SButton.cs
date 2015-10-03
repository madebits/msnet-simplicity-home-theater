using System;
using System.Drawing;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class SButton : UserControl
    {
        public SButton()
        {
            InitializeComponent();
            LoadColors();
            this.BType = ButtonType.Text;
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
            this.Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (SListItem.IsSInputKey(keyData))
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }

        public enum ButtonType { Text, Up, Down, Home, End, Exit }

        public ButtonType BType
        {
            get;
            set;
        }

        public bool Clickable
        {
            get
            {
                return this.Enabled;
            }
            set
            {
                if (value != this.Enabled)
                {
                    //Color c = this.BackColor;
                    //this.BackColor = this.ForeColor;
                    //this.ForeColor = c;
                    this.Enabled = value;
                    this.Invalidate();
                    this.Refresh();
                }
            }
        }

        private void SButton_Paint(object sender, PaintEventArgs e)
        {
            DrawButtonImage(e.Graphics);
        }

        private void DrawButtonImage(Graphics g)
        {
            SGraphics.SetGraphicProperies(g);
            sg.Init(g, this.ForeColor, this.BackColor, this.Size);
            using (Bitmap b = new Bitmap(this.Width, this.Height))
            {
                using (Graphics bg = Graphics.FromImage(b))
                {
                    bg.Clear(this.BackColor);
                    Point[] points = null;
                    int offset = 2;
                    int width = this.Width - 2 * offset;
                    int height = this.Height - 2 * offset;
                    SGraphics.SetGraphicProperies(bg);
                    sg.Init(bg, this.ForeColor, this.BackColor, this.Size);
                    switch (BType)
                    {
                        case ButtonType.Text:
                            sg.DrawText(bg, this.Text, this.Size, offset);
                            break;
                        case ButtonType.Up:
                            points = new Point[3];
                            points[0] = new Point(width / 2, 0);
                            points[1] = new Point(width, height);
                            points[2] = new Point(0, height);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            break;
                        case ButtonType.Down:
                            points = new Point[3];
                            points[0] = new Point(0, 0);
                            points[1] = new Point(width, 0);
                            points[2] = new Point(width / 2, height);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            break;
                        case ButtonType.Home:
                            points = new Point[3];
                            points[0] = new Point(width / 2, 0);
                            points[1] = new Point(width, 2 * height / 3);
                            points[2] = new Point(0, 2 * height / 3);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            points = new Point[3];
                            points[0] = new Point(width / 2, height / 3);
                            points[1] = new Point(width, height);
                            points[2] = new Point(0, height);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            break;
                        case ButtonType.End:
                            points = new Point[3];
                            points[0] = new Point(0, height / 3);
                            points[1] = new Point(width, height / 3);
                            points[2] = new Point(width / 2, height);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            points = new Point[3];
                            points[0] = new Point(0, 0);
                            points[1] = new Point(width, 0);
                            points[2] = new Point(width / 2, 2 * height / 3);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            break;
                        case ButtonType.Exit:
                            points = new Point[3];
                            points[0] = new Point(0, height / 2);
                            points[1] = new Point(width, 0);
                            points[2] = new Point(width, height);
                            MovePoints(points, offset);
                            bg.FillPolygon(sg.brush, points);
                            break;
                    }

                }
                if(this.Enabled)
                {
                    g.DrawImage(b, 0, 0, b.Width, b.Height);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(g, b, 0, 0, this.BackColor);
                }
            }            
            
        }

        private static Point[] MovePoints(Point[] p, int offset)
        {
            for (int i = 0; i < p.Length; i++)
            {
                p[i].X += offset;
                p[i].Y += offset;
            }
            return p;
        }

        private void SButton_MouseDown(object sender, MouseEventArgs e)
        {
            this.BorderStyle = BorderStyle.Fixed3D;
        }

        private void SButton_MouseUp(object sender, MouseEventArgs e)
        {
            this.BorderStyle = BorderStyle.None;
        }

        public SGraphics sg = new SGraphics();

        void SButton_Disposed(object sender, EventArgs e)
        {
            if (sg != null)
            {
                sg.Dispose();
                sg = null;
            }
        }

        private void SButton_Load(object sender, EventArgs e)
        {
            LoadColors();
        }
    }
}