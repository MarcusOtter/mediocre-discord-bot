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
        private List<MediocreFact> _mediocreFacts;

        private SocketGuildUser[] _serverUsers;
        private List<IMessage> _channelMessages;

        private FactCommand()
        {
            _dataStorage = MediocreBot.Instance.DataStorage;
        }

        [Command("fact")]
        public async Task GetRandomFact(ISocketMessageChannel channel = null)
        {
            channel = channel ?? Context.Channel;

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

            if (_dataStorage.ChannelHasMessagesSaved(channel))
            {
                await _dataStorage.SaveNewChannelMessagesAsync(channel);
            }
            else
            {
                await ReplyAsync("Please wait while I gather some information about this channel. This can take several minutes as I can only read 300-700 messages per second. Luckily I only have to do this once per channel!");
                await _dataStorage.SaveAllChannelMessagesAsync(channel);
                await ReplyAsync(":white_check_mark: Done gathering information.");
            }

            _channelMessages = _dataStorage.GetSavedMessages(channel);
            _serverUsers = Context.Guild.Users.ToArray();

            AddFactsAsync();

            var random = new Random();
            var RandomFact = _mediocreFacts[random.Next(0, _mediocreFacts.Count)];
            var result = RandomFact();
            await ReplyAsync(result);
        }

        private void AddFactsAsync()
        {
            _mediocreFacts = new List<MediocreFact>()
            {
                FirstEditedMessage,
                LatestEditedMessage,

                NewestAccountOnServer,
                OldestAccountOnServer,

                ShortestMessage,
                LongestMessage,
                AverageMessageLength,

                ShortestUsername,
                LongestUsername,
                AverageUsernameLength,

                SmallestDiscriminator,
                BiggestDiscriminator,

                LastMessageOnGoodTime,
                GoodNumberMessage,
                MessagesPerDayPastWeek,
                ServerCreationDate
            };

            var hasNicknames = _serverUsers.Any(x => x.Nickname != null);
            if (hasNicknames)
            {
                _mediocreFacts.Add(ShortestNickname);
                _mediocreFacts.Add(LongestNickname);
                _mediocreFacts.Add(AverageNicknameLength);
            }
            else
            {
                _mediocreFacts.Add(NoNicknames);
            }
        }

        private string NoNicknames()
        {
            return "Nobody in this server has a nickname.";
        }

        private string ServerCreationDate()
        {
            return $"This server was created {Context.Guild.CreatedAt.Date.ToShortDateString()}.";
        }

        private string OldestAccountOnServer()
        {
            var user = _serverUsers.OrderBy(x => x.CreatedAt.DateTime).FirstOrDefault();
            return $"The user with the oldest account in the server is `{user.Username}#{user.Discriminator}`. They created their account at {user.CreatedAt.Date.ToShortDateString()}.";
        }

        private string NewestAccountOnServer()
        {
            var user = _serverUsers.OrderByDescending(x => x.CreatedAt.DateTime).FirstOrDefault();
            return $"The user with the newest account in the server is `{user.Username}#{user.Discriminator}`. They created their account at {user.CreatedAt.Date.ToShortDateString()}.";
        }

        private string LatestEditedMessage()
        {
            var message = _channelMessages.OrderByDescending(x => x.Timestamp.DateTime).FirstOrDefault(x => x.EditedTimestamp != null);
            return $"The latest message that was edited in this channel was sent by `{message.Author}` at {message.CreatedAt.Date.ToShortDateString()}. I don't know what the message said before, but now it says:\n```{message.Content}```";
        }

        private string FirstEditedMessage()
        {
            var message = _channelMessages.OrderBy(x => x.Timestamp.DateTime).FirstOrDefault(x => x.EditedTimestamp != null);
            return $"The first message that was edited in this channel was sent by `{message.Author}` at {message.CreatedAt.Date.ToShortDateString()}. I don't know what the message said before, but now it says:\n```{message.Content}```";
        }

        private string MessagesPerDayPastWeek()
        {
            var messagesPastWeek = _channelMessages.Where(x => (DateTime.Now - x.CreatedAt.DateTime).TotalDays <= 7);
            return $"In the past week, there has been an average of {(messagesPastWeek.Count() / 7f).ToString("0.00")} messages sent per day in this channel.";
        }

        private string GoodNumberMessage()
        {
            if (_channelMessages.Count < GoodNumbersHelper.LowestGoodNumber)
            {
                return $"This channel has less than {GoodNumbersHelper.LowestGoodNumber} messages.";
            }

            var randomGoodNumber = GoodNumbersHelper.GetRandomGoodNumber(maxNumber: _channelMessages.Count - 1);
            var message = _channelMessages.OrderBy(x => x.Timestamp).ElementAt(randomGoodNumber);
            return $"Message number `{randomGoodNumber}` in this channel was written by `{message.Author}` at {message.CreatedAt.Date.ToShortDateString()} and reads:\n```{message.Content}```";
        }

        private string LastMessageOnGoodTime()
        {
            var randomGoodTime = GoodNumbersHelper.GetRandomGoodTime();
            var message = _channelMessages
                .Where(x => x.Timestamp.TimeOfDay.Hours == randomGoodTime.Hours && 
                            x.Timestamp.TimeOfDay.Minutes == randomGoodTime.Minutes).FirstOrDefault();

            return (message is null) 
                ? $"There has never been a message sent at {randomGoodTime.ToString(@"hh\:mm")} in this channel."
                : $"The last person to send a message at {randomGoodTime.ToString(@"hh\:mm")} was `{message.Author}`.\nThe message was sent at {message.CreatedAt.Date.ToShortDateString()} and reads:\n```{message.Content}```";
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

        private string AverageMessageLength()
        {
            var messageLengths = _channelMessages.Select(x => x.Content.Length).ToArray();
            return $"The average length for one message is {messageLengths.Average().ToString("0.00")} characters in this channel.";
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

        private string AverageUsernameLength()
        {
            var usernameLengths = _serverUsers.Select(x => x.Username.Length).ToArray();
            return $"The average length for a username in this server is {usernameLengths.Average().ToString("0.00")} characters.";
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

        private string AverageNicknameLength()
        {
            var nicknameLengths = _serverUsers.Where(x => x.Nickname != null).Select(x => x.Nickname.Length).ToArray();
            return $"The average length for a nickname in this server is {nicknameLengths.Average().ToString("0.00")} characters.";
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
