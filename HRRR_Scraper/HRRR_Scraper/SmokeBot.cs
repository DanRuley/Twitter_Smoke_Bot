using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HRRR_Scraper
{
    class SmokeBot
    {
        public static readonly string baseDirectory = @"c:\users\drslc\Twitter_Smoke_Bot";
        public static HashSet<string> completed;

        //Trivial to do w/ python, annoying with C#
        private static void RunMp4CreationScript(List<SmokeForecast> forecasts)
        {
            foreach (SmokeForecast forecast in forecasts)
            {
                string scriptPath = @$"{baseDirectory}\Python_Scripts\mkvid.py";
                string outputFileName = $@"{forecast.forecastDir}\{forecast.forecastID}_{forecast.region}.mp4";

                string[] scriptArgs = new string[] { scriptPath, forecast.forecastDir, outputFileName };
                RunPythonScript(scriptArgs);
            }
        }

        private static void PostToTwitter(List<SmokeForecast> forecasts)
        {
            foreach (SmokeForecast forecast in forecasts)
            {
                string scriptPath = @$"{baseDirectory}\Python_Scripts\upload_vid_post_tweet.py";
                string videoPath = @$"{forecast.forecastDir}\{forecast.forecastID}_{forecast.region}.mp4";
                string tweetTxt = $"\"{forecast.region} Region\nForecast range: {GetPrettyDate(forecast.forecastStart)} thru {GetPrettyDate(forecast.forecastEnd)} (MST)\"";
                string[] scriptArgs = new string[] { scriptPath, videoPath, tweetTxt };
                RunPythonScript(scriptArgs);
            }
        }

        private static string GetPrettyDate(DateTime dts)
        {
            return $"{dts.ToShortDateString()} {dts.ToShortTimeString().Replace(":00 ", "")}";
        }

        private static void RunPythonScript(string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\Python310\python.exe";
            startInfo.Arguments = string.Join(' ', args);
            var proc = Process.Start(startInfo);
            proc.WaitForExit();
        }

        private static HashSet<string> ReadCompletedForecastLog(string path)
        {
            return File.ReadAllLines(path).ToHashSet();
        }

        public static void Main(string[] args)
        {
            try
            {
                completed = ReadCompletedForecastLog($@"{baseDirectory}\run.log");

                ImageScraper scraper = new ImageScraper();
                List<SmokeForecast> forecasts = scraper.ScrapeHRRRData();
                RunMp4CreationScript(forecasts);
                PostToTwitter(forecasts);
            }
            catch (Exception e)
            {
                using (StreamWriter sw = File.AppendText(@"c:\users\drslc\Twitter_Smoke_Bot\error_log.log"))
                {
                    sw.WriteLine($"{DateTime.Now} - C# App Error: {e.Message}");
                    sw.WriteLine(e.StackTrace);
                }
                throw;
            }
        }
    }
}
