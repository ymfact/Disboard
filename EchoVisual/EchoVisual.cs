using Disboard;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoVisual
{
    class EchoVisual : DisboardGame
    {
        public EchoVisual(DisboardGameInitData initData) : base(initData) { }
        public override void Start()
        {
            var image = Render(() => new Label { Foreground = Brushes.White, Content = "EchoVisual started." });
            SendImage(image);
        }
        public override void OnGroup(DisboardPlayer player, string message)
        {
            var image = Render(() => new MyLabel(message));
            SendImage(image);
        }
    }
    class EchoVisualFactory : IDisboardGameFactory
    {
        public DisboardGame New(DisboardGameInitData initData) => new EchoVisual(initData);
        public void OnHelp(DisboardChannel channel) => channel.Send("`EchoVisual`");
    }
}
