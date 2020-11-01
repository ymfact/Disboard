using DSharpPlus;
using DSharpPlus.Entities;
using System;

namespace Disboard
{
    static class StringToEmoji
    {
        public static DiscordEmoji ToEmoji(this string text, DiscordClient client)
        {
            try
            {
                return DiscordEmoji.FromName(client, text);
            }
            catch (ArgumentException)
            {
                return DiscordEmoji.FromUnicode(client, text);
            }
        }
    }
}
