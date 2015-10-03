using System.Collections;
using System.Drawing;
using System.IO;
using System;

namespace Simplicity.SControls
{

    public class FItemData : SList.ItemData
    {
        public FItemData() : base(string.Empty) { }

        public FItemData(string text) : base(text, null) { }

        public FItemData(string text, object tag) : base(text, tag) { }

        public bool IsUpFolder { get; set; }

        public bool IsFolder { get; set; }

        public bool IsSpecial { get; set; }
    }//EIOC
    
    class FItemImageResolver : SList.ItemImageResolver
    {
        public delegate string DItemPathResolver(FItemData it);
        private Bitmap folderImage = null;
        private Bitmap specialImage = null;
        private Hashtable bitmaps = null;
        private const int folderImgSize = 48;

        public DItemPathResolver ItemPathResolver
        {
            get; set;
        }

        public FItemImageResolver(DItemPathResolver itemPathResolver)
        {
            this.ItemPathResolver = itemPathResolver;
        }

        public override System.Drawing.Image GetImage(SList.ItemData data)
        {
            FItemData it = data as FItemData;
            if (it == null) return base.GetImage(data);
            if (it.IsSpecial)
            {
                return GetSpecialImage();
            }
            if (it.IsFolder)
            {
                return GetFolderImage();
            }
            return GetFileImage(it);
        }

        public override void Dispose()
        {
            ItemPathResolver = null;
            if (folderImage != null)
            {
                folderImage.Dispose();
                folderImage = null;
            }
            if (specialImage != null)
            {
                specialImage.Dispose();
                specialImage = null;
            }
            ClearFileBitmaps();
        }

        public void ClearFileBitmaps()
        {
            try
            {
                if (bitmaps != null)
                {
                    foreach (string key in bitmaps.Keys)
                    {
                        Bitmap b = (Bitmap)bitmaps[key];
                        if (b != null)
                        {
                            b.Dispose();
                            b = null;
                        }
                    }
                    bitmaps = null;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private Image GetFolderImage()
        {
            if (folderImage == null)
            {
                folderImage = new Bitmap(folderImgSize, folderImgSize);
                using (Graphics g = Graphics.FromImage(folderImage))
                {
                    SGraphics.SetGraphicProperies(g);
                    SGraphics.DrawFolderIcon(g, folderImage.Size);
                }
            }
            return (Image)folderImage.Clone();
        }

        private Image GetSpecialImage()
        {
            if (specialImage == null)
            {
                specialImage = new Bitmap(folderImgSize, folderImgSize);
                using (Graphics g = Graphics.FromImage(specialImage))
                {
                    SGraphics.SetGraphicProperies(g);
                    SGraphics.DrawOptionIcon(g, specialImage.Size);
                }
            }
            return (Image)specialImage.Clone();
        }

        private Image GetFileImage(FItemData it)
        {
            string path = it.Text;
            if (this.ItemPathResolver != null)
            {
                path = ItemPathResolver(it);
            }
            string ext = Path.GetExtension(path).ToUpper();
            if (string.IsNullOrEmpty(ext))
            {
                ext = "?";
            }
            switch (ext)
            {
                case ".EXE":
                case ".ICO":
                    using (Icon c = BlackFox.Win32.Icons.IconFromShell(path, BlackFox.Win32.Icons.SystemIconSize.Large))
                    {
                        if (c != null)
                        {
                            try
                            {
                                Bitmap b = c.ToBitmap();
                                return b;
                            }
                            catch (Exception ex) { Config.Default.Error(ex); }
                        }
                    }
                    break;
            }
            return GetExtImage(ext);
        }

        private Image GetExtImage(string ext)
        {
            if (ext == null) return null;
            if (bitmaps == null) bitmaps = new Hashtable();
            if (bitmaps[ext] == null)
            {
                using (Icon c = BlackFox.Win32.Icons.IconFromExtensionShell(ext, BlackFox.Win32.Icons.SystemIconSize.Large))
                {
                    if (c != null)
                    {
                        try
                        {
                            Bitmap b = c.ToBitmap();
                            bitmaps[ext] = b;
                        }
                        catch (Exception ex) { Config.Default.Error(ex); }
                    }
                }
            }
            Bitmap bmp = (Bitmap)bitmaps[ext];
            if (bmp == null) return null;
            return (Image)bmp.Clone();
        }


    }//EOC

}