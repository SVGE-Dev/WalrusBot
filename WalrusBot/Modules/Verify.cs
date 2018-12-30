using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace WalrusBot.Modules
{
    [DontAutoLoad]
    [Group("verify")]
    class Verify : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task DefaultVerifyAsync()
            => await HelpVerifyAsync();

        public static SecureString UserDataEncryptKey = new SecureString();  // this is horrendous
        private static Random random = new Random();

        #region Help
        [Command("help")]
        public async Task HelpVerifyAsync()
        {

        }
        #endregion

        #region Commands
        [Command("reqeustverify")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task MessageNonVerifiedAsync()  // message all non-verified members
        {

        }

        [Command("email")]
        public async Task EmailVerifyAsync()
        {
            if(!Context.IsPrivate)  // delete it if it's not a private message!
            {
                await Context.Message.DeleteAsync();
                await Context.Message.Author.SendMessageAsync("Hi there! Please don't send your email to a public channel! Don't worry; I've deleted it off. To verify your email, just DM me here!");
                return;
            }
        }

        #endregion

        #region Private Functions
        private async Task UpdateMemberInfoAsync()
        {

        }
        #endregion

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
