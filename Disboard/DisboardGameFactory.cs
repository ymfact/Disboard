using System;

namespace Disboard
{
    /// <summary>
    /// 도움말을 제공하지 않는 IGameFactory의 기본적인 구현입니다.
    /// Game은 파라미터가 GameInitializeData 1개인 public 생성자를 가져야 합니다.
    /// </summary>
    /// <typeparam name="GameType">파라미터가 GameInitializeData 1개인 public 생성자를 가진 Game입니다.</typeparam>
    public sealed class DisboardGameFactory<GameType> : IDisboardGameFactory where GameType : DisboardGame
    {
        internal DisboardGame New(DisboardGameInitData initData) => (DisboardGame)Activator.CreateInstance(typeof(GameType), new[] { initData })!;
        DisboardGame IDisboardGameFactory.New(DisboardGameInitData initData) => New(initData);

        internal void OnHelp(DisboardChannel channel) => channel.Send("게임이 도움말을 제공하지 않습니다.");
        void IDisboardGameFactory.OnHelp(DisboardChannel channel) => OnHelp(channel);
    }
}
