using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinWeatherTheme.dto
{
    public class WebProxyAppConf
    {
        [JsonProperty("proxyUrl")]
        public string ProxyUrl { get; set; }

        [JsonProperty("proxyAsSecondAttempt")]
        public bool ProxyAsSecondAttempt { get; set; }

        [JsonProperty("bypassProxyOnLocal")]
        public bool BypassProxyOnLocal { get; set; }

        [JsonProperty("useDefaultCredentials")]
        public bool UseDefaultCredentials { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }



    }
}
