using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Disgrid
{
    /// <summary>
    /// Disgrid를 사용하면 텍스트로 이루어진 표를 간단히 만들 수 있습니다.
    /// Yacht.cs의 GetBoardImage를 예제로써 참고하세요.
    /// 사용하려면 Main 함수 윗줄에 [System.STAThread()]를 추가해야 합니다.
    /// </summary>
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
