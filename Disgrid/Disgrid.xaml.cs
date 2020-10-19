using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Disgrid
{
    /// <summary>
    /// Disgrid를 사용하면 텍스트로 이루어진 표를 간단히 만들 수 있습니다.
    /// Render 함수 안에서만 생성 및 수정할 수 있습니다.
    /// BoardContext.cs의 GetBoardGrid를 예제로써 참고하세요.
    /// 사용하려면 Main 함수 윗줄에 [System.STAThread()]를 추가해야 합니다.
    /// </summary>
    partial class Disgrid : UserControl
    {
        /// <summary>
        /// Render 함수 안에서 생성해야 합니다.
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public Disgrid(int rowCount, int columnCount)
        {
            InitializeComponent();
            InitGrid(rowCount, columnCount);
        }

        /// <summary>
        /// Render 함수 안에서 실행되어야 합니다.
        /// </summary>
        /// <param name="row">0부터 시작합니다.</param>
        /// <param name="column">0부터 시작합니다.</param>
        /// <param name="text"></param>
        /// <returns>WPF Label을 반환합니다. Render 함수 안에서 수정할 수 있습니다.</returns>
        public Label Add(int row, int column, string text)
        {
            var label = new Label { Content = text };
            Grid.SetColumn(label, column);
            Grid.SetRow(label, row);
            InnerGrid.Children.Add(label);
            return label;
        }

        /// <summary>
        /// 그리드 전역 스타일을 추가합니다.
        /// Render 함수 안에서만 호출해야 합니다.
        /// </summary>
        /// <typeparam name="ControlType">스타일을 적용할 WPF Control</typeparam>
        /// <param name="property">적용할 WPF 스타일 종류</param>
        /// <param name="value">property와 대응되는 타입이어야 합니다. 예를 들어 ColumnDefinition.MinWidth에는 double이 대응됩니다.</param>
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
