using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaPreview
{
    class Magic
    {
        public enum Types { Unknown, BMP, GIF, JPG, PNG, TIFF, ICO, WMF, EMF }
        public readonly static int MAX_SIG_LEN = 4;
       
        public class Type
        {
            public byte[] sig = null;
            public string name = null;
            public Types type = Types.Unknown;
        }

        public static bool IsImage(string file)
        {
            try
            {
                byte[] buf = GetFileSigBytes(file);
                return IsImage(buf);
            }
            catch { }
            return false;
        }

        private static byte[] GetFileSigBytes(string file)
        {
            byte[] buf = null;
            FileStream fs = null;
            if (file == null) return null;
            try
            {
                fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                buf = new byte[Magic.MAX_SIG_LEN];
                if (fs.Read(buf, 0, buf.Length) != buf.Length)
                {
                    return null;
                }
                fs.Close();
                fs = null;
            }
            catch { }
            finally
            {
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                    }
                    catch { }
                    fs = null;
                }
            }
            return buf;
        }

        public static bool IsImage(byte[] b)
        {
            if ((b == null) || (b.Length < MAX_SIG_LEN)) return false;
            Init();
            for (int i = 0; i < types.Length; i++)
            {
                if (MatchesType(b, types[i])) return true;
            }
            return false;
        }

        private static bool MatchesType(byte[] b, Type t)
        {
            if (t == null) return false;
            if (b == null) return false;
            if (b.Length >= MAX_SIG_LEN)
            {
                int j = 0;
                for (; j < t.sig.Length; j++)
                {
                    if (b[j] != t.sig[j]) break;
                }
                if (j == t.sig.Length) return true;
            }
            return false;
        }

        private static void Init()
        {
            lock (typesLock)
            {
                if ((types == null) || (types.Length < 10)
                    || (types[0] == null)
                    || (types[1] == null))
                {
                    types = new Type[10];
                    Type t = null;

                    t = new Type();
                    t.sig = new byte[] { 0x42, 0x4D };
                    t.type = Types.BMP;
                    types[0] = t;

                    t = new Type();
                    t.sig = new byte[] { 0x47, 0x49, 0x46, 0x38 };
                    t.type = Types.GIF;
                    types[1] = t;

                    t = new Type();
                    t.sig = new byte[] { 0xFF, 0xD8, 0xFF };
                    t.type = Types.JPG;
                    types[2] = t;

                    t = new Type();
                    t.sig = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
                    t.type = Types.PNG;
                    types[3] = t;

                    t = new Type();
                    t.sig = new byte[] { 0x4D, 0x4D, 0x00, 0x2A };
                    t.type = Types.TIFF;
                    types[4] = t;

                    t = new Type();
                    t.sig = new byte[] { 0x49, 0x49, 0x2A, 0x00 };
                    t.type = Types.TIFF;
                    types[5] = t;

                    t = new Type();
                    t.sig = new byte[] { 0x00, 0x00, 0x01, 0x00 };
                    t.type = Types.ICO;
                    types[6] = t;

                    t = new Type();
                    t.sig = new byte[] { 0xD7, 0xCD, 0xC6, 0x9A };
                    t.type = Types.WMF;
                    types[7] = t;

                    t = new Type();
                    t.sig = new byte[] { 0x01, 0x00, 0x00, 0x00 };
                    t.type = Types.EMF;
                    types[8] = t;
                }
            }
        }

        private static Type[] types = null;
        private static object typesLock = new object();
    }
}
