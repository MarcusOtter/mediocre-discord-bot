using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using MediocreBot.Entities;

namespace MediocreBot
{
    public class DataStorage
    {
        public bool IsSavingMessages;
        private Dictionary<ulong, List<IMessage>> _channelMessages = new Dictionary<ulong, List<IMessage>>();

        /// <summary>Returns all the saved messages in this channel, where the most recent message is first.</summary>
        public List<IMessage> GetSavedMessages(ISocketMessageChannel channel) => _channelMessages[channel.Id];
        public bool ChannelHasMessagesSaved(ISocketMessageChannel channel) => _channelMessages.ContainsKey(channel.Id);

        /// <summary>
        /// Saves the messages that were sent after the previous save.
        /// Please check that <see cref="ChannelHasMessagesSaved(ulong)"/> is true first.
        /// </summary>
        public async Task SaveNewChannelMessagesAsync(ISocketMessageChannel channel)
        {
            var channelMessages = _channelMessages[channel.Id];
            var latestMessageSaved = channelMessages[0];

            IsSavingMessages = true;
            var newMessages = (await channel.GetMessagesAsync(latestMessageSaved.Id, Direction.After, int.MaxValue).FlattenAsync())
                .Where(x => !x.Author.IsBot)
                .Reverse()
                .ToList();
            IsSavingMessages = false;

            _channelMessages[channel.Id].InsertRange(0, newMessages);
        }

        /// <summary>
        /// Saves all the messages in a channel. 
        /// This saves about 300-700 messages per second, which means it can take minutes for larger channels.
        /// </summary>
        public async Task SaveAllChannelMessagesAsync(ISocketMessageChannel channel)
        {
            IsSavingMessages = true;
            _channelMessages[channel.Id] = (await channel.GetMessagesAsync(int.MaxValue).FlattenAsync())
                .Where(x => !x.Author.IsBot)
                .ToList();
            IsSavingMessages = false;
        }

        public BotConfiguration GetBotConfiguration()
        {
            // Breaks unless it's running release version, very specific folder structure.

            if (!File.Exists("../../../../../../BotConfiguration.json"))
            {
                throw new FileNotFoundException("Your BotConfiguration.json file is missing. Create it and try again.");
            }

            var botConfigJson = File.ReadAllText("../../../../../../BotConfiguration.json");
            return JsonConvert.DeserializeObject<BotConfiguration>(botConfigJson);
        }
    }
}
