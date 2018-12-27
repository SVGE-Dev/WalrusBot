using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using System;

namespace WalrusBot.Modules
{
    [Group("reactrole")]
    [Alias("rr")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(GuildPermission.AddReactions | GuildPermission.ManageRoles)]
    public class ReactRole : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Alias("help")]
        public async Task DefaultReactroleAsync()
        {
            await ReplyAsync("Commands will eventually get listed here in a help doc.");
        }

        [Command("create")]
        public async Task CreateReactroleAsync(SocketGuildChannel channel, params string[] options)
        {
            if (options.Length > 20)
            {
                await ReplyAsync($"There are too many options there. Discord limits the number of reactions on a single message to 20. You put {options.Length}.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithAuthor(Context.Client.CurrentUser).WithDescription("Select a reaction below to get the role for that game!").WithFooter("React-for-Role Embed");

            foreach (string option in options)
            {
                string[] optParts = option.Split('/');
                if (optParts.Length != 3)
                {
                    await ReplyAsync("Something went wrong with one of your options! They should be enclosed in quotes and fit the form '[role name]/[emoji]/[role mention]'.");
                    return;
                }

                builder.AddField(optParts[0], $"{optParts[1]} {optParts[2]}");
            }

            var c = Context.Client.GetChannel(channel.Id) as IMessageChannel;
            IUserMessage msg = await c.SendMessageAsync("", false, builder.Build());

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {

                foreach (string option in options)
                {
                    string em = option.Split('/')[1];
                    Emote emote;
                    if (Emote.TryParse(em, out emote))
                    {
                        await msg.AddReactionAsync(emote);
                    }
                    else
                    {
                        try
                        {
                            Emoji emoji = new Emoji(em);
                            await msg.AddReactionAsync(emoji);
                        }
                        catch
                        {
                            await ReplyAsync($"{em} is not a valid emoji or emote!");
                            return;
                        }
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static async Task RrAddRoleAsync(IEmbed embed, SocketReaction reaction)
        {
            EmbedField field = embed.Fields.First(f => f.Value.StartsWith(reaction.Emote.ToString()));
            int atIndex = field.Value.IndexOf('@');
            ulong roleId = Convert.ToUInt64(field.Value.Remove(0, atIndex + 2).TrimEnd('>').ToString());
            IGuildUser user = reaction.User.Value as IGuildUser;
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
    }
}
