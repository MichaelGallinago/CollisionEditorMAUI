 using CollisionEditor.Model;
using CollisionEditor.View;
 using System.Windows.Input;
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

                page.DrawRedLine();
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
                page.DrawRedLine();
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

        private MainPage page;
        private byte byteAngle;
        private string hexAngle;
        private uint chosenTile;
        private TextboxValidator textboxValidator;

        public MainViewModel(MainPage page)
        {
            textboxValidator = new TextboxValidator();
            textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
            AngleMap = new AngleMap(0);
            TileSet  = new TileSet(0);
            chosenTile = 0;
            byteAngle = 0;
            hexAngle = "0x00";
            this.page = page;

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
            TileGridUpdate(TileSet, (int)chosenTile, page);
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
                page.SelectTileTextBox.IsEnabled = true;
                page.SelectTileButton.IsEnabled  = true;
                page.TextBoxByteAngle.IsEnabled  = true;
                page.TextBoxHexAngle.IsEnabled   = true;

                TileMapGridUpdate(TileSet.Tiles.Count);
                page.DrawRedLine();
            }
        }

        public void ShowAngles((byte byteAngle, string hexAngle, double fullAngle) angles)
        {
            
            byteAngle = angles.byteAngle;
            OnPropertyChanged(nameof(ByteAngle));
            page.ByteAngleIncrimentButton.IsEnabled = true;
            page.ByteAngleDecrementButton.IsEnabled = true;

            hexAngle = angles.hexAngle;
            OnPropertyChanged(nameof(HexAngle));
            page.HexAngleIncrimentButton.IsEnabled = true;
            page.HexAngleDecrementButton.IsEnabled = true;

            page.TextBlockFullAngle.Text = angles.fullAngle.ToString() + "°";
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
                TileGridUpdate(TileSet, (int)ChosenTile, page);
                RectanglesGridUpdate();
                page.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
                page.Widths.Text  = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

                ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
                page.TextBoxByteAngle.IsEnabled = true;
                page.TextBoxHexAngle.IsEnabled  = true;

                page.TileMapGrid.Children.Clear();

                foreach (Bitmap tile in TileSet.Tiles)
                {
                    var image = new System.Windows.Controls.Image()
                    {
                        Width  = TileSet.TileSize.Width  * 2,
                        Height = TileSet.TileSize.Height * 2
                    };
                    image.Source = ViewModelAssistant.BitmapConvert(tile);
                    page.TileMapGrid.Children.Add(image);
                }

                page.SelectTileTextBox.IsEnabled = true;
                page.SelectTileButton.IsEnabled = true;

                TileMapGridUpdate(TileSet.Tiles.Count);
                page.DrawRedLine();
            }
        }

        public void TileMapGridUpdate(int tileCount)
        {
            page.TileMapGrid.Height = (int)Math.Ceiling((double)tileCount / page.TileMapGrid.Columns) * (TileSet.TileSize.Height * tileMapTileScale + tileMapSeparation);
        }

        private async void MenuSaveTileMap()
        {
            if (TileSet.Tiles.Count == 0)
            {
                await DisplayAlert ("Error: You haven't chosen TileMap to save");
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
            page.TileMapGrid.Children.Clear();
            TileSet = new TileSet(AngleMap.Values.Count);

            TileGridUpdate(TileSet, (int)ChosenTile, page);
            page.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
            page.Widths.Text = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        }

        private void MenuUnloadAngleMap()
        {
            AngleMap = new AngleMap(TileSet.Tiles.Count);
            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        }

        private void MenuUnloadAll()
        {
            page.TileMapGrid.Children.Clear();
            TileSet = new TileSet(0);
            AngleMap = new AngleMap(0);

            page.Heights.Text = null;
            page.Widths.Text = null;
            ShowAngles((0, "0x00", 0));
            page.SelectTileTextBox.Text = "0";
            page.ByteAngleIncrimentButton.IsEnabled = false;
            page.ByteAngleDecrementButton.IsEnabled = false;
            page.HexAngleIncrimentButton.IsEnabled = false;
            page.HexAngleDecrementButton.IsEnabled = false;
            page.SelectTileTextBox.IsEnabled = false;
            page.SelectTileButton.IsEnabled = false;
            page.TextBoxByteAngle.IsEnabled = false;
            page.TextBoxHexAngle.IsEnabled = false;
            page.canvasForLine.Children.Clear();
            page.RectanglesGrid.Children.Clear();
        }

        private void AngleIncrement()
        {
            byte byteAngle = AngleMap.ChangeAngle((int)chosenTile, 1);

            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

            page.DrawRedLine();
        }

        private void AngleDecrement()
        {
            byte byteAngle = AngleMap.ChangeAngle((int)chosenTile, -1);

            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

            page.DrawRedLine();
        }

        public void SelectTile()
        {
            if (chosenTile > TileSet.Tiles.Count - 1)
            {
                chosenTile = (uint)TileSet.Tiles.Count - 1;
                OnPropertyChanged(nameof(ChosenTile));
            }

            System.Windows.Controls.Image lastTile = GetTile(page.LastChosenTile);

            page.TileMapGrid.Children.RemoveAt(page.LastChosenTile);
            page.TileMapGrid.Children.Insert(page.LastChosenTile, lastTile);

            System.Windows.Controls.Image newTile = GetTile((int)chosenTile);

            Border border = new Border()
            {
                Width = TileSet.TileSize.Width * tileMapTileScale + tileMapSeparation,
                Height = TileSet.TileSize.Height * tileMapTileScale + tileMapSeparation,
                BorderBrush = new SolidColorBrush(Colors.Red),
                BorderThickness = new Thickness(2),
                Child = newTile
            };

            page.TileMapGrid.Children.RemoveAt((int)chosenTile);
            page.TileMapGrid.Children.Insert((int)chosenTile, border);

            page.LastChosenTile = (int)chosenTile;
            TileGridUpdate(TileSet, (int)ChosenTile, page);
            page.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
            page.Widths.Text  = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);
            
            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));

            page.DrawRedLine();
            page.RectanglesGrid.Children.Clear();
        }

        public void SelectTileFromTileMap()
        {
            OnPropertyChanged(nameof(ChosenTile));
            TileGridUpdate(TileSet, (int)ChosenTile, page);
            page.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
            page.Widths.Text = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

            ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));

            page.DrawRedLine();
            page.RectanglesGrid.Children.Clear();
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
            page.Close();
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
            page.RectanglesGrid.ColumnDefinitions.Clear();
            page.RectanglesGrid.RowDefinitions.Clear();

            var size = TileSet.TileSize;

            for (int x = 0; x < size.Width; x++)
                page.RectanglesGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int y = 0; y < size.Height; y++)
                page.RectanglesGrid.RowDefinitions.Add(new RowDefinition());
        }

        private static void TileGridUpdate(TileSet tileSet, int ChosenTile, MainPage page)
        {
            page.TileGrid.Children.Clear();

            var size = tileSet.TileSize;

            page.TileGrid.Rows    = size.Height;
            page.TileGrid.Columns = size.Width;
            page.TileGrid.Background = new SolidColorBrush(Colors.Transparent);

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

                    page.TileGrid.Children.Add(Border);
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
