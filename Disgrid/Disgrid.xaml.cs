using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Disgrid
{
    partial class Disgrid : UserControl
    {
        public Disgrid(int rowCount, int columnCount)
        {
            InitializeComponent();
            InitGrid(rowCount, columnCount);
        }

        public Label Add(int row, int column, string text)
        {
            var label = new Label { Content = text };
            Grid.SetColumn(label, column);
            Grid.SetRow(label, row);
            grid.Children.Add(label);
            return label;
        }

        public void AddStyle<ControlType>(DependencyProperty property, object value) where ControlType : Control
        {
            var type = typeof(ControlType);
            if (false == grid.Resources.Contains(type))
            {
                grid.Resources.Add(type, new Style(type));
            }
            var style = grid.Resources[type] as Style;
            style!.Setters.Add(new Setter(property, value));
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
