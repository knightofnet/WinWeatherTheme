using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinWeatherTheme.dto
{
    public class WeatherInputParams
    {

        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("long")]
        public float Longitude { get; set; }
        

    }
}
