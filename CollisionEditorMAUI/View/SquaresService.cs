using CollisionEditor.Model;
using System.Windows.Controls;

namespace CollisionEditor.View
{
    internal static class SquaresService
    {
        static MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;

        public static void MoveSquare(Vector2<int> position, SquareAndPosition firstSquare, SquareAndPosition secondSquare)
        {
            firstSquare.Square.SetValue(Grid.ColumnProperty, position.X);
            firstSquare.Square.SetValue(Grid.RowProperty,    position.Y);

            bool isFirstExists = mainWindow.RectanglesGrid.Children.Contains(firstSquare.Square);
            if (!Equals(position, firstSquare.Position) && isFirstExists ||  !isFirstExists)
            {
                firstSquare.Position = position;

                if (Equals(position, secondSquare.Position) && mainWindow.RectanglesGrid.Children.Contains(secondSquare.Square))
                {
                    mainWindow.RectanglesGrid.Children.Remove(secondSquare.Square);
                }

                if (!isFirstExists)
                {
                    mainWindow.RectanglesGrid.Children.Add(firstSquare.Square);
                }
            }
            else
            {
                mainWindow.RectanglesGrid.Children.Remove(firstSquare.Square);
            }
        }
    }
}
