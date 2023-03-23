using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Media;
using System;
using Microsoft.Maui.Graphics;

namespace CollisionEditor.View
{
    internal static class RedLineService
    {
        static MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;

        public static void DrawRedLine(ref Line redLine)
        {
            float floatAngle = float.Parse(mainWindow.TextBlockFullAngle.Text.TrimEnd('°'));

            if (floatAngle > 180f)
                floatAngle -= 180f;

            int size = (int)mainWindow.canvasForLine.Width / 2;

            Line newLine = new Line();
            double length = size / Math.Abs(Math.Cos((-45 + ((floatAngle + 45) % 90)) / 180 * Math.PI));
            floatAngle += 90;
            newLine.X1 = length * Math.Sin(floatAngle / 180 * Math.PI);
            newLine.Y1 = length * Math.Cos(floatAngle / 180 * Math.PI);
            newLine.X2 = -newLine.X1;
            newLine.Y2 = -newLine.Y1;
            ICanvas.SetTop(newLine, size);
            ICanvas.SetLeft(newLine, size);
            newLine.Stroke = new SolidColorBrush(Colors.Red);
            newLine.Fill = new SolidColorBrush(Colors.Red);

            mainWindow.canvasForLine.Children.Remove(redLine);
            redLine = newLine;
            mainWindow.canvasForLine.Children.Add(newLine);
        }
    }
}
