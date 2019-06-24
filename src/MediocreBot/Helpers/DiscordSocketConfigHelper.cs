using Discord;
using Discord.WebSocket;

namespace MediocreBot.Helpers
{
    public static class DiscordSocketConfigHelper
    {
        public static DiscordSocketConfig GetDefaultConfig()
        {
            return new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            };
        }
    }
}
