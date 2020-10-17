using System;

namespace Disboard
{
    public interface IGameFactory
    {
        Game New(GameInitializeData initData);

        void OnHelp(Channel channel);
    }

    public class GameFactory<GameType> : IGameFactory where GameType : Game
    {
        public Game New(GameInitializeData initData) => (Game)Activator.CreateInstance(typeof(GameType), new[] { initData })!;

        public void OnHelp(Channel channel) => channel.Send("게임이 도움말을 제공하지 않습니다.");
    }
}
