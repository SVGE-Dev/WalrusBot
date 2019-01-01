using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security;
using System.IO;
using MimeKit;
using MailKit.Net.Smtp;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;

namespace WalrusBot.Modules
{
    [Group("verify")]
    public class Verify : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task DefaultVerifyAsync()
            => await HelpVerifyAsync();

        public static SecureString UserDataEncryptKey = new SecureString();  // this is horrendous
        private static Random random = new Random();
        private static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private static string ApplicationName = "Walrus Bot";
        private static string spreadsheetId = "11ogdfFitHG6OLQ6-EjWmTpHQXFs_s6N1hjQDt6u1O8M";
        private static string sheetRange = "Sheet_a!A2:E";

        #region Help
        [Command("help")]
        public async Task HelpVerifyAsync()
        {
            await ReplyAsync("Verify help");
        }
        #endregion

        #region Commands 

        [Command("reqeustverify")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task MessageNonVerifiedAsync()  // message all non-verified members
        {

        }

        [Command("email")]
        public async Task EmailVerifyAsync([Remainder]string emailAddr)
        {
            if (!Context.IsPrivate)  // delete it if it's not a private message!
            {
                await Context.Message.DeleteAsync();
                await Context.Message.Author.SendMessageAsync("Hi there! Please don't send your email to a public channel! Don't worry; I've deleted it off. To verify your email, just DM me here!");
                return;
            }
            await ReplyAsync("Sending email..!");
            // need to do a quick check here that it's a valid email address
            // check that the file exists

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("SVGE", Program._config["accGmail"]));
            msg.To.Add(new MailboxAddress(Context.User.Username, emailAddr));
            msg.Subject = "SVGE: Your verification code!";
            BodyBuilder body = new BodyBuilder();
            using (StreamReader sr = new StreamReader(Program._config["verifEmail"]))
            {
                body.HtmlBody = sr.ReadToEnd().Replace("xXxCODEHERExXx", RandomString(16));
            }
            msg.Body = body.ToMessageBody();

            using (SmtpClient client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;  // literatlly not idea what this does...
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync(Program._config["accGmail"], Program._config["pwGmail"]);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);  // idk if awaiting all of this is a good idea...
            }
            await ReplyAsync("Email sent!");

            UserCredential credential;
            using (var fs = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(fs).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            //Test code
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, sheetRange);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                Console.WriteLine("Name, Major");
                foreach (var row in values)
                {
                    // Print columns A and E, which correspond to indices 0 and 4.
                    Console.WriteLine("{0}, {1}", row[0], row[4]);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
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
