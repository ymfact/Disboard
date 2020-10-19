namespace Disboard
{
    /// <summary>
    /// 게임을 생성하고 도움말을 제공합니다. 기본적인 기능만을 갖고있는 DisboardGameFactory를 사용할 수도 있습니다.
    /// </summary>
    public interface IDisboardGameFactory
    {
        /// <summary>
        /// 게임을 생성합니다. 그룹 채널에서 BOT start, BOT restart를 입력할 때 실행됩니다.
        /// </summary>
        /// <param name="initData">게임 생성에 필요한 기본적인 데이터입니다.</param>
        /// <returns>생성된 게임을 반환해야 합니다.</returns>
        DisboardGame New(DisboardGameInitData initData);

        /// <summary>
        /// 도움말을 제공합니다. 그룹 채널에서 BOT help를 입력할 때 실행됩니다.
        /// </summary>
        /// <param name="channel">채널에 메시지를 전송할 수 있습니다.</param>
        void OnHelp(DisboardChannel channel);
    }
}
