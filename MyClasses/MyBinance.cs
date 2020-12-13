using System;
using Binance.Net;

public class MyBinance
{
    private static int _waitingForApiAnswer = 30;
    private static int _waitingAfterApiError = 30;
    private static BinanceClient CreateBinanceClient()
    {
        var client = new BinanceClient(new Binance.Net.Objects.Spot.BinanceClientOptions
        {
            ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(
            MyConfig.Config.bainanc_api_key,
            MyConfig.Config.bainanc_secret_key)
        });

        return client;
    }

    public static void GetBalance(out decimal usdt, out decimal btc)
    {
        usdt = 0; btc = 0;
        decimal myUsdt = 0;
        decimal myBtc = 0;

        bool mustContinue = true;
        while (mustContinue)
        {
            mustContinue = false;

            var hasException = false;
            MyThread.StartWithThread(() =>
            {
                hasException = _getBalance(out myUsdt, out myBtc);
            }, _waitingForApiAnswer, "Get My Binance Balance");

            if (hasException)
            {
                mustContinue = true;
                MyConsole.WriteLine_Error($"\nThere is some error on Get Binance Balance.\n I will recheck on {_waitingAfterApiError} second !!!\n");
                MyThread.GenerateConsoleWaiting(_waitingAfterApiError);
            }

            usdt = myUsdt;
            btc = myBtc;
        }
    }
    private static bool _getBalance(out decimal usdt, out decimal btc)
    {
        string baseCurrency = MyConfig.Config.currency_base__BTC;
        string secondCurrency = MyConfig.Config.currency_second__USDT;

        usdt = 0;
        btc = 0;
        try
        {
            BinanceClient binanceClient = MyBinance.CreateBinanceClient();

            var acc = binanceClient.General.GetAccountInfo();

            if (!acc.Success)
            {
                MyConsole.WriteLine_Error("\n\nGetBalance Error: " + acc.Error.Message + "\n\n");
                return true;
            }


            var assets = acc.Data.Balances;

            foreach (var item in assets)
            {
                if (item.Asset == secondCurrency) usdt = item.Total;
                if (item.Asset == baseCurrency) btc = item.Total;
            }
            return false;
        }
        catch (Exception ex)
        {
            MyConsole.WriteLine_Exception("Get Binance Balance", ex);
            return true;
        }
    }


    public static void CheckThereIsNotOpenOrders()
    {
        bool hasOpenOrders = true;
        while (hasOpenOrders)
        {
            var hasException = false;
            MyThread.StartWithThread(() =>
            {
                hasException = MyBinance._checkThereIsNotOpenOrders(out hasOpenOrders);
            }, _waitingForApiAnswer, "Get Binance Open Orders");

            if (!hasException && hasOpenOrders)
            {
                MyConsole.WriteLine_Error($"\n\nThere is open orders. plaese close them to start.\n I will rechech on 120 seconds !!!!\n");

                MyThread.GenerateConsoleWaiting(120);
            }

            if (hasException)
            {
                hasOpenOrders = true;
                MyConsole.WriteLine_Error($"\nThere is some error on Get Binance Open Orders.\n I will recheck on {_waitingAfterApiError} seconds !!!\n");
                MyThread.GenerateConsoleWaiting(_waitingAfterApiError);
            }
        }
    }
    private static bool _checkThereIsNotOpenOrders(out bool hasOpenOrders)
    {
        hasOpenOrders = false;
        try
        {
            BinanceClient binanceClient = MyBinance.CreateBinanceClient();
            var orders = binanceClient.Spot.Order.GetOpenOrders();

            if (!orders.Success)
            {
                MyConsole.WriteLine_Error("\n\nGetOpenOrders Error: " + orders.Error.Message + "\n\n");
                return true;
            }


            var ans = orders.Data;



            foreach (Binance.Net.Objects.Spot.SpotData.BinanceOrder order in ans)
            {
                if (order.Symbol == MyConfig.Config.currency_name ||
                order.Symbol == MyConfig.Config.currency_base__BTC ||
                order.Symbol == MyConfig.Config.currency_second__USDT)
                {
                    hasOpenOrders = true;
                    return false;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            MyConsole.WriteLine_Exception(nameof(CheckThereIsNotOpenOrders), ex);
            return true;
        }

    }


    public static decimal? PlaceOrder(string type, decimal value)
    {
        decimal? orderPrice = null;

        bool mustContinue = true;
        while (mustContinue)
        {
            mustContinue = false;

            var hasException = false;
            MyThread.StartWithThread(() =>
            {
                hasException = _placeOrder(type, value, out orderPrice);
            }, _waitingForApiAnswer, $"Place Bainance Order type:{type} value:{value}");

            if (hasException)
            {
                mustContinue = true;
                MyConsole.WriteLine_Error("\nThere is some error on Place Order.\n I will recheck on {_waitingAfterApiError} seconds !!!\n");
                MyThread.GenerateConsoleWaiting(_waitingAfterApiError);
            }
        }

        return orderPrice;
    }
    static bool _placeOrder(string type, decimal value, out decimal? orderPrice)
    {
        string currencyName = MyConfig.Config.currency_name;

        orderPrice = null;
        try
        {
            BinanceClient binanceClient = MyBinance.CreateBinanceClient();
            if (type == "buy")
            {
                value = _calValue(value, MyConfig.Config.currency_second__USDT_number_of_zero_after_point);

                var answer = binanceClient.Spot.Order.PlaceOrder(currencyName,
                                        Binance.Net.Enums.OrderSide.Buy,
                                        Binance.Net.Enums.OrderType.Market,
                                        null,
                                        value);
                if (!answer.Success)
                {
                    MyConsole.WriteLine_Error("\n\nPlace Order Buy Error: " + answer.Error.Message + "\n\n");

                    return true;
                }

                orderPrice = answer.Data.Price;
            }
            else
            {
                value = _calValue(value, MyConfig.Config.currency_base__BTC_number_of_zero_after_point);

                var answer = binanceClient.Spot.Order.PlaceOrder(currencyName,
                            Binance.Net.Enums.OrderSide.Sell,
                            Binance.Net.Enums.OrderType.Market,
                            value);
                if (!answer.Success)
                {
                    MyConsole.WriteLine_Error("\n\nPlace Order Sell Error: " + answer.Error.Message + "\n\n");
                    return true;
                }

                orderPrice = answer.Data.Price;

            }
            return false;
        }
        catch (Exception ex)
        {
            MyConsole.WriteLine_Exception("Place Binance Order", ex);
            return true;
        }
    }
    private static decimal _calValue(decimal value, int numberOFZeroAfterPoint)
    {
        if (numberOFZeroAfterPoint > 0)
        {
            value = Math.Floor(value * numberOFZeroAfterPoint) / numberOFZeroAfterPoint;
        }
        else
        {
            value = Math.Floor(value);
        }

        return value;
    }




    public static decimal? GetPrice()
    {
        decimal? price = null;

        bool mustContinue = true;
        while (mustContinue)
        {
            mustContinue = false;

            var hasException = false;
            MyThread.StartWithThread(() =>
            {
                hasException = _getPrice(out price);
            }, _waitingForApiAnswer, $"Get Price");

            if (hasException)
            {
                mustContinue = true;
                MyConsole.WriteLine_Error($"\nThere is some error on Get Price.\n I will recheck on {_waitingAfterApiError} seconds !!!\n");
                MyThread.GenerateConsoleWaiting(_waitingAfterApiError);
            }
        }

        return price;
    }
    private static bool _getPrice(out decimal? price)
    {
        price = null;
        try
        {
            BinanceClient binanceClient = MyBinance.CreateBinanceClient();
            var answer = binanceClient.Spot.Market.GetBookPrice(MyConfig.Config.currency_name);

            if (!answer.Success)
            {
                MyConsole.WriteLine_Error("\n\nGet Price Error: " + answer.Error.Message + "\n\n");
                return true;
            }

            var p = (Binance.Net.Objects.Spot.MarketData.BinanceBookPrice)answer.Data;
            price = p.BestBidPrice;

            return false;
        }
        catch (Exception ex)
        {
            MyConsole.WriteLine_Exception("Get price", ex);
            return true;
        }
    }



    public static decimal? GetLastBuyOrderPrice()
    {
        decimal? lastPrice = null;

        bool mustContinue = true;
        while (mustContinue)
        {
            mustContinue = false;

            var hasException = false;
            MyThread.StartWithThread(() =>
            {
                hasException = _getLastBuyOrderPrice(out lastPrice);
            }, _waitingForApiAnswer, $"Get Last Buy OrderPrice");

            if (hasException)
            {
                mustContinue = true;
                MyConsole.WriteLine_Error($"\nThere is some error on GetLastBuyOrderPrice.\n I will recheck on {_waitingAfterApiError} seconds !!!\n");
                MyThread.GenerateConsoleWaiting(_waitingAfterApiError);
            }
        }

        return lastPrice;
    }
    private static bool _getLastBuyOrderPrice(out decimal? lastPrice)
    {
        lastPrice = null;

        try
        {
            BinanceClient binanceClient = MyBinance.CreateBinanceClient();

            var lastTrad = binanceClient.Spot.Order.GetMyTrades(MyConfig.Config.currency_name, null, null, 1);
            if (!lastTrad.Success)
            {
                MyConsole.WriteLine_Error("\n\nGet Last Buy Order Price Error: error on GetMyTrades api service - "
                                + lastTrad.Error.Message + "\n\n");
                return true;
            }

            foreach (Binance.Net.Objects.Spot.SpotData.BinanceTrade item in lastTrad.Data)
            {
                if(!item.IsBuyer){
                    MyConsole.WriteLine_Error("Last Trade was sell not buy. We use current price as 'Last buy price'");
                    return false;
                }
                lastPrice = item.Price;
            }

            return false;
        }
        catch (Exception ex)
        {
            MyConsole.WriteLine_Exception("GetLastBuyOrderPrice", ex);
            return true;
        }
    }
}