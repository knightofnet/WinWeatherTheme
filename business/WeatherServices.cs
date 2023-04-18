using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WinWeatherTheme.dto.jsonresponse;

namespace WinWeatherTheme.business
{
    internal class WeatherServices
    {
        private readonly string UrlWeather = "https://api.open-meteo.com/v1/forecast";

        public async Task<WeatherJsonResponse> GetWeather(float latt, float longi)
        {
            using (var client = new HttpClient())
            {
                UriBuilder builder = new UriBuilder(UrlWeather);
                NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);
                query["latitude"] = latt.ToString(CultureInfo.InvariantCulture);
                query["longitude"] = longi.ToString(CultureInfo.InvariantCulture);

                query["hourly"] = "temperature_2m,cloudcover";
                query["daily"] = "sunrise,sunset";
                query["models"] = "meteofrance_seamless";
                query["forecast_days"] = "1";
                query["timezone"] = "Europe/Berlin";

                builder.Query = query.ToString();

    
                
                var response = await client.GetAsync(builder.Uri);
                string res = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<WeatherJsonResponse>(res);

            }
        }
    }
}
