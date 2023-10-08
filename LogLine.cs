using System.Collections.ObjectModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ApkHelper
{
    public class LogLine
    {
        public string Content { get; private set; }
        public Brush Color { get; private set; }

        private LogLine(string content, Brush color)
        {
            Content = content;
            Color = color;
        }

        public static LogLine Info(string content)
        {
            var theme = App.CurrentTheme();
            var color = Colors.Black;
            if (theme == ElementTheme.Dark)
            {
                color = Colors.White;
            }

            return new LogLine(content, new SolidColorBrush(color));
        }

        public static LogLine Error(string content)
        {
            var theme = App.CurrentTheme();
            var color = Colors.Red;
            if (theme == ElementTheme.Dark)
            {
                color = Colors.DarkRed;
            }

            return new LogLine(content, new SolidColorBrush(color));
        }
    }

    public class LogViewModel
    {
        public readonly ObservableCollection<LogLine> LogLines = new();
    }
}