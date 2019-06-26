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
        private delegate string MediocreFact();
        private readonly List<MediocreFact> _mediocreFacts;

        private SocketGuildUser[] _serverUsers;
        private List<IMessage> _lastThousandMessages;

        private FactCommand()
        {
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
                await ReplyAsync("Uh.. I'm not very smart. You need to send this in the server you want the fact in.");
                return;
            }

            var sentMessage = await ReplyAsync("Alright, let me think for a bit...");

            // Ultimately this would be cached or something but I can't be bothered atm.
            // Cache in dictionary with channel ID, then add the latest 100 messages since the [0]th one
            _lastThousandMessages = (await Context.Channel.GetMessagesAsync(1000)
                .FlattenAsync())
                .Where(x => !x.Author.IsBot)
                .ToList();

            _serverUsers = Context.Guild.Users.ToArray();

            var random = new Random();
            await sentMessage.ModifyAsync(x => x.Content = $"Okay. {_mediocreFacts[random.Next(0, _mediocreFacts.Count)]()}");
        }

        private string LongestMessage()
        {
            var longestMessage = _lastThousandMessages.OrderByDescending(x => x.Content.Length).FirstOrDefault();
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
