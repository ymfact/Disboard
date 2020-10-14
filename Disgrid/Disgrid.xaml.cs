using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            InnerGrid.Children.Add(label);
            return label;
        }

        public void AddStyle<ControlType>(DependencyProperty property, object value) where ControlType : DependencyObject
        {
            var type = typeof(ControlType);
            var setter = new Setter(property, value);
            var style = new Style(type, InnerGrid.Resources[type] as Style);
            style.Setters.Add(setter);
            InnerGrid.Resources[type] = style;
        }

        void InitGrid(int rowCount, int columnCount)
        {
            foreach (var _ in Enumerable.Range(0, rowCount))
                InnerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            foreach (var _ in Enumerable.Range(0, columnCount))
                InnerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        }
    }
}
