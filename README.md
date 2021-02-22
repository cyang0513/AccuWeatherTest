# AccuWeatherTest

In testsettings.json, you can define a bunch of things to make the test more flexable.

For example:

* "StockholmKey": "314929" //The location key of Stockholm
* "AthensKey": "182536"    //The location key of Athens
* "ApiKey": "EDkUiFhgPMVY6XE91AjhXhWIMaDeCA24"  //You API key, max 50 request per day
* "ColdThreshold": 15   //The threshold of "cold"
* "UseMetric": "true"   //If use Metric in the query
* "QueryStringBase": "http://dataservice.accuweather.com/forecasts/v1/daily/5day/"  //Base query string

Theoretically the test can be setup for any two cities, and the "code" temperature is defined as the Max temperature of the day is below "ColdThreshold" you setup.

It's running on DevOps.
[![Build Status](https://dev.azure.com/ChengkaiYang/MyApiTest/_apis/build/status/cyang0513.AccuWeatherTest?branchName=master)](https://dev.azure.com/ChengkaiYang/MyApiTest/_build/latest?definitionId=1&branchName=master)
