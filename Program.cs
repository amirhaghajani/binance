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

namespace binance
{
    class Program
    {
        static decimal? lastBuyPrice = null;
        static void Main(string[] args)
        {
            try
            {
                MyConsole.WriteLine_Info("------------------------------------------------------------------");
                MyConsole.WriteLine_Info("Start Program for first ------------------------------------------");
                while (true)
                {
                    start();
                }

            }
            catch (Exception ex)
            {
                MyConsole.WriteLine_Exception("" , ex);
                Console.WriteLine();


                start();
            }

            Console.WriteLine("end -----------------------------");
            Console.ReadLine();
        }

        static void start()
        {

            MyConfig.LoadJson();

            var validEmailTimeInMinute = MyConfig.Config.validDuration_in_minute;
            var CheckEmailDurationInSecond = MyConfig.Config.gmailCheckPeriod_in_second;

            DateTime startTime = DateTime.Now;
            decimal minBTC = MyConfig.Config.currency_base__BTC_minimum_order;
            decimal minUSDT = MyConfig.Config.currency_second__USDT_minimum_order;

            MyConsole.WriteLine_Info("\nStart on " + DateTime.Now + "--------------\n");

            MyBinance.CheckThereIsNotOpenOrders();

            decimal USDT = 0;
            decimal BTC = 0;
            MyBinance.GetBalance(out USDT, out BTC);



            MyConsole.WriteLine_Info($"Wallet Balance=> {MyConfig.Config.currency_second__USDT}: {USDT} {MyConfig.Config.currency_base__BTC}: {BTC}");

            if (USDT >= minUSDT)
            {
                MyConsole.WriteLine_Info("I going to buy .....");

                var ans = MyGmail.CheckBuyEmail("buy", validEmailTimeInMinute, CheckEmailDurationInSecond, startTime);
                if (ans.HasValue)
                {
                    decimal? buyPrice = MyBinance.PlaceOrder("buy", USDT);
                    if (buyPrice.HasValue) lastBuyPrice = buyPrice;
                }
            }

            if (BTC >= minBTC)
            {
                MyConsole.WriteLine_Info("I going to sell ......");

                var ans = MyGmail.CheckBuyEmail("sell", validEmailTimeInMinute, CheckEmailDurationInSecond, startTime);
                if (ans.HasValue)
                {
                    decimal? sellPrice = MyBinance.PlaceOrder("sell", BTC);
                }
            }

            // // if (BTC >= minBTC)
            // // {
            // //     MyConsole.WriteLine_Info("I going to sell by checking price ..........");

            // //     if (!lastBuyPrice.HasValue) lastBuyPrice = MyBinance.GetLastBuyOrderPrice();
            // //     if (!lastBuyPrice.HasValue) lastBuyPrice = MyBinance.GetPrice();
            // //     MyConsole.WriteLine_Info($"\n\n'Last Buy Price' is: {lastBuyPrice}\n\n");

            // //     var selled = false;

            // //     while (!selled)
            // //     {
            // //         var price = MyBinance.GetPrice();
            // //         MyConsole.WriteLine_Info($"\n\nLastBuyPrice: {lastBuyPrice} Current price: {price}\n\n");
            // //         var growPercent = MyConfig.Config.priceGrowthPercent_comparedWithBasePrice_forChangeBasePrice;
            // //         if (price >= lastBuyPrice + (lastBuyPrice.Value * growPercent))
            // //         {
            // //             MyConsole.WriteLine_Info($"\n\n{growPercent}% grow. I change base price to {price}. Have a good Trade ...\n\n");
            // //             lastBuyPrice = price;
            // //         }
            // //         else if (price <= lastBuyPrice - (lastBuyPrice.Value * MyConfig.Config.priceFallPercent_comparedWithBasePrice_forSell))
            // //         {
            // //             MyConsole.WriteLine_Info($"\n\nLast Buy Price is {lastBuyPrice} current price is {price}. Must sell now\n\n");
            // //             decimal? sellPrice = MyBinance.PlaceOrder("sell", BTC);
            // //             selled = true;
            // //         }
            // //         else
            // //         {
            // //             MyConsole.WriteLine_Info($"We are Monitoring price. Check price after 2 seconds");
            // //             MyThread.GenerateConsoleWaiting(2);
            // //         }
            // //     }

            // //     // var ans = MyGmail.CheckBuyEmail("sell", validEmailTimeInMinute, CheckEmailDurationInSecond, startTime);
            // //     // if (ans.HasValue)
            // //     // {
            // //     //     decimal? sellPrice = MyBinance.PlaceOrder("sell", BTC);
            // //     // }
            // // }

            if (BTC < minBTC && USDT < minUSDT)
            {
                MyConsole.WriteLine_Error("\n\nErrore - Please Check your balance. Cannot buy or sell.");
                MyConsole.WriteLine_Error($"Must have min {minUSDT} USDT for Buy or min {minBTC} BTC for Sell");
                MyConsole.WriteLine_Error("\n\nRecheck on 1 Minute.");

                MyThread.GenerateConsoleWaiting(60);
            }

        }
    }
}
