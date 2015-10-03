using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Simplicity.SControls
{
    public class SGraphics : IDisposable
    {
        public Pen drawPen = null;
        public Brush brush = null;
        public Font font = null;
        public Pen borderPen = null;
        public Brush backBrush = null;

        private Color lfc = SystemColors.ControlText;
        private Color lbc = SystemColors.Control;
        private Size ls = new Size(0, 0);

        public void Init(Graphics g, Color fc, Color bc, Size s)
        {
            Config.Default.Load();
            if ((drawPen == null) || (!fc.Equals(lfc)))
            {
                if (drawPen != null)
                {
                    drawPen.Dispose();
                    drawPen = null;
                }
                drawPen = new Pen(fc, s.Width / 10);
            }
            if ((borderPen == null) || (!fc.Equals(lfc)))
            {
                if (borderPen != null)
                {
                    borderPen.Dispose();
                    borderPen = null;
                }
                borderPen = new Pen(fc, 4);
            }
            if ((brush == null) || (!fc.Equals(lfc)))
            {
                if (brush != null)
                {
                    brush.Dispose();
                    brush = null;
                }
                brush = new SolidBrush(fc);
            }
            if ((backBrush == null) || (!bc.Equals(lbc)))
            {
                if (backBrush != null)
                {
                    backBrush.Dispose();
                    backBrush = null;
                }
                backBrush = new SolidBrush(bc);
            }
            if ((g != null) && ((font == null) || (!s.Equals(ls))))
            {
                int maxFontHeight = s.Height;
                float fsize = (maxFontHeight / 2) * 2;
                if (fsize > 72) fsize = 72;
                while (true)
                {
                    if (font != null)
                    {
                        font.Dispose();
                        font = null;
                    }
                    try
                    {
                        font = new Font(Config.Default.fontFamily, fsize, FontStyle.Bold);
                    }
                    catch
                    {
                        font = new Font("Arial", fsize, FontStyle.Bold);
                    }
                    SizeF sf = g.MeasureString("WjM", this.font);
                    if (sf.Height < maxFontHeight)
                    {
                        break;
                    }
                    fsize -= 2;
                    if (fsize < 10)
                    {
                        fsize = 10;
                        break;
                    }
                }
            }
            lfc = fc;
            lbc = bc;
            ls = s;
        }

        public SizeF DrawText(Graphics g, string s, Size ss, float x)
        {
            if (string.IsNullOrEmpty(s)) return new SizeF(0, 0);
            SizeF sf = g.MeasureString(s, this.font);
            g.DrawString(s, font, brush, new PointF(x, (ss.Height - sf.Height) / 2));
            return sf;
        }

        public void DrawBorder(Graphics g, Size ss)
        {
            Rectangle r = new Rectangle(2, 2, ss.Width - 4, ss.Height - 4);
            g.DrawRectangle(this.borderPen, r);
        }

        public static void DrawFolderIcon(Graphics g, Size ss)
        {
            Config.Default.Load();
            using (Brush b = new SolidBrush(Config.Default.dirColor))
            {
                g.FillEllipse(b, 2, 2, ss.Width - 4, ss.Height - 4);
            }
        }

        public static void DrawOptionIcon(Graphics g, Size ss)
        {
            Config.Default.Load();
            using (Brush b = new SolidBrush(Config.Default.specialColor))
            {
                g.FillEllipse(b, 2, 2, ss.Width - 4, ss.Height - 4);
            }
        }

        public static void SetGraphicProperies(Graphics g)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
        }

        public void Dispose()
        {
            if (drawPen != null)
            {
                drawPen.Dispose();
                drawPen = null;
            }
            if (borderPen != null)
            {
                borderPen.Dispose();
                borderPen = null;
            }
            if (brush != null)
            {
                brush.Dispose();
                brush = null;
            }
            if (backBrush != null)
            {
                backBrush.Dispose();
                backBrush = null;
            }
            if (font != null)
            {
                font.Dispose();
                font = null;
            }
        }
    }
}