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
    class HRRR_Scraper
    {
        class RegionForecast
        {
            public string region { get; set; }
            public DateTime forecastStart { get; private set; }
            public DateTime forecastEnd { get; private set; }
            public string forecastID { get; private set; }
            public string timeZone { get; private set; }
            public string forecastDir { get; set; }
            public List<string> imgIDs { get; set; }


            public void ParseAndSetDates(string rawDate)
            {
                Regex rgx = new Regex("([\\d]+)\\s*([A-Za-z]+)\\s*([\\d]{4})\\s-\\s([\\d]+)");
                GroupCollection m = rgx.Matches(rawDate)[0].Groups;

                string day = m[1].Value, month = MonthNumberMap[m[2].Value], yr = m[3].Value, utcHr = m[4].Value;

                DateTime startDate = new DateTime(int.Parse(yr), int.Parse(month), int.Parse(day), int.Parse(utcHr), 0, 0, DateTimeKind.Utc);
                DateTime endDate = startDate.AddHours(imgIDs.Count);

                forecastID = $"{yr}{month}{day}{utcHr}";
                forecastStart = startDate.ToLocalTime();
                forecastEnd = endDate.ToLocalTime();
                timeZone = "MST";
            }
        }

        static readonly Dictionary<string, string> MonthNumberMap = new Dictionary<string, string>()
        {
            {"Jan", "01" },
            {"Feb", "02" },
            {"Mar", "03" },
            {"Apr", "04" },
            {"May", "05" },
            {"Jun", "06" },
            {"Jul", "07" },
            {"Aug", "08" },
            {"Sep", "09" },
            {"Oct", "10" },
            {"Nov", "11" },
            {"Dec", "12" },
        };
        static readonly int TABLE_MAX = 18;
        static readonly int SMOKE_TABLE_OFFSET = 4;
        string baseDirectory;

        public HRRR_Scraper(string _baseDirectory)
        {
            baseDirectory = _baseDirectory;
        }

        public void ScrapeHRRRData()
        {
            ChromeDriver Driver = new ChromeDriver();
            List<RegionForecast> forecasts = new List<RegionForecast>() {
                new RegionForecast { region = "NW", imgIDs = new List<string>()},
                new RegionForecast { region = "SW", imgIDs = new List<string>() } };

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Driver.Manage().Window.Maximize();

            string url = "https://rapidrefresh.noaa.gov/hrrr/HRRRsmoke/";
            Driver.Navigate().GoToUrl(url);

            //Select previous forecast because usually the current one is not fully populated until the end of the hour.
            try
            {
                Driver.FindElement(By.CssSelector("#dateDiv > select")).Click();
                Driver.FindElement(By.CssSelector("#dateDiv > select > option:nth-child(2)")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n\n{e.GetType()}");
            }

            string rawDate = Driver.FindElement(By.CssSelector("#content > h2:nth-child(3)")).Text.ToString();

            string smokeMapSelector = "#tableDiv > table > tbody > tr:nth-child(7) > td:nth-child(%)";

            foreach (var forecast in forecasts)
            {
                //open region menu
                Driver.FindElement(By.XPath("/html/body/div/div[2]/div[2]/div[1]/form/div[2]/select")).Click();
                Driver.FindElement(By.XPath($"/html/body/div/div[2]/div[2]/div[1]/form/div[2]/select/option[{(forecast.region == "NW" ? "2" : "5")}]")).Click();

                int i = 0;
                while (i <= TABLE_MAX)
                {
                    try
                    {
                        var smokeEle = Driver.FindElement(By.CssSelector(smokeMapSelector.Replace("%", $"{SMOKE_TABLE_OFFSET + i}")));

                        ////Apparently the images won't be loaded (thus downloads will 404) until these are actually clicked
                        //smokeEle.Click();

                        ////Add some sleeps to avoid spamming server
                        //Thread.Sleep(1500);

                        ////Close the opened tab and switch focus back to the main table tab
                        //Driver.SwitchTo().Window(Driver.WindowHandles[1]);
                        //Driver.Close();
                        //Thread.Sleep(1500);
                        //Driver.SwitchTo().Window(Driver.WindowHandles[0]);

                        string imgID = smokeEle.Text.ToString().Trim();

                        if (imgID.Length > 0)
                            forecast.imgIDs.Add(imgID);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    i++;
                }
            }


            foreach (var forecast in forecasts)
            {
                forecast.ParseAndSetDates(rawDate);
                forecast.forecastDir = $@"{baseDirectory}\{forecast.forecastID}_{forecast.region}";
                //CreateForecastDirectory(forecast);
                //DownloadForecastImages(forecast);
                //RunMp4CreationScript(forecast);
            }

            Driver.Close();
            Driver.Quit();
        }

        private void DownloadForecastImages(RegionForecast forecast)
        {

            string baseURL = "https://rapidrefresh.noaa.gov/hrrr/HRRRsmoke/for_web/hrrr_ncep_jet/<date_forecastID>/<region>/trc1_<region>_sfc_f<imgID>.png";

            using (WebClient client = new WebClient())
            {
                foreach (string imgID in forecast.imgIDs)
                {
                    string imgURL = baseURL.Replace("<date_forecastID>", forecast.forecastID).Replace("<imgID>", imgID).Replace("<region>", forecast.region);
                    client.DownloadFile(new Uri(imgURL), $@"{forecast.forecastDir}\{forecast.forecastID}_{forecast.region}_{imgID}.png");
                    Thread.Sleep(1500);             //Add sleeps to avoid spamming server
                }
            }
        }



        private void CreateForecastDirectory(RegionForecast forecast)
        {
            System.IO.Directory.CreateDirectory(forecast.forecastDir);
        }

        //Trivial to do w/ python, annoying with C#
        private void RunMp4CreationScript(RegionForecast forecast)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\Python310\python.exe";
            string scriptPath = @$"{baseDirectory}\Python_Scripts\mkvid.py", outputFileName = $@"{forecast.forecastDir}\{forecast.forecastID}_{forecast.region}.mp4";
            startInfo.Arguments = @$"{scriptPath} {forecast.forecastDir} {outputFileName}";
            Process.Start(startInfo);
        }

        void PostToTwitter(List<RegionForecast> forecasts)
        {

        }

        static void Main(string[] args)
        {

            HRRR_Scraper s = new HRRR_Scraper(@"c:\users\drslc\NOAA_Twitter_Smoke_Bot");
            s.ScrapeHRRRData();

            Console.Read();
            Environment.Exit(0);
        }
    }
}