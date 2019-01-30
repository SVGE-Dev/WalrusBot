using System;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Discord.Commands;

namespace WalrusBot.Modules
{
    [Group("gameserver")]
    public class GameServer : ModuleBase<SocketCommandContext>
    {
        private static ConcurrentDictionary<string, ServerAddr> servers = new ConcurrentDictionary<string, ServerAddr>();

        [Command("add")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddServerAsync(string ip, int port, [Remainder] string name)
        {
            servers.TryAdd(name, new ServerAddr(ip, port));
            await ReplyAsync("Server added!");
        }

        [Command("ping")]
        public async Task ServerPingAsync([Remainder] string name)
        {
            await ReplyAsync($"Pinging the {name} server...");
            try
            {
                ServerAddr addr = servers[name];
                using (TcpClient tcpClient = new TcpClient())
                {
                    var result = tcpClient.BeginConnect(servers[name].ip, servers[name].port, null, null);
                    if (result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        await ReplyAsync($"The {name} server is online!");
                    }
                    else
                    {
                        await ReplyAsync($"The {name} server is offline!");
                    }
                }
            }
            catch
            {
                await ReplyAsync("That server does not exist! Did you spell its name correctly?");
            }
        }


        private struct ServerAddr
        {
            public ServerAddr(string sIp, int sPort)
            {
                ip = sIp;
                port = sPort;
            }
            public string ip;
            public int port;
        }
    }
}