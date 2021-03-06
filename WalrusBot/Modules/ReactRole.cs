﻿using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using System;
using System.Collections.Generic;

namespace WalrusBot.Modules
{
    [Group("reactrole")]
    [Alias("rr")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(GuildPermission.AddReactions | GuildPermission.ManageRoles)]
    public class ReactRole : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task DefaultReactroleAsync()
            => await HelpReactroleAsync();

        #region Help
        [Command("help")]
        public async Task HelpReactroleAsync()
        {
            await ReplyAsync("Commands will eventually get listed here in a help doc.");
        }
        #endregion

        #region Commands
        #region Create
        [Command("create")]
        public async Task CreateReactroleAsync(SocketGuildChannel channel, params string[] options)
            => await CreateReactroleAsync("Game Roles", "https://cdn.iconscout.com/icon/premium/png-256-thumb/game-controller-181-789235.png", "Select a reaction below to get the role for that game!", channel, options);

        [Command("create")]
        public async Task CreateReactroleAsync(string autTitle, string autImgUrl, SocketGuildChannel channel, params string[] options)
            => await CreateReactroleAsync(autTitle, autImgUrl, "Select a reaction below to get the role for that game!", channel, options);

        [Command("create")]
        public async Task CreateReactroleAsync(string autTitle, string autImgUrl, string desc, SocketGuildChannel channel, params string[] options)
        {
            #region Parse Options
            if (options.Length > 20)
            {
                await ReplyAsync($"There are too many options there. Discord limits the number of reactions on a single message to 20. You put {options.Length}.");
                return;
            }

            List<RoleInfo> rifs = new List<RoleInfo>();
            foreach (string opt in options)
            {
                RoleInfo ri = new RoleInfo();
                if (!ri.TryParse(opt))
                {
                    await ReplyAsync("One of your role options is invalid. Please check your syntax!");
                    return;
                }
                rifs.Add(ri);
            }
            #endregion

            #region Generate Message
            EmbedBuilder builder = new EmbedBuilder();
            var author = new EmbedAuthorBuilder().WithName(autTitle).WithIconUrl(autImgUrl);
            builder.WithAuthor(author).WithDescription(desc).WithFooter("React-for-Role Embed");

            foreach (RoleInfo r in rifs)
            {
                builder.AddField(r.Name, $"{r.EmoName} {r.Role}", true);
            }

            var c = Context.Client.GetChannel(channel.Id) as IMessageChannel;
            IUserMessage msg = await c.SendMessageAsync("", false, builder.Build());

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                foreach (RoleInfo r in rifs)
                {
                    await msg.AddReactionAsync(r.Emo);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            #endregion
            await Context.Message.DeleteAsync();
        }
        #endregion
        #endregion

        #region Subclasses
        private class RoleInfo
        {
            public bool TryParse(string roleInfo)
            {
                if (roleInfo.Split('/').Length != 3) return false;

                name = roleInfo.Split('/')[0];
                role = roleInfo.Split('/')[2];

                emoName = roleInfo.Split('/')[1];

                if(emoName.StartsWith("+") )
                {
                    emoName = emoName.TrimStart('+').Replace('+', ':');
                }
                if (!Emote.TryParse(emoName, out emote))
                {
                    try
                    {
                        emoji = new Emoji(emoName);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
            string name;
            string emoName;
            Emote emote;
            Emoji emoji;
            string role;

            public string Name
            {
                get { return name; }
            }
            public IEmote Emo
            {
                get {
                    if (emote != null) return emote;
                    return emoji;
                }
            }
            public string Role
            {
                get { return role; }
            }
            public string EmoName
            {
                get { return emoName; }
            }

        }
        #endregion

        #region Static Functions
        public static async Task RrAddRoleAsync(IEmbed embed, SocketReaction reaction)
        {
            IGuildUser user = reaction.User.Value as IGuildUser;

            if (!user.RoleIds.Contains<ulong>(Convert.ToUInt64(Program._config["RID_Student"]) ) ) return;
            
            EmbedField field = embed.Fields.First(f => f.Value.StartsWith(reaction.Emote.ToString()));
            int atIndex = field.Value.IndexOf('@');
            ulong roleId = Convert.ToUInt64(field.Value.Remove(0, atIndex + 2).TrimEnd('>').ToString());
            IRole role = user.Guild.Roles.First(r => r.Id == roleId);
            await user.AddRoleAsync(role);
        }

        public static async Task RrDelRoleAsync(IEmbed embed, SocketReaction reaction)
        {
            EmbedField field = embed.Fields.First(f => f.Value.StartsWith(reaction.Emote.ToString()));
            int atIndex = field.Value.IndexOf('@');
            ulong roleId = Convert.ToUInt64(field.Value.Remove(0, atIndex + 2).TrimEnd('>').ToString());
            IGuildUser user = reaction.User.Value as IGuildUser;
            IRole role = user.Guild.Roles.First(r => r.Id == roleId);
            await user.RemoveRoleAsync(role);
        }
        #endregion
    }
}
