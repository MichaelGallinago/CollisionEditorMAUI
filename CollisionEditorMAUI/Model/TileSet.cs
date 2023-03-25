using Microsoft.Maui.Controls.Shapes;
using System.Collections.Generic;
using System.IO;

namespace CollisionEditor.Model
{
    public class TileSet
    {
        public readonly Vector2<int> TileSize;

        public List<Bitmap> Tiles { get; private set; }
        public List<byte[]> WidthMap { get; private set; }
        public List<byte[]> HeightMap { get; private set; }

        public TileSet(string path, int tileWidth = 16, int tileHeight = 16,
            Vector2<int> separate = new(), Vector2<int> offset = new())
        {
            TileSize = new Vector2<int>(tileWidth, tileHeight);

            Tiles = new List<Bitmap>();
            WidthMap  = new List<byte[]>();
            HeightMap = new List<byte[]>();

            Image image = new Image
            {
                Source = ImageSource.FromFile(path)
            };
            var bitmap = new Bitmap(image);

            var cellCount = new Vector2<int>(
                ((int)image.Width  - offset.X) / TileSize.X,
                ((int)image.Height - offset.Y) / TileSize.Y);

            for (int y = 0; y < cellCount.Y; y++)
            {
                for (int x = 0; x < cellCount.X; x++)
                {
                    byte[] pixelsColors = new byte[TileSize.X * TileSize.Y * 4];
                    for (int i = 0; i < TileSize.Y; i++)
                    {
                        for (int j = 0; j < TileSize.X; j++)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                pixelsColors[(i * TileSize.X + j) * 4 + k] = bitmap.PixelsColors[
                                    ((offset.Y + i * (TileSize.Y + separate.Y)) * (int)image.Width + (offset.X + j * (TileSize.X + separate.X))) * 4 + k];
                            }
                        }
                    }

                    Tiles.Add(new Bitmap(pixelsColors));
                    if (Tiles.Count == int.MaxValue)
                    {
                        CreateCollisionMap();
                        return;
                    }
                }
            }

            CreateCollisionMap();
        }

        private void CreateCollisionMap()
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                WidthMap.Add(new byte[TileSize.X]);
                HeightMap.Add(new byte[TileSize.Y]);

                for (int x = 0; x < TileSize.X; x++)
                {
                    for (int y = 0; y < TileSize.Y; y++)
                    {
                        if (Tiles[i].GetPixel(x, y).A > 0)
                        {
                            WidthMap[i][y]++;
                            HeightMap[i][x]++;
                        }
                    }
                }
            }
        }

        public TileSet(int angleCount, int tileWidth = 16, int tileHeight = 16)
        {
            TileSize = new Size(tileWidth, tileHeight);

            Tiles = new List<Bitmap>(angleCount);
            WidthMap  = new List<byte[]>(angleCount);
            HeightMap = new List<byte[]>(angleCount);

            for (int i = 0; i < angleCount; i++)
            {
                Tiles.Add(new Bitmap(tileWidth, tileHeight));
                WidthMap.Add(new byte[tileWidth]);
                HeightMap.Add(new byte[tileHeight]);
            }
        }

        public void Save(string path, int columnCount, Size separation = new Size(), Size offset = new Size())
        {
            if (File.Exists(path))
                File.Delete(path);

            Size cell = new Size(TileSize.Width + separation.Width, TileSize.Height + separation.Height);
            int rowCount = (Tiles.Count & -columnCount) / columnCount;

            Size tileMapSize = new Size(
                offset.Width  + columnCount * cell.Width  - separation.Width, 
                offset.Height + rowCount    * cell.Height - separation.Height);

            Bitmap tileMap = DrawTileMap(columnCount, tileMapSize, separation, offset);

            tileMap.Save(path, ImageFormat.Png);
        }

        public void SaveCollisionMap(string path, List<byte[]> collisionMap)
        {
            if (File.Exists(path))
                File.Delete(path);

            using BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew));
                foreach (byte[] values in collisionMap)
                    foreach (byte value in values)
                        writer.Write(value);
        }

        public Bitmap DrawTileMap(int columnCount, Size tileMapSize, Size separation, Size offset)
        {
            Bitmap tileMap = new Bitmap(tileMapSize.Width, tileMapSize.Height);
            using (Graphics graphics = Graphics.FromImage(tileMap))
            {
                Vector2<int> position = new Vector2<int>();
                foreach (Bitmap tile in Tiles)
                {
                    graphics.DrawImage(
                    tile,
                    new Rectangle(
                        offset.Width  + position.X * (TileSize.Width  + separation.Width),
                        offset.Height + position.Y * (TileSize.Height + separation.Height),
                        TileSize.Width, TileSize.Height),
                    new Rectangle(0, 0, TileSize.Width, TileSize.Height),
                    GraphicsUnit.Pixel);

                    if (++position.X >= columnCount)
                    {
                        position.X = 0;
                        position.Y++;
                    }
                }
            }
            return tileMap;
        }

        public Bitmap SetTile(int tileIndex, Bitmap tile)
        {
            return Tiles[tileIndex] = tile;
        }
    }
}
