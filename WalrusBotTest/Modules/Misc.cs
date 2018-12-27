using System.Threading.Tasks;
using Discord.Commands;

namespace WalrusBotTest.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task DefaultReactRoleAsync()
            => ReplyAsync("https://youtu.be/X2jjf_XRpKc?t=7");

        [Command("info")]
        public Task Info()
            => ReplyAsync($"Hello, I am a bot called {Context.Client.CurrentUser.Username} written in Discord.Net 1.0.2\n");
    }
}
