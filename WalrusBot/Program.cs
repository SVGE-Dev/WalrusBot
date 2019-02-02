using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using WalrusBot.Services;
using System.Security.Cryptography;
using WalrusBot.Modules;
using System.Text;

namespace WalrusBot
{
    class Program
    {
        #region Main
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public static IConfiguration _config;

        public async Task MainAsync()
        {
            _config = BuildConfig();
            
            #region Encryption Password
            /*
            using (MD5 hasher = MD5.Create())  // idk if I could just pass a new MD5 to the hashToString but eh I'll leave it like this for now
            {
                string pw;
                if (!File.Exists(_config["passwordFile"]))
                {
                    string confirm;
                    bool pass = false;
                    do
                    {
                        Console.Write("Enter a new password: ");
                        pw = getPassword();
                        Console.Write("\nPlease confirm this password: ");
                        confirm = getPassword();
                        pass = pw == confirm;
                        if (!pass) Console.Write("\nPasswords do not match!\n\n");
                    } while (!pass);

                    using (StreamWriter sw = File.CreateText(_config["passwordFile"]))
                    {
                        sw.WriteLine(hashToString(hasher, pw));
                    }
                }
                else
                {
                    string hash = File.OpenText(_config["passwordFile"]).ReadLine();
                    bool pass = false;
                    do
                    {
                        Console.Write("Please enter the password: ");
                        pw = getPassword();
                        pass = hashToString(hasher, pw) == hash;  // this probably eats memory oopsy
                        if (!pass) Console.WriteLine("\nThat password does not match the previous password used! Please try again");
                    } while (!pass);
                }
                foreach (char c in pw)
                {
                    Verify.UserDataEncryptKey.AppendChar(c);
                }
                Console.WriteLine("\n");
            }*/
            #endregion

            _client = new DiscordSocketClient();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
        #endregion

        #region Config Builders
        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                // Add additional services here...
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }
        #endregion

        #region Utility Functions
        private string getPassword()
        {
            ConsoleKeyInfo key;
            string pwd = "";
            do
            {
                key = Console.ReadKey(true);

                // Ignore any key out of range.
                if (((int)key.Key) >= 65 && ((int)key.Key <= 90))
                {
                    // Append the character to the password.
                    pwd += key.KeyChar;
                    Console.Write("*");
                }
                // Exit if Enter key is pressed.
            } while (key.Key != ConsoleKey.Enter);
            return pwd;
        }

        private string hashToString(HashAlgorithm hasher, string input)
        {
            hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            byte[] re = hasher.Hash;
            StringBuilder sb = new StringBuilder();
            foreach (byte b in re)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        #endregion
    }
}