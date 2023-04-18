using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinWeatherTheme.dto.jsonresponse
{
    public class WeatherJsonResponse
    {
        [JsonProperty("latitude")]
        public double Latitude;

        [JsonProperty("longitude")]
        public double Longitude;

        [JsonProperty("generationtime_ms")]
        public double GenerationtimeMs;

        [JsonProperty("utc_offset_seconds")]
        public int UtcOffsetSeconds;

        [JsonProperty("timezone")]
        public string Timezone;

        [JsonProperty("timezone_abbreviation")]
        public string TimezoneAbbreviation;

        [JsonProperty("elevation")]
        public double Elevation;

        [JsonProperty("hourly_units")]
        public HourlyUnits HourlyUnits;

        [JsonProperty("hourly")]
        public Hourly Hourly;

        [JsonProperty("daily_units")]
        public DailyUnits DailyUnits;

        [JsonProperty("daily")]
        public Daily Daily;

        [JsonIgnore]
        public bool IsCallOk { get; set; }
    }

    public class Daily
    {
        [JsonProperty("time")]
        public List<string> Time;

        [JsonProperty("sunrise")]
        public List<string> Sunrise;

        [JsonProperty("sunset")]
        public List<string> Sunset;

        [JsonIgnore]
        public DateTime? SunriseDatetime => Sunrise != null && Sunrise.Any() ? (DateTime?)DateTime.Parse(Sunrise[0]) : null;

        [JsonIgnore]
        public DateTime? SunsetDatetime => Sunset != null && Sunset.Any() ? (DateTime?)DateTime.Parse(Sunset[0]) : null;
    }

    public class DailyUnits
    {
        [JsonProperty("time")]
        public string Time;

        [JsonProperty("sunrise")]
        public string Sunrise;

        [JsonProperty("sunset")]
        public string Sunset;
    }

    public class Hourly
    {
        [JsonProperty("time")]
        public List<string> Time;

        [JsonProperty("temperature_2m")]
        public List<double> Temperature2m;

        [JsonProperty("cloudcover")]
        public List<int> Cloudcover;
    }

    public class HourlyUnits
    {
        [JsonProperty("time")]
        public string Time;

        [JsonProperty("temperature_2m")]
        public string Temperature2m;

        [JsonProperty("cloudcover")]
        public string Cloudcover;
    }

   
}
