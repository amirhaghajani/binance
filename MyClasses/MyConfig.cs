using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class MyConfig
{
    static SystemConfig item;

    public static void LoadJson()
    {
        using (StreamReader r = new StreamReader("00Config.json"))
        {
            string json = r.ReadToEnd();
            item = JsonConvert.DeserializeObject<SystemConfig>(json);
        }
    }

    public static SystemConfig Config => item;

}

public class SystemConfig
{
    public int gmailCheckPeriod_in_second { get; set; }
    public int validDuration_in_minute { get; set; }

    public string currency_name { get; set; }


    public string currency_base__BTC { get; set; }
    public decimal currency_base__BTC_minimum_order { get; set; }
    public int currency_base__BTC_number_of_zero_after_point { get; set; }



    public string currency_second__USDT { get; set; }
    public decimal currency_second__USDT_minimum_order { get; set; }
    public int currency_second__USDT_number_of_zero_after_point { get; set; }

    public string bainanc_api_key { get; set; }
    public string bainanc_secret_key { get; set; }

    public decimal priceGrowthPercent_comparedWithBasePrice_forChangeBasePrice{get;set;}
    public decimal priceFallPercent_comparedWithBasePrice_forSell{get;set;}

    public int? numberOfEmailCheck{get;set;}
}