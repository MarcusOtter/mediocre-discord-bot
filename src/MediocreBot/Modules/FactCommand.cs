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
                ShortestMessage,
                LongestMessage,

                ShortestUsername,
                LongestUsername,

                ShortestNickname,
                LongestNickname,

                SmallestDiscriminator,
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

        private string ShortestMessage()
        {
            var shortestMessage = _channelMessages.Where(x => x.Content.Length > 0).OrderBy(x => x.Content.Length).FirstOrDefault();
            return $"The shortest user message sent in this channel is {shortestMessage.Content.Length} character(s) long and was written by {shortestMessage.Author} at {shortestMessage.CreatedAt.Date.ToShortDateString()}? " +
                $"The message was:\n```{shortestMessage.Content}```";
        }

        private string LongestMessage()
        {
            var longestMessage = _channelMessages.OrderByDescending(x => x.Content.Length).FirstOrDefault();
            return $"The longest user message sent in this channel is {longestMessage.Content.Length} characters long and was written by {longestMessage.Author} at {longestMessage.CreatedAt.Date.ToShortDateString()}? " +
                $"The message was:\n```{longestMessage.Content}```";
        }

        private string ShortestUsername()
        {
            var user = _serverUsers.OrderBy(x => x.Username.Length).FirstOrDefault();
            return $"The user with the shortest username is {user.Mention}{(string.IsNullOrEmpty(user.Nickname) ? "" : $" ({user.Username})")} which is {user.Username.Length} characters long.";
        }

        private string LongestUsername()
        {
            var user = _serverUsers.OrderByDescending(x => x.Username.Length).FirstOrDefault();
            return $"The user with the longest username is {user.Mention}{(string.IsNullOrEmpty(user.Nickname) ? "" : $" ({user.Username})")} which is {user.Username.Length} characters long.";
        }

        private string ShortestNickname()
        {
            var user = _serverUsers.Where(x => x.Nickname != null).OrderBy(x => x.Nickname.Length).FirstOrDefault();
            if (user is null) return $"Nobody in this server has a nickname.";
            return $"The user with the shortest nickname is {user.Mention} which is {user.Nickname.Length} characters long.";
        }

        private string LongestNickname()
        {
            var user = _serverUsers.Where(x => x.Nickname != null).OrderByDescending(x => x.Nickname.Length).FirstOrDefault();
            if (user is null) return $"Nobody in this server has a nickname.";
            return $"The user with the longest nickname is {user.Mention} which is {user.Nickname.Length} characters long.";
        }

        private string SmallestDiscriminator()
        {
            var user = _serverUsers.OrderBy(x => x.DiscriminatorValue).FirstOrDefault();
            return $"The user with the smallest discriminator value is {user.Mention}, which is #{user.DiscriminatorValue.ToString("0000")}.";
        }

        private string BiggestDiscriminator()
        {
            var user = _serverUsers.OrderByDescending(x => x.DiscriminatorValue).FirstOrDefault();
            return $"The user with the biggest discriminator value is {user.Mention}, which is #{user.DiscriminatorValue.ToString("0000")}.";
        }
    }
}
