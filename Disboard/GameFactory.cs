using System;

namespace Disboard
{
    public interface IGameFactory
    {
        Game New(GameInitializeData initData);

        void OnHelp(Channel channel);
    }

    /// <summary>
    /// 도움말을 제공하지 않는 IGameFactory의 기본적인 구현입니다.
    /// Game은 파라미터가 GameInitializeData 1개인 public 생성자를 가져야 합니다.
    /// </summary>
    /// <typeparam name="GameType">파라미터가 GameInitializeData 1개인 public 생성자를 가진 Game입니다.</typeparam>
    public class GameFactory<GameType> : IGameFactory where GameType : Game
    {
        public Game New(GameInitializeData initData) => (Game)Activator.CreateInstance(typeof(GameType), new[] { initData })!;

        public void OnHelp(Channel channel) => channel.Send("게임이 도움말을 제공하지 않습니다.");
    }
}
