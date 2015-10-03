using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MediaPreview
{
    class MFile : IDisposable
    {
        private object lockObj = new object();
        private object lockObjFileCopy = new object();
        private volatile bool inited = false;
        private string file = null;
        private string fileCopy = null;
        private string time = null;
        private string timeShort = string.Empty;
        private Image image = null;
        public enum ImageType { None, Movie, MovieImage, Image, Icon }
        private ImageType imgType = ImageType.None;
        private bool isDir = false;

        public void Update(string file, string time, string timeShort, ImageType imgType) 
        {
            lock (this) 
            {
                this.file = file;
                this.time = time;
                this.timeShort = timeShort;
                this.imgType = imgType;
            }
            lock (lockObjFileCopy)
            {
                this.fileCopy = file;
            }
        }
        
        public void Dispose() 
        {
            Clear();
        }

        private void Clear()
        {
            lock (this.lockObj)
            {
                this.inited = false;
                if (this.image != null)
                {
                    this.image.Dispose();
                    this.image = null;
                }

                file = null;
                time = null;
                timeShort = string.Empty;
                imgType = ImageType.None;
            }
            lock (lockObjFileCopy)
            {
                this.fileCopy = null;
            }
        }

        public void DrawFileText(Graphics g, int x, int y, int w, int h, Font thumbFont, bool addEllipsis)
        {
            string text = string.Empty;
            string fn = string.Empty;
            lock (lockObjFileCopy)
            {
                fn = this.fileCopy;
            }
            if (!string.IsNullOrEmpty(fn))
            {
                try
                {
                    fn = Path.GetFileName(fn);
                }
                catch { }
                if (!string.IsNullOrEmpty(fn))
                {
                    text = fn + " ";
                }
                if (addEllipsis)
                {
                    text += "...";
                }
                else // used as flag for movies 
                {
                    if (!string.IsNullOrEmpty(this.timeShort))
                    {
                        text = timeShort + " " + text;
                    }
                }
            }
            else
            {
                text = string.Empty;
            }
            text = TruncateStr(g, text, thumbFont, w);
            g.DrawString(text, thumbFont, Brushes.DarkGray, x + 1, y + 1);
            g.DrawString(text, thumbFont, Brushes.White, x, y);
        }

        public void Draw(Graphics g, int x, int y, int w, int h, Point rowCols, Font thumbFont) 
        {
            try
            {
                if ((rowCols.X != 1) && (rowCols.Y != 1))
                {
                    if (Program.showImageBorder)
                    {
                        Rectangle br = new Rectangle(x, y, w - 1, h - 1);
                        ControlPaint.DrawBorder(g, br, Color.DimGray, ButtonBorderStyle.Solid);
                        //g.DrawRectangle(Pens.DimGray, x, y, w - 1, h - 1);
                        x += 2;
                        y += 2;
                        w -= 4;
                        h -= 4;
                    }
                    else
                    {
                        x += 1;
                        y += 1;
                        w -= 2;
                        h -= 2;
                    }
                }
                if (isDir) 
                {
                    g.DrawRectangle(Pens.Khaki, new Rectangle(x + 1, y + 1, w - 1, h - 1));
                    g.FillRectangle(Brushes.DarkKhaki, new Rectangle(x + 2 , y + 2, w - 2, h - 2));
                    w -= FolderMargin;
                    h -= FolderMargin;
                    x += FolderMargin / 2;
                    y += FolderMargin / 2;
                }

                string text = string.Empty;
                if (!this.inited)
                {
                    DrawFileText(g, x, y, w, h, thumbFont, true);
                    return;
                }
                lock (this.lockObj)
                {
                    bool hasFile = (this.file != null);
                    try
                    {
                        if (hasFile)
                        {
                            text = Path.GetFileName(this.file);
                        }
                    }
                    catch { }
                    if (this.image == null)
                    {
                        //text = (hasFile ? "<?> " : string.Empty) + text;
                        text = TruncateStr(g, text, thumbFont, w);
                        g.DrawString(text, thumbFont, Brushes.DarkGray, x + 1, y + 1);
                        g.DrawString(text, thumbFont, Brushes.White, x, y);
                        return;
                    }

                    Size ims = Imager.ScaleSize(this.image.Size, new Size(w, h));
                    Rectangle r = new Rectangle(x, y, w, h);
                    if (this.imgType == ImageType.Icon)
                    {
                        ims = Imager.ScaleSize(this.image.Size, new Size((int)Math.Min(96, w), (int)Math.Min(96, h)));
                    }
                    // fit image
                    //if ( this.imgType != ImageType.Movie) //(this.imgType == ImageType.Icon) || (this.imgType == ImageType.Image))
                    //{
                        r = new Rectangle(
                        x + (w - ims.Width) / 2,
                        y + (h - ims.Height) / 2,
                        ims.Width,
                        ims.Height);
                    //}
                    g.DrawImage(this.image, r);
                    if (!string.IsNullOrEmpty(this.timeShort))
                    {
                        text = this.timeShort + " " + text;
                    }
                    text = TruncateStr(g, text, thumbFont, w);
                    g.DrawString(text, thumbFont, Brushes.DarkGray, x + 1, y + 1);
                    g.DrawString(text, thumbFont, Brushes.White, x, y);
                }
            }
            catch { }
        }

        public static string TruncateStr(Graphics g, string s, Font f, int w)
        {
            do
            {
                SizeF ss = g.MeasureString(s, f);
                if (ss.Width < 25)
                {
                    return s;
                }
                if (ss.Width > w)
                {
                    s = s.Substring(0, s.Length - 3);
                }
                else
                {
                    break;
                }
            }
            while (true);
            return s;
        }

        private const int FolderMargin = 12;

        public bool InitImage(Size imageSize, Files.DShouldStop stop) 
        {
            lock (this.lockObj)
            {
                if (this.inited)
                {
                    return (this.image != null);
                }
                if (this.file == null)
                {
                    this.inited = true;
                    return false;
                }
                bool hasImage = false;
                try
                {
                    string workingFile = this.file;
                    this.isDir = Directory.Exists(this.file);
                    if (isDir)
                    {
                        // make smaller
                        imageSize.Width -= FolderMargin;
                        imageSize.Height -= FolderMargin;
                        string dirFile = null;
                        if ((imageSize.Width < 25) || (imageSize.Height < 25))
                        {
                            this.inited = true;
                            return true;
                        }
                        try
                        {
                            string[] df = Directory.GetFiles(this.file);
                            if ((df != null) && (df.Length > 0))
                            {
                                for (int i = 0; i < df.Length; i++) 
                                {
                                    if ((stop != null) && stop())
                                    {
                                        return false;
                                    }
                                    if (!Files.IsSystemHidden(df[i])) 
                                    {
                                        dirFile = df[i];
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                        workingFile = dirFile;
                    }
                    if (workingFile == null)
                    {
                        this.inited = true;
                        return false;
                    }

                    // disk image first
                    if ((imgType == ImageType.Image) || Magic.IsImage(workingFile))
                    {
                        this.image = Imager.GetDiskImage(workingFile, imageSize);
                        this.imgType = ((this.image != null) ? ImageType.Image : ImageType.None);
                    }
                    if (this.image != null)
                    {
                        this.inited = true;
                        return true;
                    }
                    if ((stop != null) && stop()) return false;

                    // is it a movie
                    bool isMovie = true;
                    if (string.IsNullOrEmpty(this.time))
                    {
                        TimeSpan ts = TimeSpan.MaxValue;
                        ts = Imager.GetLength(workingFile);
                        if ((stop != null) && stop()) return false;
                        if (ts != TimeSpan.MaxValue)
                        {
                            this.time = Imager.GetTime(0, 1, ts, false);
                            this.timeShort = Imager.GetTime(0, 1, ts, true);
                        }
                        else
                        {
                            isMovie = false;
                        }
                    }
                    if (isMovie)
                    {
                        this.image = Imager.GetImage(Imager.GetScreenShot(workingFile, this.time), imageSize);
                    }
                    if ((stop != null) && stop()) return false;
                    this.imgType = ((this.image != null) ? this.imgType : ImageType.None);
                    if (this.image != null)
                    {
                        this.inited = true;
                        return true;
                    }

                    //get the icon
                    this.image = Imager.GetIcon(workingFile);
                    this.imgType = ((this.image != null) ? ImageType.Icon : ImageType.None);
                    if (this.image != null)
                    {
                        this.inited = true;
                        return true;
                    }
                }
                catch { }
                finally
                {
                    hasImage = (image != null);
                    this.inited = true;
                }
                return hasImage;
            }
        }

    }//EOC
}
