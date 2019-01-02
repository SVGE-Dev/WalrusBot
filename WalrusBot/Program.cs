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
        #region Main
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public static IConfiguration _config;
        public static ConcurrentDictionary<string, string> Config = new ConcurrentDictionary<string, string>(Environment.ProcessorCount * 2, 503);  // capacity has to be a prime for some reason
        public static UserCredential Credential;  // fuck thread safety
        private static string[] Scopes = {
            SheetsService.Scope.Spreadsheets,
            DriveService.Scope.DriveReadonly,
            GmailService.Scope.GmailSend
        };

        public async Task MainAsync()
        {
            _config = BuildConfig();

            #region Google Auth
            using (var fs = new FileStream(_config["credFile"], FileMode.Open, FileAccess.Read))
            {
                Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(fs).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(_config["credPath"], true)).Result;
            }
            var sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = _config["appName"]
            });

            SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetService.Spreadsheets.Values.Get(_config["dataSheetId"], _config["configRange"]);
            ValueRange response = await request.ExecuteAsync();
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                Console.WriteLine("Config:");
                foreach (var row in values)
                {
                    Console.WriteLine($"    {row[0]}: {row[1]}");
                    Config.TryAdd(row[0] as string, row[1] as string);
                }
            }
            Console.WriteLine();
            #endregion

            #region Encryption Password
            using (MD5 hasher = MD5.Create())  // idk if I could just pass a new MD5 to the hashToString but eh I'll leave it like this for now
            {
                string pw;
                if (!System.IO.File.Exists("password.txt"))   //  eventually put this into data! on the spreadsheet, and means I can get rid of "system"
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

                    using (StreamWriter sw = System.IO.File.CreateText("password.txt"))
                    {
                        sw.WriteLine(hashToString(hasher, pw));
                    }
                }
                else
                {
                    string hash = System.IO.File.OpenText("password.txt").ReadLine();
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
                    //Verify.UserDataEncryptKey.AppendChar(c);
                }
                Console.WriteLine("\n");
            }
            #endregion

            #region Download the HTML file. 
            // eventually this'll all go into a nice subroutine
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = _config["appName"],
            });

            var expReq = driveService.Files.Export(Config["verifyEmailHtmlId"], "text/plain");
            var stream = new MemoryStream();
            expReq.MediaDownloader.ProgressChanged += (IDownloadProgress progress)
                =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Verification email file has been downloaded!\n");
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine("The download of the verification email file has failed! D: Try adding it manually!");
                            break;
                        }
                }
            };
            await expReq.DownloadAsync(stream);

            using (FileStream fs = new FileStream(Config["verifyEmailHtmlName"], FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.WriteTo(fs);
            }
            #endregion

            _client = new DiscordSocketClient();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, Config["botToken"]);
            await _client.StartAsync();

            await Task.Delay(-1);

            #region subscribe to updates to desired files, including the html one
            //ChangesResource.WatchRequest wr = new ChangesResource.WatchRequest();
            #endregion
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