using Disboard;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoVisual
{
    class EchoVisual : Game
    {
        public EchoVisual(GameInitializeData initData) : base(initData) { }
        public override void Start()
        {
            var image = Render(() => new Label { Foreground = Brushes.White, Content = "EchoVisual started." });
            SendImage(image);
        }
        public override void OnGroup(Player player, string message)
        {
            var image = Render(() => new MyLabel(message));
            SendImage(image);
        }
    }
    class EchoVisualFactory : IGameFactory
    {
        public Game New(GameInitializeData initData) => new EchoVisual(initData);
        public void OnHelp(Channel channel) => channel.Send("`EchoVisual`");
    }
}
