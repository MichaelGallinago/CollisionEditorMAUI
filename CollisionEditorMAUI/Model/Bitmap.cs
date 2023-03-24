namespace CollisionEditor.Model
{
    public class Bitmap
    {
        public byte[] PixelsColors { get; }

        public Bitmap(byte[] pixelsColors)
        {
            PixelsColors = pixelsColors;
        }

        public Bitmap(Image image)
        {
            StreamImageSource source = (StreamImageSource)image.Source;

            var streamFromImageSource = source.Stream(CancellationToken.None).GetAwaiter().GetResult();

            using var memoryStream = new MemoryStream();
            streamFromImageSource.CopyTo(memoryStream);

            PixelsColors = memoryStream.ToArray();
        }

        public Image GetImage()
        {
            Image image = new Image()
            {
                Source = ImageSource.FromStream(() => new MemoryStream(PixelsColors))
            };

            return image;
        }
    }
}
