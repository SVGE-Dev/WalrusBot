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
using System.Collections.Concurrent;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using MimeKit.Encodings;

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
            // check that the HTML file exists

            var gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Program.Credential,
                ApplicationName = Program._config["appName"]
            });

            // basic message info
            MimeMessage message = new MimeMessage();
            message.To.Add(new MailboxAddress(Context.User.Username.ToString(), emailAddr));
            message.From.Add(new MailboxAddress(Program.Config["gmailFromName"], Program.Config["gmailFromAddr"]));
            message.Subject = "SVGE Discord Verification Email!";
            // HTML body of email
            var body = new BodyBuilder();
            string htmlString = await File.OpenText(Program.Config["verifyEmailHtmlName"]).ReadToEndAsync();
            body.HtmlBody = htmlString.Replace("xXxCODEHERExXx", RandomString(16));
            message.Body = body.ToMessageBody();

            var gMessage = new Message() { Raw = MimeToGmail(message.ToString()) };
            try
            {
                await gmailService.Users.Messages.Send(gMessage, "me").ExecuteAsync();
            }
            catch (Exception e)
            {
                await ReplyAsync("There was an issue with sending your email! Try again in a few minutes, and if the problem persists then please contact a committee member.");
                Console.WriteLine($"Exception when sending an email: {e.ToString()}");
            }
            await ReplyAsync("Verification email sent! Once you've got your code, send it to me with *svge!verify code* ***[your-code-here]***.");
        }

        [Command("code")]
        public async Task CodeVerifyAsync(string code)
        {
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

        private static string MimeToGmail(string msg)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(msg);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}
