using Disboard;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoVisual
{
    class EchoVisual : Game
    {
        public EchoVisual(GameInitializeData initData) : base(initData) { }
        public override async Task Start()
        {
            var image = Render(() => new Label { Foreground = Brushes.White, Content = "EchoVisual started." });
            await SendImage(image);
        }
        public override async Task OnGroup(Player player, string message)
        {
            var image = Render(() => new MyLabel(message));
            await SendImage(image);
        }
    }
}
