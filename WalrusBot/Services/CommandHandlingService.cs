using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WalrusBot
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService _commands;
        private IServiceProvider _provider;

        #region Command Handling Service
        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient client, CommandService commands)
        {
            discordClient = client;
            _commands = commands;
            _provider = provider;

            discordClient.MessageReceived += MessageReceived;
            discordClient.ReactionAdded += ReactionAdded;
            discordClient.ReactionRemoved += ReactionRemoved;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);  // change this
            // Add additional initialization code here...
        }
        #endregion

        #region Event Handlers
        private async Task MessageReceived(SocketMessage rawMessage)
        {
        }

        
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
        }
        #endregion
    }
}
