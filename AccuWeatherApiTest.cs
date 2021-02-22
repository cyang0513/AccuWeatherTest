using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NUnit.Framework;

namespace AccuWeatherTest
{
   public class AccuWeatherApiTest
   {

      string m_StoKey;
      string m_AthensKey;
      string m_ApiKey;
      int m_ColdThreshold = 0;

      IConfiguration m_Config;

      [SetUp]
      public void Setup()
      {
         m_Config = new ConfigurationBuilder().AddJsonFile("testsettings.json").Build();
         m_StoKey = m_Config.GetSection("StockholmKey").Value;
         m_AthensKey = m_Config.GetSection("AthensKey").Value;
         m_ApiKey = m_Config.GetSection("ApiKey").Value;
         m_ColdThreshold = Convert.ToInt32(m_Config.GetSection("ColdThreshold").Value);
      }


      [Test, Description("Assert this week Stockholm is cold"), Author("Chengkai")]
      public void TestSthlmWeatherColdInThisWeek()
      {

      }

      [Test, Description("Assert in next 5 days, the max temperature in Stockholm is lower than that in Athens"), Author("Chengkai")]
      public void Test5DaySthlmMaxTemperatureColderThanAthens()
      {
         var querySto = $"http://dataservice.accuweather.com/forecasts/v1/daily/5day/{m_StoKey}?apikey={m_ApiKey}";
         var queryAthens = $"http://dataservice.accuweather.com/forecasts/v1/daily/5day/{m_AthensKey}?apikey={m_ApiKey}";

         using var client = new HttpClient();

         var stoResultDict = Get5DayResult(client, querySto);
         var athensResultDict = Get5DayResult(client, queryAthens);

         Assert.AreEqual(5, stoResultDict.Count);
         Assert.AreEqual(5, athensResultDict.Count);

         for (int i = 0; i < 5; i++)
         {
            Assert.AreEqual(stoResultDict.ElementAt(i).Key, athensResultDict.ElementAt(i).Key);
            Assert.Less(stoResultDict.ElementAt(i).Value, athensResultDict.ElementAt(i).Value);
         }
      }

      private Dictionary<string, double> Get5DayResult(HttpClient client, string queryStoBase)
      {
         var apiResult = client.GetStringAsync(queryStoBase);
         apiResult.Wait();

         using var jsonDocResult = JsonDocument.Parse(apiResult.Result);

         var resultDict = new Dictionary<string, double>();
         foreach (var dayResult in jsonDocResult.RootElement.GetProperty("DailyForecasts").EnumerateArray())
         {
            var date = dayResult.GetProperty("Date").GetString().Split("T")[0];
            var value = dayResult.GetProperty("Temperature").GetProperty("Maximum").GetProperty("Value").GetDouble();
            resultDict.Add(date, value);
         }

         return resultDict;
      }
   }
}