using System.Threading.Tasks;
using Discord.Commands;

namespace WalrusBot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task DefaultReactRoleAsync()
            => await ReplyAsync("https://youtu.be/X2jjf_XRpKc?t=7");

        [Command("info")]
        public async Task Info()
            => await ReplyAsync($"Hello, I am a bot called **{Context.Client.CurrentUser.Username}** written in **Discord.Net 1.0.2**!\n");

        [Command("GDPR")]
        public async Task Gdpr()
            => await ReplyAsync(Program._config["GDPR_Message"]);

        [Command("website")]
        [Alias("site")]
        public async Task Website()
            => await ReplyAsync("https://www.svge.susu.org");

        [Command("bork")]
        public async Task Bork()
            => await ReplyAsync("Woof woof", true);
    }
}
