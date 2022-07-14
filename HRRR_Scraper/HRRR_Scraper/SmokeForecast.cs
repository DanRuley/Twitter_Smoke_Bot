using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HRRR_Scraper
{
    class SmokeForecast
    {
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

        public string region { get; set; }
        public DateTime forecastStart { get; private set; }
        public DateTime forecastEnd { get; private set; }
        public string forecastID { get; private set; }
        public string timeZone { get; private set; }
        public string forecastDir { get; set; }
        public List<string> imgURLs { get; set; }

        public void ParseAndSetDates(string rawDate)
        {
            Regex rgx = new Regex("([\\d]+)\\s*([A-Za-z]+)\\s*([\\d]{4})\\s-\\s([\\d]+)");
            GroupCollection m = rgx.Matches(rawDate)[0].Groups;

            string day = m[1].Value, month = MonthNumberMap[m[2].Value], yr = m[3].Value, utcHr = m[4].Value;

            DateTime startDate = new DateTime(int.Parse(yr), int.Parse(month), int.Parse(day), int.Parse(utcHr), 0, 0, DateTimeKind.Utc);
            DateTime endDate = startDate.AddHours(imgURLs.Count);

            forecastID = $"{yr}{month}{day}{utcHr}";
            forecastStart = startDate.ToLocalTime();
            forecastEnd = endDate.ToLocalTime();
            timeZone = "MST";
        }
    }
}
