using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MediaPreview
{
    class AutoRotate
    {
        public static readonly int Orientation = 0x0112;
        public static readonly int ThumbnailOrientation = 0x5029;

        internal static System.Drawing.RotateFlipType GetThumbAutoRotate(System.Drawing.Imaging.PropertyItem[] pi)
        {
            System.Drawing.RotateFlipType r = System.Drawing.RotateFlipType.RotateNoneFlipNone;
            System.Drawing.Imaging.PropertyItem imgProp = null;
            System.Drawing.Imaging.PropertyItem thumbProp = null;
            try
            {
                if (pi == null) return r;
                if ((pi != null) && (pi.Length > 0))
                {
                    for (int i = 0; i < pi.Length; i++)
                    {
                        if (pi[i].Id == Orientation)
                        {
                            imgProp = pi[i];
                        }
                        if (pi[i].Id == ThumbnailOrientation)
                        {
                            thumbProp = pi[i];
                        }
                    }
                    if (thumbProp != null)
                    {
                        r = GetAutoRotate(thumbProp);
                    }
                    else if (imgProp != null)
                    {
                        r = GetAutoRotate(imgProp);
                    }
                }
            }
            catch {  }
            return r;
        }

        private static System.Drawing.RotateFlipType GetAutoRotate(System.Drawing.Imaging.PropertyItem pi)
        {
            System.Drawing.RotateFlipType r = System.Drawing.RotateFlipType.RotateNoneFlipNone;
            if ((pi != null)
                && ((pi.Id == Orientation)
                || (pi.Id == ThumbnailOrientation))
                )
            {
                Array result = ExifConvert.FromPropertyItem(pi);
                foreach (object pVal in result)
                {
                    if (pVal is UInt16)
                    {
                        UInt16 exifRot = (UInt16)pVal;
                        r = GetAutoRotate(exifRot);
                        return r;
                    }
                }
            }
            return r;
        }

        private static System.Drawing.RotateFlipType GetAutoRotate(UInt16 exifRot)
        {
            System.Drawing.RotateFlipType r = System.Drawing.RotateFlipType.RotateNoneFlipNone;
            switch (exifRot)
            {
                case 2: r = System.Drawing.RotateFlipType.RotateNoneFlipX; break; // = Mirror horizontal 			
                case 3: r = System.Drawing.RotateFlipType.Rotate180FlipNone; break;
                case 4: r = System.Drawing.RotateFlipType.RotateNoneFlipY; break;
                case 5: r = System.Drawing.RotateFlipType.Rotate270FlipX; break;
                case 6: r = System.Drawing.RotateFlipType.Rotate90FlipNone; break;
                case 7: r = System.Drawing.RotateFlipType.Rotate90FlipX; break;
                case 8: r = System.Drawing.RotateFlipType.Rotate270FlipNone; break;
            }
            return r;
        }

        /////////////////////////////////////

        public enum ExifPropertyType
        {
            /// <summary>
            /// Specifies that the value data member is an array of bytes.
            /// </summary>
            Byte = 1,

            /// <summary>
            /// Specifies that the value data member is a null-terminated ASCII string.
            /// </summary>
            /// <remarks>If you set <see cref="PropertyItem.Type">PropertyItem.Type</see> to <see cref="PropertyItem.Type"/>, you should set the length data member to the length of the string including the NULL terminator. For example, the string HELLO would have a length of 6.</remarks>
            Ascii = 2,

            /// <summary>
            /// Specifies that the value data member is an array of signed short (16-bit) integers.
            /// </summary>
            UInt16 = 3,

            /// <summary>
            /// Specifies that the value data member is an array of unsigned long (32-bit) integers.
            /// </summary>
            UInt32 = 4,

            /// <summary>
            /// Specifies that the value data member is an array of pairs of unsigned long integers. Each pair represents a fraction; the first integer is the numerator and the second integer is the denominator.
            /// </summary>
            URational = 5,

            /// <summary>
            /// Specifies that the value data member is an array of bytes that can hold values of any data type.
            /// </summary>
            Raw = 7,

            /// <summary>
            /// Specifies that the value data member is an array of signed long (32-bit) integers.
            /// </summary>
            Int32 = 9,

            /// <summary>
            /// Specifies that the value data member is an array of pairs of signed long integers. Each pair represents a fraction; the first integer is the numerator and the second integer is the denominator.
            /// </summary>
            Rational = 10,
        } // EOC

        public sealed class ExifConvert
        {
            private ExifConvert() { }

            /// <summary>
            /// Converts a property item to an array of objects.
            /// </summary>
            /// <param name="propertyItem">The property item to convert.</param>
            /// <returns>An array of <see cref="object"/> items.</returns>
            public static Array FromPropertyItem(PropertyItem propertyItem)
            {
                ExifPropertyType type = (ExifPropertyType)propertyItem.Type;

                switch (type)
                {
                    case ExifPropertyType.Raw:
                        // The value represents raw data (a single byte[] value)
                        return new byte[][] { propertyItem.Value };

                    case ExifPropertyType.Ascii:
                        // The value represents an array of strings separated by \0 characters
                        string stringValue = Encoding.ASCII.GetString(propertyItem.Value, 0, propertyItem.Len - 1);
                        return stringValue.Split('\0');

                    case ExifPropertyType.Byte:
                        // The value represents an array of bytes
                        return propertyItem.Value;

                    case ExifPropertyType.UInt16:
                        // The value represents an array of unsigned 16-bit integers.
                        int ushortCount = propertyItem.Len / ushortSize;

                        ushort[] ushortResult = new ushort[ushortCount];
                        for (int i = 0; i < ushortCount; i++)
                            ushortResult[i] = ReadUInt16(propertyItem.Value, i * ushortSize);
                        return ushortResult;

                    case ExifPropertyType.Int32:
                        // The value represents an array of signed 32-bit integers.
                        int intCount = propertyItem.Len / intSize;

                        int[] intResult = new int[intCount];
                        for (int i = 0; i < intCount; i++)
                            intResult[i] = ReadInt32(propertyItem.Value, i * intSize);
                        return intResult;

                    case ExifPropertyType.UInt32:
                        // The value represents an array of unsigned 32-bit integers.
                        int uintCount = propertyItem.Len / uintSize;

                        uint[] uintResult = new uint[uintCount];
                        for (int i = 0; i < uintCount; i++)
                            uintResult[i] = ReadUInt32(propertyItem.Value, i * uintSize);
                        return uintResult;

                    case ExifPropertyType.Rational:
                    case ExifPropertyType.URational:
                    default:
                        return new byte[][] { propertyItem.Value };
                }
            }

            #region Static Fields

            private static readonly int ushortSize = Marshal.SizeOf(typeof(ushort));
            private static readonly int intSize = Marshal.SizeOf(typeof(int));
            private static readonly int uintSize = Marshal.SizeOf(typeof(uint));
            private static readonly int rationalSize = 2 * Marshal.SizeOf(typeof(int));
            private static readonly int urationalSize = 2 * Marshal.SizeOf(typeof(uint));

            #endregion

            #region Private Helpers

            public static ushort ReadUInt16(byte[] buffer, int offset)
            {
                return (ushort)(
                    ((ushort)buffer[offset] +
                    ((ushort)buffer[offset + 1] << 8)));
            }

            private static int ReadInt32(byte[] buffer, int offset)
            {
                return (int)(
                    ((uint)buffer[offset] +
                    ((uint)buffer[offset + 1] << 8) +
                    ((uint)buffer[offset + 2] << 16) +
                    ((int)buffer[offset + 3] << 24)));
            }

            private static uint ReadUInt32(byte[] buffer, int offset)
            {
                return (uint)(
                    ((uint)buffer[offset] +
                    ((uint)buffer[offset + 1] << 8) +
                    ((uint)buffer[offset + 2] << 16) +
                    ((uint)buffer[offset + 3] << 24)));
            }

            #endregion
        }


    }//EOC
}
