using DSharpPlus.Entities;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    public class Channel
    {
        public SendType Send { get; }
        public SendImageType SendImage { get; }
        public SendImagesType SendImages { get; }
        public string URL { get; }

        internal protected Channel(DiscordChannel channel, ConcurrentQueue<Task> messageQueue)
        {
            Send = (message, embed) => messageQueue.Enqueue(channel.SendMessageAsync(message, embed: embed));
            SendImage = (stream, message, embed) => messageQueue.Enqueue(channel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed));
            SendImages = (streams, message, embed) => messageQueue.Enqueue(channel.SendMultipleFilesAsync(streams.Enumerate().ToDictionary(_ => $"{_.index}.png", _ => _.elem), content: message, embed: embed));

            string guildId = channel.GuildId == default ? "@me" : $"{channel.GuildId}";
            URL = $"https://discord.com/channels/{guildId}/{channel.Id}";
        }
    }
}
