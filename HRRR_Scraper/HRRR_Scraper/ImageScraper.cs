using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace HRRR_Scraper
{
    class ImageScraper
    {
        public List<SmokeForecast> ScrapeHRRRData()
        {
            ChromeDriver Driver = new ChromeDriver();
            List<SmokeForecast> scrapedForecasts = new List<SmokeForecast>() {
                new SmokeForecast { region = "SW", imgURLs = new List<string>() },
                new SmokeForecast { region = "NW", imgURLs = new List<string>() } };

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            Driver.Manage().Window.Maximize();

            string url = "https://rapidrefresh.noaa.gov/hrrr/HRRRsmoke/";
            Driver.Navigate().GoToUrl(url);

            //Select previous forecast because usually the current one is not fully populated until the end of the hour.
            Driver.FindElement(By.CssSelector("#dateDiv > select")).Click();
            Driver.FindElement(By.CssSelector("#dateDiv > select > option:nth-child(2)")).Click();

            string rawDate = Driver.FindElement(By.CssSelector("#content > h2:nth-child(3)")).Text.ToString();

            string smokeMapSelector = "/html/body/div/div[2]/div[2]/div[1]/table/tbody/tr[7]/td[2]/a";

            foreach (var forecast in scrapedForecasts)
            {
                //open region menu
                Driver.FindElement(By.XPath("/html/body/div/div[2]/div[2]/div[1]/form/div[2]/select")).Click();
                Driver.FindElement(By.XPath($"/html/body/div/div[2]/div[2]/div[1]/form/div[2]/select/option[{(forecast.region == "NW" ? "2" : "5")}]")).Click();

                var imgs = Driver.FindElement(By.XPath(smokeMapSelector));
                Driver.Navigate().GoToUrl(imgs.GetAttribute("href").ToString());

                var imgTable = Driver.FindElement(By.XPath("/html/body/div/div[2]/div[2]/table/tbody"));
                var images = imgTable.FindElements(By.TagName("img"));

                foreach (var img in images)
                {
                    string imgURL = img.GetAttribute("src");
                    forecast.imgURLs.Add(imgURL);
                }

                Driver.Navigate().Back();
            }

            // Sometimes the site is not updated for a while, so if these forecasts in scrapedForecasts are in our log file we do not bother with them.
            List<SmokeForecast> forecasts = new List<SmokeForecast>();

            foreach (var forecast in scrapedForecasts)
            {
                forecast.ParseAndSetDates(rawDate);

                if (SmokeBot.completed.Contains($"{forecast.forecastID}_{forecast.region}"))
                    continue;
                else
                {
                    forecasts.Add(forecast);
                    forecast.forecastDir = $@"{SmokeBot.baseDirectory}\forecasts\{forecast.forecastID}_{forecast.region}";
                    CreateForecastDirectory(forecast);
                    DownloadForecastImages(forecast);
                }
            }

            Driver.Close();
            Driver.Quit();

            return forecasts;
        }

        private void DownloadForecastImages(SmokeForecast forecast)
        {
            using (WebClient client = new WebClient())
            {
                foreach (string imgURL in forecast.imgURLs)
                {
                    string fileName = Regex.Replace(imgURL, @".*\/([\d]{10})(\/[A-Z]{2}\/).*([\d]{3}\.png)", "$1$2$3").Replace('/', '_');

                    client.DownloadFile(new Uri(imgURL), $@"{forecast.forecastDir}\{fileName}");
                    Thread.Sleep(200);             //Add sleeps to avoid spamming server
                }
            }
        }

        private void CreateForecastDirectory(SmokeForecast forecast)
        {
            System.IO.Directory.CreateDirectory(forecast.forecastDir);
        }
    }
}