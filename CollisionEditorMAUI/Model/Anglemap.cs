namespace CollisionEditor.Model
{
    public class AngleMap
    {
        public List<byte> Values { get; private set; }

        public AngleMap(string path)
        {
            BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
            Values = reader.ReadBytes((int)Math.Min(int.MaxValue, reader.BaseStream.Length)).ToList();
        }

        public AngleMap(int tileCount)
        {
            Values = new List<byte>(new byte[tileCount]);
        }

        public void Save(string path)
        {
            if (File.Exists(path)) 
                File.Delete(path);

            using BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew));
                foreach (byte value in Values)
                    writer.Write(value);
        }

        public byte SetAngleWithLine(int tileIndex, Vector2<int> positionGreen, Vector2<int> positionBlue)
        {
            return Values[tileIndex] = (byte)(Math.Atan2(positionBlue.Y - positionGreen.Y, positionBlue.X - positionGreen.X) * 128 / Math.PI);
        }

        public byte SetAngle(int tileIndex, byte value)
        {
            return Values[tileIndex] = value;
        }

        public byte ChangeAngle(int tileIndex, int value)
        {
            return Values[tileIndex] = (byte)(Values[tileIndex] + value);
        }
    }
}
