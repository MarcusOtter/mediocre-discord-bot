using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediocreBot.Modules
{
    public class FactCommand : ModuleBase<SocketCommandContext>
    {
        private readonly DataStorage _dataStorage;

        private delegate string MediocreFact();
        private readonly List<MediocreFact> _mediocreFacts;

        private SocketGuildUser[] _serverUsers;
        private List<IMessage> _channelMessages;

        private FactCommand()
        {
            _dataStorage = MediocreBot.Instance.DataStorage;

            _mediocreFacts = new List<MediocreFact>
            {
                LongestMessage,
                LongestUsername,
                LongestNickname,
                BiggestDiscriminator
            };
        }

        [Command("fact")]
        public async Task GetRandomFact()
        {
            if (Context.IsPrivate)
            {
                await ReplyAsync("I am... not very smart. You need to send this in the server you want the fact in.");
                return;
            }

            if (_dataStorage.IsSavingMessages)
            {
                await ReplyAsync("I'm currently gathering some information about another channel, please try again soon.");
                return;
            }

            if (_dataStorage.ChannelHasMessagesSaved(Context.Channel))
            {
                await _dataStorage.SaveNewChannelMessagesAsync(Context.Channel);
            }
            else
            {
                await ReplyAsync("Please wait while I gather some information about this channel. I hope it doesn't have too many messages, I'm not very good at counting over 10 000. Luckily I only have to do this once per channel!");
                await _dataStorage.SaveAllChannelMessagesAsync(Context.Channel);
            }

            _channelMessages = _dataStorage.GetSavedMessages(Context.Channel);
            _serverUsers = Context.Guild.Users.ToArray();

            var random = new Random();
            await ReplyAsync($"Okay. {_mediocreFacts[random.Next(0, _mediocreFacts.Count)]()}");
        }

        private string LongestMessage()
        {
            var longestMessage = _channelMessages.OrderByDescending(x => x.Content.Length).FirstOrDefault();
            return $"The longest message sent in this channel is {longestMessage.Content.Length} characters long and was written by {longestMessage.Author} at {longestMessage.CreatedAt.Date.ToShortDateString()}? " +
                $"The message was:\n```{longestMessage.Content}```";
        }

        private string LongestUsername()
        {
            var userWithLongestUsername = _serverUsers.OrderByDescending(x => x.Username.Length).FirstOrDefault();
            return $"The user with the longest username is {userWithLongestUsername.Mention} which is {userWithLongestUsername.Username.Length} characters long.";
        }

        private string LongestNickname()
        {
            var userWithLongestNickname = _serverUsers.Where(x => x.Nickname != null).OrderByDescending(x => x.Nickname.Length).FirstOrDefault();
            if (userWithLongestNickname is null) return $"Nobody in this server has a nickname.";
            return $"The user with the longest nickname is {userWithLongestNickname.Mention} which is {userWithLongestNickname.Username.Length} characters long.";
        }

        private string SmallestDiscriminator()
        {
            var userWithSmallestDiscriminator = _serverUsers.OrderBy(x => x.DiscriminatorValue).FirstOrDefault();
            return $"The user with the smallest discriminator value is {userWithSmallestDiscriminator.Mention}, which is #{userWithSmallestDiscriminator.DiscriminatorValue}.";
        }

        private string BiggestDiscriminator()
        {
            var userWithBiggestDiscriminator = _serverUsers.OrderByDescending(x => x.DiscriminatorValue).FirstOrDefault();
            return $"The user with the biggest discriminator value is {userWithBiggestDiscriminator.Mention}, which is #{userWithBiggestDiscriminator.DiscriminatorValue}.";
        }

        //private string ServerCount()
        //{
        //    return "I am in 5 servers";
        //}
    }
}
