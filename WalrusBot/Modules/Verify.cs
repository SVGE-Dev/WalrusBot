using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;
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
using System.Text;

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
            #region Checks
            if (!Context.IsPrivate)  // delete it if it's not a private message!
            {
                await Context.Message.DeleteAsync();
                await Context.Message.Author.SendMessageAsync("Hi there! Please don't send your email to a public channel! Don't worry; I've deleted it off. To verify your email, just DM me here!");
                return;
            }
            await ReplyAsync("Sending email..!");
            // need to do a quick check here that it's a valid email address
            // check that the HTML file exists
            // check that they're not already in the spreadsheet i.e. verified or have a code assigned
            #endregion

            #region Record Data
            // put email into sheet with verification code and discord id
            string encryptedEmail = Encrypt.EncryptString(emailAddr, UserDataEncryptKey.ToString() );
                
            #endregion

            #region Send email
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
                await Program.Gmail.Users.Messages.Send(gMessage, "me").ExecuteAsync();
            }
            catch (Exception e)
            {
                await ReplyAsync("There was an issue with sending your email! Try again in a few minutes, and if the problem persists then please contact a committee member.");
                Console.WriteLine($"Exception when sending an email: {e.ToString()}");
            }
            #endregion

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

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string MimeToGmail(string msg)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(msg);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        #region Subclasses
        public static class Encrypt
        {
            // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
            // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
            private const string initVector = "pemgail9uzpgzl88";
            // This constant is used to determine the keysize of the encryption algorithm
            private const int keysize = 256;
            //Encrypt
            public static string EncryptString(string plainText, string passPhrase)
            {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                return Convert.ToBase64String(cipherTextBytes);
            }
            //Decrypt
            public static string DecryptString(string cipherText, string passPhrase)
            {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
        }
        #endregion
    }
}
