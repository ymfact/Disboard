namespace Disboard
{
    /// <summary>
    /// 게임이 플레이어의 DM을 받기를 원할 때 DisboardGame 대신 사용합니다.
    /// </summary>
    /// <remarks>
    /// 생성자에서는 Send, OnFinish 등을 호출하지 않아야 합니다. 대신 OnStart를 사용하세요.
    /// </remarks>
    public abstract class DisboardGameUsingDM : DisboardGame
    {
        /// <summary>
        /// 그룹 채널의 BOT start, BOT restart 명령어에 의해 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 생성자에서는 Send, OnFinish 등을 호출하지 않아야 합니다. 대신 OnStart를 사용하세요.
        /// </remarks>
        /// <param name="initData">게임 생성에 필요한 데이터입니다.</param>
        public DisboardGameUsingDM(DisboardGameInitData initData) : base(initData) { }
        /// <summary>
        /// 플레이어가 DM을 보내면 호출됩니다.
        /// </summary>
        /// <param name="author">DM을 작성한 플레이어입니다. 반드시 게임에 참여하고 있습니다.</param>
        /// <param name="message">플레이어가 작성한 DM의 내용입니다.</param>
        public abstract void OnDM(DisboardPlayer author, string message);
    }
}
