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
        private bool _hasNicknames;

        private FactCommand()
        {
            _dataStorage = MediocreBot.Instance.DataStorage;

            _mediocreFacts = new List<MediocreFact>
            {
                ShortestMessage,
                LongestMessage,

                ShortestUsername,
                LongestUsername,

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
                await ReplyAsync("Please wait while I gather some information about this channel. This can take several minutes as I can only read 300-700 messages per second. Luckily I only have to do this once per channel!");
                await _dataStorage.SaveAllChannelMessagesAsync(Context.Channel);
            }

            _channelMessages = _dataStorage.GetSavedMessages(Context.Channel);
            _serverUsers = Context.Guild.Users.ToArray();

            _hasNicknames = _serverUsers.Any(x => x.Nickname != null);
            if (_hasNicknames)
            {
                _mediocreFacts.Add(ShortestNickname);
                _mediocreFacts.Add(LongestNickname);
            }
            else
            {
                _mediocreFacts.Add(NoNicknames);
            }

            var random = new Random();
            await ReplyAsync(_mediocreFacts[random.Next(0, _mediocreFacts.Count)]());
        }

        private string NoNicknames()
        {
            return "Nobody in this server has a nickname.";
        }

        // A link to this message would be great
        private string ShortestMessage()
        {
            var shortestMessage = _channelMessages.Where(x => x.Content.Length > 0).OrderBy(x => x.Content.Length).FirstOrDefault();
            return $"The shortest user message sent in this channel is {shortestMessage.Content.Length} character(s) long and was written by `{shortestMessage.Author}` at {shortestMessage.CreatedAt.Date.ToShortDateString()}.\n" +
                $"The message was:\n```{shortestMessage.Content}```";
        }

        // A link to this message would be great
        // This fails if the content doesn't fit in less than 2000 characters. Needs some error checking.
        private string LongestMessage()
        {
            var longestMessage = _channelMessages.OrderByDescending(x => x.Content.Length).FirstOrDefault();
            return $"The longest user message sent in this channel is {longestMessage.Content.Length} characters long and was written by `{longestMessage.Author}` at {longestMessage.CreatedAt.Date.ToShortDateString()}.\n" +
                $"The message was:\n```{longestMessage.Content}```";
        }

        private string ShortestUsername()
        {
            var user = _serverUsers.OrderBy(x => x.Username.Length).FirstOrDefault();
            return $"The user with the shortest username is `{user.Username}#{user.Discriminator}`, which has a username that is {user.Username.Length} characters long.";
        }

        private string LongestUsername()
        {
            var user = _serverUsers.OrderByDescending(x => x.Username.Length).FirstOrDefault();
            return $"The user with the longest username is `{user.Username}#{user.Discriminator}`, which has a username that is {user.Username.Length} characters long.";
        }

        private string ShortestNickname()
        {
            var user = _serverUsers.Where(x => x.Nickname != null).OrderBy(x => x.Nickname.Length).FirstOrDefault();
            return $"The user with the shortest nickname is `{user.Nickname}`, which has a nickname that is {user.Nickname.Length} characters long.";
        }

        private string LongestNickname()
        {
            var user = _serverUsers.Where(x => x.Nickname != null).OrderByDescending(x => x.Nickname.Length).FirstOrDefault();
            return $"The user with the longest nickname is `{user.Nickname}`, which has a nickname that is {user.Nickname.Length} characters long.";
        }

        private string SmallestDiscriminator()
        {
            var user = _serverUsers.OrderBy(x => x.DiscriminatorValue).FirstOrDefault();
            return $"The user with the smallest discriminator value is `{user.Username}#{user.Discriminator}`, which is #{user.Discriminator}.";
        }

        private string BiggestDiscriminator()
        {
            var user = _serverUsers.OrderByDescending(x => x.DiscriminatorValue).FirstOrDefault();
            return $"The user with the biggest discriminator value is `{user.Username}#{user.Discriminator}`, which is #{user.Discriminator}.";
        }
    }
}
