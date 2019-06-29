using Discord;
using Discord.Commands;
using MediocreBot.Entities;
using System.Threading.Tasks;

namespace MediocreBot.Modules
{
    public class InfoCommand : ModuleBase<SocketCommandContext>
    {
        private readonly BotConfiguration _botConfiguration;

        public InfoCommand()
        {
            _botConfiguration = MediocreBot.Instance.DataStorage.GetBotConfiguration();
        }

        [Command("info")]
        [Alias("help")]
        public async Task Info()
        {
            var botUser = Context.Client.CurrentUser;

            var embedBuilder = new EmbedBuilder()
                .WithTitle("__About me__")
                .WithDescription("Uh, h..hey. I know some things about your discord server. They're not very interesting things, so I was named MediocreBot. If you want to hear my mediocre facts, just tell me `fact` and I'll know.")
                .WithColor(100, 180, 255)
                .WithThumbnailUrl(botUser.GetAvatarUrl(size: 64))
                .AddField("How?", $"Say \"{botUser.Mention} fact\" *or* \"{_botConfiguration.Prefix}fact\" to get a random mediocre fact about your discord server. You can also provide an optional target channel to target a different channel than the one you are in. Example usage: \"{_botConfiguration.Prefix}fact #channel-name\"")
                .AddField("Why?", "I was created in four days for [Discord Community Hack Week 2019](https://dis.gd/hackweek). Apparently I'm in the \"shitpost\" category :slight_frown:")
                .AddField("Who?", "My [source code](https://github.com/LeMorrow/mediocre-discord-bot \"GitHub\") (and beautiful face) is made by [Marcus Otterström](https://github.com/LeMorrow \"GitHub\").");

            await ReplyAsync("", embed: embedBuilder.Build());
        }
    }
}
