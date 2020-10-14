using Disboard;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoVisual
{
    class EchoVisual : IGame
    {
        SendImageType SendImage { get; }
        RenderType Render { get; }
        public EchoVisual(GameInitializeData initData)
        {
            SendImage = initData.SendImage;
            Render = initData.Render;
        }
        public async Task Start()
        {
            var image = Render(() => new Label { Foreground = Brushes.White, Content = "EchoVisual started." });
            await SendImage(image);
        }
        public async Task OnGroup(Player player, string message)
        {
            var image = Render(() => new MyLabel(message));
            await SendImage(image);
        }
    }
}
