using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Disgrid
{
    partial class Disgrid : UserControl
    {
        public Grid Grid => grid;
        double DefaultFontSize { get; } = SystemFonts.MessageFontSize;
        HorizontalAlignment DefaultTextAlign { get; }
        public Disgrid(int rowCount, int columnCount, double? defaultFontSize = null, HorizontalAlignment defaultTextAlign = HorizontalAlignment.Center)
        {
            DefaultTextAlign = defaultTextAlign;
            if (defaultFontSize != null && defaultFontSize > 0)
                DefaultFontSize = defaultFontSize.Value;

            InitializeComponent();
            InitGrid(rowCount, columnCount);
        }

        public Label Add(int row, int column, string text)
        {
            var label = new Label
            {
                Content = text,
                FontSize = DefaultFontSize,
                HorizontalContentAlignment = DefaultTextAlign,
                Foreground = Brushes.White,
            };
            Grid.SetColumn(label, column);
            Grid.SetRow(label, row);
            grid.Children.Add(label);
            return label;
        }

        void InitGrid(int rowCount, int columnCount)
        {
            foreach (var _ in Enumerable.Range(0, rowCount))
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            foreach (var _ in Enumerable.Range(0, columnCount))
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        }
    }
}
