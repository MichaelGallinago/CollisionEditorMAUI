using CollisionEditor.Model;
using CollisionEditor.View;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.IO;
using System;

namespace CollisionEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {   
        public AngleMap AngleMap { get; private set; }
        public TileSet TileSet { get; private set; }
        
        public ICommand MenuOpenAngleMapCommand { get; }
        public ICommand MenuOpenTileMapCommand { get; }
        public ICommand MenuSaveTileMapCommand { get; }
        public ICommand MenuSaveWidthMapCommand { get; }
        public ICommand MenuSaveHeightMapCommand { get; }
        public ICommand MenuSaveAngleMapCommand { get; }
        public ICommand MenuSaveAllCommand { get; }
        public ICommand MenuUnloadTileMapCommand { get; }
        public ICommand MenuUnloadAngleMapCommand { get; }
        public ICommand MenuUnloadAllCommand { get; }
        public ICommand SelectTileCommand { get; }
        public ICommand AngleIncrementCommand { get; }
        public ICommand AngleDecrementCommand { get; }
        public ICommand ExitAppCommand { get; }

        public byte ByteAngle
        {
            get => byteAngle;
            set
            {   
                byteAngle = value;
                ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

                window.DrawRedLine();
            }
        }

        public string HexAngle
        {
            get => hexAngle;
            set
            {
                hexAngle = value;

                if (hexAngle.Length != 4 || hexAngle[0] != '0' || hexAngle[1] != 'x'
                    || !hexadecimalAlphabet.Contains(hexAngle[2]) || !hexadecimalAlphabet.Contains(hexAngle[3]))
                {
                    textboxValidator.AddError(nameof(HexAngle), "Error! Wrong hexadecimal number");
                    return;
                }

                textboxValidator.ClearErrors(nameof(HexAngle));

                (byte byteAngle, string hexAngle, double fullAngle) angles = ViewModelAngleService.GetAngles(hexAngle);
                ByteAngle = angles.byteAngle;
                AngleMap.SetAngle((int)ChosenTile, angles.byteAngle);
                window.DrawRedLine();
            }
        }

        public uint ChosenTile
        {
            get => chosenTile;
            set
            {   
                chosenTile = value;
            }
        }

        private const string hexadecimalAlphabet = "0123456789ABCDEF";
        private const int tileMapSeparation = 4;
        private const int tileMapTileScale = 2;

        private MainWindow window;
        private byte byteAngle;
        private string hexAngle;
        private uint chosenTile;
        private TextboxValidator textboxValidator;

        public MainViewModel(MainWindow window)
        {
            textboxValidator = new TextboxValidator();
            textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
            AngleMap = new AngleMap(0);
            TileSet  = new TileSet(0);
            chosenTile = 0;
            byteAngle = 0;
            hexAngle = "0x00";
            this.window = window;

            MenuOpenAngleMapCommand = new RelayCommand(MenuOpenAngleMap);
            MenuOpenTileMapCommand  = new RelayCommand(MenuOpenTileMap);

            MenuSaveTileMapCommand   = new RelayCommand(MenuSaveTileMap);
            MenuSaveWidthMapCommand  = new RelayCommand(MenuSaveWidthMap);
            MenuSaveHeightMapCommand = new RelayCommand(MenuSaveHeightMap);
            MenuSaveAngleMapCommand  = new RelayCommand(MenuSaveAngleMap);
            MenuSaveAllCommand       = new RelayCommand(MenuSaveAll);

            MenuUnloadTileMapCommand  = new RelayCommand(MenuUnloadTileMap);
            MenuUnloadAngleMapCommand = new RelayCommand(MenuUnloadAngleMap);
            MenuUnloadAllCommand      = new RelayCommand(MenuUnloadAll);

            AngleIncrementCommand = new RelayCommand(AngleIncrement);
            AngleDecrementCommand = new RelayCommand(AngleDecrement);
            SelectTileCommand     = new RelayCommand(SelectTile);

            ExitAppCommand = new RelayCommand(ExitApp);

            RectanglesGridUpdate();
            TileGridUpdate(TileSet, (int)chosenTile, window);
        }

        private void MenuOpenAngleMap()
        {
            string filePath = ViewModelFileService.GetFileOpenPath(ViewModelFileService.Filters.AngleMap);
            if (filePath != string.Empty)
            {   
                AngleMap = new AngleMap(filePath);
                if (TileSet is null)
                {
                    TileSet = new TileSet(AngleMap.Values.Count);
                }
                
                ViewModelAssistant.SupplementElements(AngleMap, TileSet);

                ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
                window.SelectTileTextBox.IsEnabled = true;
                window.SelectTileButton.IsEnabled  = true;
                window.TextBoxByteAngle.IsEnabled  = true;
                window.TextBoxHexAngle.IsEnabled   = true;

                TileMapGridUpdate(TileSet.Tiles.Count);
                window.DrawRedLine();
            }
        }

        public void ShowAngles((byte byteAngle, string hexAngle, double fullAngle) angles)
        {
            
            byteAngle = angles.byteAngle;
            OnPropertyChanged(nameof(ByteAngle));
            window.ByteAngleIncrimentButton.IsEnabled = true;
            window.ByteAngleDecrementButton.IsEnabled = true;

            hexAngle = angles.hexAngle;
            OnPropertyChanged(nameof(HexAngle));
            window.HexAngleIncrimentButton.IsEnabled = true;
            window.HexAngleDecrementButton.IsEnabled = true;

            window.TextBlockFullAngle.Text = angles.fullAngle.ToString() + "°";
        }

        private void MenuOpenTileMap()
        {
            string filePath = ViewModelFileService.GetFileOpenPath(ViewModelFileService.Filters.TileMap);
            if (filePath != string.Empty)
            {
                TileSet = new TileSet(filePath);
                if (AngleMap is null)
                {
                    AngleMap = new AngleMap(TileSet.Tiles.Count);
                }

                ViewModelAssistant.SupplementElements(AngleMap,TileSet);

                ViewModelAssistant.BitmapConvert(TileSet.Tiles[(int)chosenTile]);
                TileGridUpdate(TileSet, (int)ChosenTile, window);
                RectanglesGridUpdate();
                window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
                window.Widths.Text  = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

                ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
                window.TextBoxByteAngle.IsEnabled = true;
                window.TextBoxHexAngle.IsEnabled  = true;

                window.TileMapGrid.Children.Clear();

                foreach (Bitmap tile in TileSet.Tiles)
                {
                    var image = new System.Windows.Controls.Image()
                    {
                        Width  = TileSet.TileSize.Width  * 2,
                        Height = TileSet.TileSize.Height * 2
                    };
                    image.Source = ViewModelAssistant.BitmapConvert(tile);
                    window.TileMapGrid.Children.Add(image);
                }

                window.SelectTileTextBox.IsEnabled = true;
                window.SelectTileButton.IsEnabled = true;

                TileMapGridUpdate(TileSet.Tiles.Count);
                window.DrawRedLine();
            }
        }

        public void TileMapGridUpdate(int tileCount)
        {
            window.TileMapGrid.Height = (int)Math.Ceiling((double)tileCount / window.TileMapGrid.Columns) * (TileSet.TileSize.Height * tileMapTileScale + tileMapSeparation);
        }

        private void MenuSaveTileMap()
        {
            if (TileSet.Tiles.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: You haven't chosen TileMap to save");
                return;
            }

            string filePath = ViewModelFileService.GetFileSavePath(ViewModelFileService.Filters.TileMap);
            if (filePath != string.Empty)
            {
                TileSet.Save(Path.GetFullPath(filePath), 16);
            }
        }

        private void MenuSaveWidthMap()
        {
            if (TileSet.Tiles.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: The WidthMap isn't generated!");
                return;
            }

            string filePath = ViewModelFileService.GetFileSavePath(ViewModelFileService.Filters.WidthMap);
            if (filePath != string.Empty)
            {
                TileSet.SaveCollisionMap(Path.GetFullPath(filePath), TileSet.WidthMap);
            }
        }

        private void MenuSaveHeightMap()
        {
            if (TileSet.Tiles.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: The HeightMap isn't generated!");
                return;
            }

            string filePath = ViewModelFileService.GetFileSavePath(ViewModelFileService.Filters.HeightMap);
            if (filePath != string.Empty)
            {
                TileSet.SaveCollisionMap(Path.GetFullPath(filePath), TileSet.HeightMap);
            }
        }

        private void MenuSaveAngleMap()
        {
            if (AngleMap.Values.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: You haven't chosen AngleMap to save");
                return;
            }

            string filePath = ViewModelFileService.GetFileSavePath(ViewModelFileService.Filters.AngleMap);
            if (filePath != string.Empty)
            {
                AngleMap.Save(Path.GetFullPath(filePath));
            }
        }

        private void MenuSaveAll()
        {
            MenuSaveAngleMap();
            MenuSaveHeightMap();
            MenuSaveWidthMap();
            MenuSaveTileMap();
        }

        private void MenuUnloadTileMap()
        {
            window.TileMapGrid.Children.Clear();
            TileSet = new TileSet(AngleMap.Values.Count);

            TileGridUpdate(TileSet, (int)ChosenTile, window);
            window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
            window.Widths.Text = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        }

        private void MenuUnloadAngleMap()
        {
            AngleMap = new AngleMap(TileSet.Tiles.Count);
            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        }

        private void MenuUnloadAll()
        {
            window.TileMapGrid.Children.Clear();
            TileSet = new TileSet(0);
            AngleMap = new AngleMap(0);

            window.Heights.Text = null;
            window.Widths.Text = null;
            ShowAngles((0, "0x00", 0));
            window.SelectTileTextBox.Text = "0";
            window.ByteAngleIncrimentButton.IsEnabled = false;
            window.ByteAngleDecrementButton.IsEnabled = false;
            window.HexAngleIncrimentButton.IsEnabled = false;
            window.HexAngleDecrementButton.IsEnabled = false;
            window.SelectTileTextBox.IsEnabled = false;
            window.SelectTileButton.IsEnabled = false;
            window.TextBoxByteAngle.IsEnabled = false;
            window.TextBoxHexAngle.IsEnabled = false;
            window.canvasForLine.Children.Clear();
            window.RectanglesGrid.Children.Clear();
        }

        private void AngleIncrement()
        {
            byte byteAngle = AngleMap.ChangeAngle((int)chosenTile, 1);

            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

            window.DrawRedLine();
        }

        private void AngleDecrement()
        {
            byte byteAngle = AngleMap.ChangeAngle((int)chosenTile, -1);

            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

            window.DrawRedLine();
        }

        public void SelectTile()
        {
            if (chosenTile > TileSet.Tiles.Count - 1)
            {
                chosenTile = (uint)TileSet.Tiles.Count - 1;
                OnPropertyChanged(nameof(ChosenTile));
            }

            System.Windows.Controls.Image lastTile = GetTile(window.LastChosenTile);

            window.TileMapGrid.Children.RemoveAt(window.LastChosenTile);
            window.TileMapGrid.Children.Insert(window.LastChosenTile, lastTile);

            System.Windows.Controls.Image newTile = GetTile((int)chosenTile);

            Border border = new Border()
            {
                Width = TileSet.TileSize.Width * tileMapTileScale + tileMapSeparation,
                Height = TileSet.TileSize.Height * tileMapTileScale + tileMapSeparation,
                BorderBrush = new SolidColorBrush(Colors.Red),
                BorderThickness = new Thickness(2),
                Child = newTile
            };

            window.TileMapGrid.Children.RemoveAt((int)chosenTile);
            window.TileMapGrid.Children.Insert((int)chosenTile, border);

            window.LastChosenTile = (int)chosenTile;
            TileGridUpdate(TileSet, (int)ChosenTile, window);
            window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
            window.Widths.Text  = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);
            
            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));

            window.DrawRedLine();
            window.RectanglesGrid.Children.Clear();
        }

        public void SelectTileFromTileMap()
        {
            OnPropertyChanged(nameof(ChosenTile));
            TileGridUpdate(TileSet, (int)ChosenTile, window);
            window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
            window.Widths.Text = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));

            window.DrawRedLine();
            window.RectanglesGrid.Children.Clear();
        }

        internal System.Windows.Controls.Image GetTile(int index)
        {
            Bitmap tile = TileSet.Tiles[index];
            var image = new System.Windows.Controls.Image()
            {
                Width = TileSet.TileSize.Width * 2,
                Height = TileSet.TileSize.Height * 2
            };
            image.Source = ViewModelAssistant.BitmapConvert(tile);
            return image;
        }

        private void ExitApp()
        {
            window.Close();
        }

        public void UpdateAngles(Vector2<int> positionGreen, Vector2<int> positionBlue)
        {
            if (AngleMap.Values.Count == 0)
                return;

            byte byteAngle = AngleMap.SetAngleWithLine((int)chosenTile, positionGreen, positionBlue);

            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RectanglesGridUpdate()
        {
            window.RectanglesGrid.ColumnDefinitions.Clear();
            window.RectanglesGrid.RowDefinitions.Clear();

            var size = TileSet.TileSize;

            for (int x = 0; x < size.Width; x++)
                window.RectanglesGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int y = 0; y < size.Height; y++)
                window.RectanglesGrid.RowDefinitions.Add(new RowDefinition());
        }

        private static void TileGridUpdate(TileSet tileSet, int ChosenTile, MainWindow window)
        {
            window.TileGrid.Children.Clear();

            var size = tileSet.TileSize;

            window.TileGrid.Rows    = size.Height;
            window.TileGrid.Columns = size.Width;
            window.TileGrid.Background = new SolidColorBrush(Colors.Transparent);

            Bitmap tile = tileSet.Tiles.Count > 0 ? tileSet.Tiles[ChosenTile] : new Bitmap(size.Height, size.Width);

            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    Border Border = new Border()
                    {
                        BorderThickness = new Thickness(x == 0 ? 1d : 0d, y == 0 ? 1d : 0d, 1d, 1d),
                        Background = new SolidColorBrush(tile.GetPixel(x, y).A > 0 ? Colors.Black : Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Gray),
                    };

                    window.TileGrid.Children.Add(Border);
                }
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
            OnPropertyChanged(nameof(HexAngle));
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            return textboxValidator.GetErrors(propertyName);
        }

        public bool HasErrors => textboxValidator.HasErrors;
    }
}
