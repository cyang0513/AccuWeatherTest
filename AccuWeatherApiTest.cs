using System;
using System.Collections;
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
      string m_QueryString;
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
         m_QueryString = m_Config.GetSection("QueryStringBase").Value;
      }

      /// <summary>
      /// Define "cold" as maximum temperature is lower than m_ColdThreshold
      /// </summary>
      [Test, Description("Assert this week Stockholm is cold"), Author("Chengkai")]
      public void TestSthlmWeatherColdInThisWeek()
      {
         var useMetric = m_Config.GetSection("UseMetric").Value;
         var metricPostfix = Boolean.TrueString.ToLower() == useMetric ? "C" : "F";
         var querySto = $"{m_QueryString}{m_StoKey}?apikey={m_ApiKey}&metric={useMetric}";

         using var client = new HttpClient();
         var stoResultDict = Get5DayResult(client, querySto);

         Assert.AreEqual(5, stoResultDict.Count, "There are 5 days forecasts returned");
         Assert.IsTrue(stoResultDict.Values.All(x=> x <= m_ColdThreshold), $"This week is not cold. Not all days max temperature is below {m_ColdThreshold} {metricPostfix}");
      }

      [Test, Description("Assert in next 5 days, the max temperature in Stockholm is lower than that in Athens"), Author("Chengkai")]
      public void Test5DaySthlmMaxTemperatureColderThanAthens()
      {
         var querySto = $"{m_QueryString}{m_StoKey}?apikey={m_ApiKey}";
         var queryAthens = $"{m_QueryString}{m_AthensKey}?apikey={m_ApiKey}";

         using var client = new HttpClient();

         var stoResultDict = Get5DayResult(client, querySto);
         var athensResultDict = Get5DayResult(client, queryAthens);

         Assert.AreEqual(5, stoResultDict.Count, "There are 5 days forecasts returned for Stockholm");
         Assert.AreEqual(5, athensResultDict.Count, "There are 5 days forecasts returned for Athens");

         CollectionAssert.AreEqual(stoResultDict.Keys, athensResultDict.Keys, "Both contains data for same date");

         for (int i = 0; i < 5; i++)
         {
            Assert.Less(stoResultDict.Values.ElementAt(i), athensResultDict.Values.ElementAt(i), $"Stockholm is warmer than Athens on {stoResultDict.Keys.ElementAt(i)}");
         }
      }

      /// <summary>
      /// Generate 5 days forecast result for assert
      /// </summary>
      /// <param name="client">Http client</param>
      /// <param name="queryStoBase">Api query string</param>
      /// <returns>A dictionary with date as key and max temperature as value</returns>
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