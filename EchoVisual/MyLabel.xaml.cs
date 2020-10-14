using System.Windows.Controls;

namespace EchoVisual
{
    public partial class MyLabel : UserControl
    {
        public MyLabel(string content)
        {
            InitializeComponent();
            Label.Content = content;
        }
    }
}
