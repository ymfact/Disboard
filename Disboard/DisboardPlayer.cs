using System;

namespace Disboard
{
    using UserIdType = UInt64;
    /// <summary>
    /// 게임에 참여하는 플레이어입니다. 게임에 참가하는 인원은 변경할 수 없습니다.
    /// </summary>
    public abstract class DisboardPlayer
    {
        internal DisboardPlayer(UserIdType id, string name, string nickname, string mention, DisboardChannel channel)
        {
            Id = id;
            Name = name;
            Nickname = nickname;
            Mention = mention;
            Channel = channel;
            NextPlayer = this;
        }
        internal UserIdType Id { get; }
        /// <summary>
        /// 다음 차례의 플레이어입니다. 순서는 InitialPlayers의 순서와 일치하며, 게임 시작시 임의로 정해집니다.
        /// </summary>
        public DisboardPlayer NextPlayer { get; set; }
        /// <summary>
        /// 이름입니다. 변동될 경우 실시간으로 반영됩니다.
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// 서버에서 사용하는 닉네임입니다. 변동될 경우 실시간으로 반영됩니다.
        /// </summary>
        public string Nickname { get; internal set; }
        /// <summary>
        /// 이 문자열을 대화방에 전송하면 플레이어를 호출할 수 있습니다.
        /// </summary>
        public string Mention { get; }
        /// <summary>
        /// DM 채널입니다.
        /// </summary>
        /// <remarks>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </remarks>
        public DisboardChannel Channel { get; }
        /// <summary>
        /// DM을 전송합니다. Discord embed를 포함할 수도 있습니다.
        /// </summary>
        /// <remarks>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </remarks>
        public SendType DM => Channel.Send;
        /// <summary>
        /// DM 채널의 URL입니다. 대화방에 전송하면 URL을 클릭했을 때 이 채널을 보게 됩니다. 
        /// </summary>
        /// <remarks>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </remarks>
        public string DMURL => Channel.URL;
        /// <summary>
        /// 한 장의 이미지를 DM으로 전송합니다. 메시지나, Discord embed를 포함할 수도 있습니다.
        /// </summary>
        /// <remarks>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </remarks>
        public SendImageType DMImage => Channel.SendImage;
        /// <summary>
        /// 여러 장의 이미지를 전송합니다. 메시지나, Discord embed를 포함할 수도 있습니다.
        /// </summary>
        /// <remarks>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </remarks>
        public SendImagesType DMImages => Channel.SendImages;
    }
}
