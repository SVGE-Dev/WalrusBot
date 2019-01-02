/*using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalrusBot.Modules
{
    [DontAutoLoad]
    [Group("admin")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task DefaultAdminAsync()
            => await HelpAdminAsync();

        [Command("help")]
        public async Task HelpAdminAsync()
        {

        }

        [Command("backup")]
        [RequireBotPermission(GuildPermission.ManageEmojis | GuildPermission.ManageRoles)]  // this should be enough permissions? Plus it already has these for react roles. May need to be able to manage channels
        public async Task BackupServerAsync()  // I think it **should** be possible to to see every user and their roles, but needs to be able to see every channel
        {

        }

        [Command("restore")]
        [RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageRoles | GuildPermission.ManageNicknames | GuildPermission.ManageChannels | GuildPermission.ManageEmojis)]
        public async Task RestoreServerAsync()
        {
            await UpdateAsync();  // run an update after restoring the server
        }

        [Command("update")]
        [RequireBotPermission(GuildPermission.ManageRoles)]  // I'll add other perms as they become relavent
        public async Task UpdateAsync()  // possibly put this under Verify. Or call Verify's update as part of this
        {
            /*
             * Instead of verifying student state, get people to get membership first, then verify against this database;
             * This encourages more people to get membership, whilst also easily adding support for associate members.
             * Remove the "Guest" role as this is kinda redundant and also restricts our ability to prune.
             * Need an alumni role.
             */
            // ensure that everyone who is verified has the student role
            // assign relavent role to those who have membership
            // remove all roles from people who aren't members
     /*   }

        [Command("listroles")]
        
        public async Task ListRoles()  // list all roles and how many people are in them
        {

        }

        [Command("listroles")]
        public async Task ListRoles(string rolecolour)  // this one can be used to list out only the games by specifying their color code i.e. for getting all of the game roles
        {

        }

        [Command("purge")]
        public async Task Purge(ushort num)  // delete a given number of messages from the channel. Max 255 at a time.
        {

        }
    }
}
*/