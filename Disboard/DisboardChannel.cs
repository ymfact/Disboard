using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Disboard
{
    /// <summary>
    /// 디스코드 채널입니다. 그룹 채널일 수도, DM 채널일 수도 있습니다.
    /// </summary>
    public class DisboardChannel
    {
        /// <summary>
        /// 메시지를 전송합니다. Discord embed를 포함할 수도 있습니다.
        /// </summary>
        public SendType Send { get; }
        /// <summary>
        /// 한 장의 이미지를 전송합니다. 메시지나, Discord embed를 포함할 수도 있습니다.
        /// </summary>
        public SendImageType SendImage { get; }
        /// <summary>
        /// 여러 장의 이미지를 전송합니다. 메시지나, Discord embed를 포함할 수도 있습니다.
        /// </summary>
        public SendImagesType SendImages { get; }
        /// <summary>
        /// 이 채널의 URL입니다. 대화방에 전송하면 URL을 클릭했을 때 이 채널을 보게 됩니다. 
        /// </summary>
        public string URL { get; }
        /// <summary>
        /// WPF 컨트롤을 사용하여 이미지를 그릴 수 있습니다. EchoVisual.cs를 예제로써 참고하세요.
        /// </summary>
        public RenderType Render { get; }

        /// <summary>
        /// 디스코드 채널입니다. 그룹 채널일 수도, DM 채널일 수도 있습니다.
        /// </summary>
        /// <param name="channel">그룹 채널일 수도, DM 채널일 수도 있습니다.</param>
        /// <param name="messageQueue">메시지 큐에 태스크를 넣으면 메시지를 전송할 수 있습니다.</param>
        /// <param name="dispatcher">WPF 컨트롤을 다루기 위해 메인 스레드의 디스패쳐가 필요합니다.</param>
        internal protected DisboardChannel(DiscordChannel channel, ConcurrentQueue<Func<Task>> messageQueue, Dispatcher dispatcher)
        {
            Send = (message, embed) => messageQueue.Enqueue(() => channel.SendMessageAsync(message, embed: embed));
            SendImage = (stream, message, embed) => messageQueue.Enqueue(() => channel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed));
            SendImages = (streams, message, embed) => messageQueue.Enqueue(() => channel.SendMultipleFilesAsync(streams.Enumerate().ToDictionary(_ => $"{_.index}.png", _ => _.elem), content: message, embed: embed));

            string guildId = channel.GuildId == default ? "@me" : $"{channel.GuildId}";
            URL = $"https://discord.com/channels/{guildId}/{channel.Id}";

            Render = controlConstructor => dispatcher.Invoke(() => controlConstructor().Render());
        }
    }
}
