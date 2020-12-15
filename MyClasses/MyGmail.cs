using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.IdentityModel.Tokens;
using Binance.Net;

public class MyGmail
{
    static string[] Scopes = { GmailService.Scope.GmailReadonly };
    static string ApplicationName = "Binance Desktop";

    private static GmailService CreateGmailService(string googleCredentialJsonPathName, string tokenJsonFileName)
    {
        UserCredential credential;

        using (var stream =
                        new FileStream(googleCredentialJsonPathName, FileMode.Open, FileAccess.Read))
        {
            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
            string credPath = tokenJsonFileName;
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            //Console.WriteLine("Credential file saved to: " + credPath);
        }

        // Create Gmail API service.
        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        return service;
    }

    internal static DateTime? CheckBuyEmail(string type, int validMinuteDuration, double recheckEmail_second, DateTime startTime)
    {
        var validSecondDuration = validMinuteDuration * 60;
        var searchSubject = "Alert: buy signal!!!";
        var tokenAddress = "token_buy.json";
        var credentialFile = "gmail_Buy_cridential.json";

        if (type == "sell")
        {
            searchSubject = "Alert: sell signal!!!";
            tokenAddress = "token_sell.json";
            credentialFile = "gmail_Sell_cridential.json";
        }


        DateTime? lastMessageDate = null;
        var co = 1; var maxChack = MyConfig.Config.numberOfEmailCheck ?? 5;
        bool mustContinue = true;
        while (mustContinue && co <= maxChack)
        {
            MyConsole.WriteLine_Info($"\n\nChecking Email ${co} of ${maxChack} \n\n");
            mustContinue = false;

            var hasException = false;
            MyThread.StartWithThread(() =>
            {
                hasException = checkEmail(credentialFile, tokenAddress, searchSubject, out lastMessageDate);
            }, 60, "Check Gmail");

            if (hasException)
            {
                mustContinue = true;
                MyConsole.WriteLine_Error("\nThere is some error on Check Gmail.\n I will recheck on 60 second !!!\n");
                MyThread.GenerateConsoleWaiting(60);
            }

            if (!hasException && !lastMessageDate.HasValue)
            {
                //کلا ایمیلی یافت نشد
                mustContinue = true;
                MyConsole.WriteLine_Info($"Dont Found any Email. Recheck on {recheckEmail_second} seconds ");
                MyThread.GenerateConsoleWaiting(recheckEmail_second);
            }

            if (!hasException && lastMessageDate.HasValue)
            {
                var a = DateTime.Now.Subtract(lastMessageDate.Value).TotalSeconds;
                if (a > validSecondDuration)
                {
                    mustContinue = true;
                    MyConsole.WriteLine_Error($"\nEmail time Expired. More than {validSecondDuration} seconds have elapsed since the email arrived {a}\n");
                    MyConsole.WriteLine_Info($"Recheck on {recheckEmail_second} seconds");
                    MyThread.GenerateConsoleWaiting(recheckEmail_second);
                }
                else
                {
                    if (lastMessageDate.Value > startTime) return lastMessageDate.Value;

                    MyConsole.WriteLine_Error($"\n\nEmail after start. start:{startTime} emailTime:{lastMessageDate.Value}. I wait {recheckEmail_second} for another one.\n\n");
                    MyThread.GenerateConsoleWaiting(recheckEmail_second);
                    mustContinue = true;
                }
            }
            co++;
        }

        return null;
    }

    private static bool checkEmail(string googleCredentialJsonPathName, string tokenJsonFileName, string emailSubject, out DateTime? lastMessageDate)
    {
        lastMessageDate = null;
        try
        {
            GmailService service = MyGmail.CreateGmailService(googleCredentialJsonPathName, tokenJsonFileName);
            var mm = service.Users.Messages.List("me");
            mm.MaxResults = 1;
            mm.Q = $"subject:{emailSubject}";
            //var mmm2 = mm.Execute();


            var test = mm.ExecuteAsync();
            var sub = "";
            var date = "";

            if (test.Result == null) return false;

            foreach (var item in test.Result.Messages)
            {
                var ff = service.Users.Messages.Get("me", item.Id).Execute();

                foreach (var head in ff.Payload.Headers)
                {
                    if (head.Name == "Date") date = head.Value;
                    if (head.Name == "Subject") sub = head.Value;
                }

                lastMessageDate = DateTime.Parse(date);

                MyConsole.WriteLine_Info($"\n\nFound Email=> Subject: {sub} Time: {lastMessageDate.ToString()}\n");
            }

            return false;
        }
        catch (Exception ex)
        {
            MyConsole.WriteLine_Exception("Check Gmail", ex);
            return true;
        }

    }
}