using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalrusBot.Modules
{
    [DontAutoLoad]
    [Group("help")]
    class Help : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task HelpListAsync()
        {
            await ReplyAsync("HELP");
        }
    }
}
