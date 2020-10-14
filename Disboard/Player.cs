namespace Disboard
{
    public abstract class Player
    {
        public abstract string Name { get; }
        public abstract string Mention { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public abstract SendType DM { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public abstract string DMURL { get; }
    }
}
