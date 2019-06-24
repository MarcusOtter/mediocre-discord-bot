using Discord.Commands;
using System.Threading.Tasks;

namespace MediocreBot.Modules
{
    public class InfoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public async Task Info()
        {
            await ReplyAsync("I am a pretty good bot");
        }
    }
}
