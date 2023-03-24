using System.Globalization;
using System.Text;

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
                Vector2<int> size = tileSet.TileSize;
                for (int i = tileSet.Tiles.Count; i < angleMap.Values.Count; i++)
                {
                    tileSet.Tiles.Add(new Bitmap(size.X, size.Y));
                    tileSet.WidthMap.Add(new byte[size.X]);
                    tileSet.HeightMap.Add(new byte[size.Y]);
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
