using Discord.Commands;
using Discord.WebSocket;
using MediocreBot.Entities;
using System.Reflection;
using System.Threading.Tasks;

namespace MediocreBot
{
    public class DiscordMessageHandler
    {
        private readonly CommandService _commandService;
        private readonly BotConfiguration _botConfiguration;
        private readonly DiscordSocketClient _discordSocketClient;

        public DiscordMessageHandler(CommandService commandService, BotConfiguration botConfiguration, DiscordSocketClient discordSocketClient)
        {
            _commandService = commandService;
            _botConfiguration = botConfiguration;
            _discordSocketClient = discordSocketClient;
        }

        public async Task InstallCommands()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
            StatusLogger.Log("Added command modules");
        }

        public async Task HandleMessageReceived(SocketMessage socketMessage)
        {
            var message = socketMessage as SocketUserMessage;
            var context = new SocketCommandContext(_discordSocketClient, message);

            if (context.User.IsBot) { return; }
            if (message is null) { return; }

            await HandleCommand(message, context);
        }

        private async Task HandleCommand(SocketUserMessage message, SocketCommandContext context)
        {
            int argPosition = 0;
            if (!MessageIsCommand(message, context, ref argPosition)) { return; }

            var result = await _commandService.ExecuteAsync(context, argPosition, services: null);
            if (result.IsSuccess) { return; }
            // todo: error handling with result here
        }

        /// <summary>Returns true if the message starts with a prefix, a mention or is sent in DMs.</summary>
        private bool MessageIsCommand(SocketUserMessage message, SocketCommandContext context, ref int argPosition)
        {
            return message.HasStringPrefix(_botConfiguration.Prefix, ref argPosition) ||
                   message.HasMentionPrefix(context.Client.CurrentUser, ref argPosition) ||
                   context.IsPrivate;
        }
    }
}
