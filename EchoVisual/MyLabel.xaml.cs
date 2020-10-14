using System.Windows.Controls;

namespace EchoVisual
{
    public partial class MyLabel : UserControl
    {
        public string BindContent { get; }
        public MyLabel(string content)
        {
            BindContent = content;
            InitializeComponent();
        }
    }
}
