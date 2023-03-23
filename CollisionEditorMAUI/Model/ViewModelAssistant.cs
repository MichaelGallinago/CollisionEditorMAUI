using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Globalization;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System;

namespace CollisionEditor.Model
{
    internal static class ViewModelAssistant
    {
        public static (byte byteAngle, string hexAngle, double fullAngle) GetAngles(AngleMap angleMap, uint chosenTile)
        {
            byte angle = angleMap.Values[(int)chosenTile];
            return (angle, GetHexAngle(angle), GetFullAngle(angle));
        }

        public static string GetCollisionValues(byte[] collisionArray)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte value in collisionArray)
                builder.Append((char)(value + (value < 10 ? 48 : 55)));

            return string.Join(" ", builder.ToString().ToCharArray());
        }

        public static BitmapSource BitmapConvert(Bitmap bitmap, double dpi = 0.1)
        {
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, dpi, dpi,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        public static string GetHexAngle(byte angle)
        {
            return "0x" + string.Format("{0:X}", angle).PadLeft(2, '0');
        }

        public static double GetFullAngle(byte angle)
        {
            return Math.Round((256 - angle) * 1.40625, 1);
        }

        public static byte GetByteAngle(string hexAngle)
        {
            return byte.Parse(hexAngle.Substring(2), NumberStyles.HexNumber);
        }

        public static void SupplementElements(AngleMap angleMap, TileSet tileSet)
        {
            if (tileSet.Tiles.Count < angleMap.Values.Count)
            {
                Size size = tileSet.TileSize;
                for (int i = tileSet.Tiles.Count; i < angleMap.Values.Count; i++)
                {
                    tileSet.Tiles.Add(new Bitmap(size.Width, size.Height));
                    tileSet.WidthMap.Add(new byte[size.Width]);
                    tileSet.HeightMap.Add(new byte[size.Height]);
                }
            }
            else
            {
                for (int i = angleMap.Values.Count; i < tileSet.Tiles.Count; i++)
                {
                    angleMap.Values.Add(0);
                }
            }
        }
    }
}
