using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Sheets.v4;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Download;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace WalrusBot
{
    class Program
    {
        public static SheetsExpanded Config;
        private DiscordSocketClient discordClient;
        private static IConfiguration localConfig;
        private UserCredential userCredential;
        private string[] scopes = {
            SheetsService.Scope.Spreadsheets,
            DriveService.Scope.DriveReadonly,
            GmailService.Scope.GmailSend
        };

        #region Main
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            localConfig = BuildConfig();

            using (var fs = new FileStream(localConfig["credFile"], FileMode.Open, FileAccess.Read))
            {
                userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(fs).Secrets, scopes, "user", CancellationToken.None, new FileDataStore(localConfig["credPath"], true)).Result;
            }

            SheetsExpanded.InitializeGServices(ref userCredential, localConfig["appName"]);
            Config = new SheetsExpanded(localConfig["configSheetID"]);

            await Task.Delay(-1);
        }
        #endregion



        #region Config Builders
        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(discordClient)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(localConfig)
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
        private static string getPassword()
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

        private static string hashToString(HashAlgorithm hasher, string input)
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

    class SheetsExpanded
    {
        private static SheetsService sheetService;
        private static DriveService driveService;

        public static void InitializeGServices(ref UserCredential credential, string appName)
        {
            sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = appName
            });
            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = appName
            });
        }

        private ConcurrentDictionary<string, SSheet> sSheets = new ConcurrentDictionary<string, SSheet>(Environment.ProcessorCount * 2, 503);  // capacity has to be a prime for some reason

        private string sheetID = "";

        public SheetsExpanded(string sid)
        {
            sheetID = sid;
        }
        

        /*
        private async Task PullAsync()  // reads the google sheet and puts its data into it. Probably triggered by a drive event on updates
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = sheetService.Spreadsheets.Values.Get(sheetID, sheetRange);
            ValueRange response = await request.ExecuteAsync();
            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    foreach (string cell in values[i])
                    {
                        Console.Write(cell + "\t");
                    }
                    Console.Write("\n");
                }
            }
        }

        private async Task PushAsync()  // updates the google sheet values
        {
            await PullAsync();
        }*/

        class SSheet
        {
            private string sheetRange = "";
            private bool transpose = false;

        }
    }
}