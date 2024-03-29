﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MediocreBot.Entities;
using System.Threading.Tasks;

namespace MediocreBot
{
    public class MediocreBot
    {
        // might not need this singleton
        public static MediocreBot Instance { get; private set; }

        public readonly DiscordSocketClient DiscordSocketClient;
        public readonly DataStorage DataStorage;

        private readonly DiscordMessageHandler _messageHandler;
        private readonly BotConfiguration _botConfiguration;

        public MediocreBot()
        {
            if (Instance != null) { return; }
            Instance = this;

            DiscordSocketClient = new DiscordSocketClient(Helpers.DiscordSocketConfigHelper.GetDefaultConfig());
            DataStorage = new DataStorage();

            _botConfiguration = DataStorage.GetBotConfiguration();
            _messageHandler = new DiscordMessageHandler(new CommandService(), _botConfiguration, DiscordSocketClient);
        }

        public async Task ConnectAsync()
        {
            await _messageHandler.InstallCommands();

            DiscordSocketClient.Log += StatusLogger.Log;
            await DiscordSocketClient.LoginAsync(TokenType.Bot, _botConfiguration.Token);
            await DiscordSocketClient.StartAsync();

            DiscordSocketClient.MessageReceived += _messageHandler.HandleMessageReceived;

            await DiscordSocketClient.SetGameAsync($"\"{_botConfiguration.Prefix}info\"", type: ActivityType.Listening);
        }

        ~MediocreBot()
        {
            Instance = null;
        }
    }
}
