using System.Collections.Generic;
using System.Windows.Forms;

namespace CollisionEditor.ViewModel
{
    public static class ViewModelFileService
    {
        public enum Filters { TileMap, AngleMap, WidthMap, HeightMap }

        private static Dictionary<Filters, string> _filters = new Dictionary<Filters, string>()
        {
            [Filters.TileMap]   = "Image Files(*.png)| *.png",
            [Filters.AngleMap]  = "Binary Files(*.bin)| *.bin",
            [Filters.WidthMap]  = "Binary Files(*.bin)| *.bin",
            [Filters.HeightMap] = "Binary Files(*.bin)| *.bin"
        };

        public static string GetFileSavePath(Filters filterID)
        {
            var fileDialog = new SaveFileDialog()
            {
                Filter = _filters[filterID]
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
                return fileDialog.FileName;

            return string.Empty;
        }

        public static string GetFileOpenPath(Filters filterID)
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = _filters[filterID] + "| All files(*.*) | *.*"
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
                return fileDialog.FileName;

            return string.Empty;
        }
    }
}
