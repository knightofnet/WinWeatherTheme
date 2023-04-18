using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinWeatherTheme.dto
{
    public class AppConf
    {
        [JsonProperty("proxySettings")]
        public WebProxyAppConf Proxy { get; set; }


        [JsonProperty("coords")]
        public WeatherInputParams Coords { get; set; }



        [JsonProperty("cloudCoverThresholdLight")]
        public double CloudCoverThresholdLight { get; set; }

        [JsonProperty("hourStart")]
        public TimeSpan HourStart { get; set; }

        [JsonProperty("hourEnd")]
        public TimeSpan HourEnd { get; set; }

        [JsonProperty("isWithCoord")]
        public bool IsWithCoord { get; set; }

        [JsonProperty("isWithTime")]
        public bool IsWithTime { get; set; }

        [JsonProperty("isNoChangeIfFocusAssist")]
        public bool IsNoChangeIfFocusAssist { get; set; }


    }
}
