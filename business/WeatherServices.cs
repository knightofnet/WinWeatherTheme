using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog.Fluent;
using WinWeatherTheme.dto.jsonresponse;
using NLog;

namespace WinWeatherTheme.business
{
    internal class WeatherServices
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private const string UrlWeather = "https://api.open-meteo.com/v1/forecast";

        public async Task<WeatherJsonResponse> GetWeather(float latt, float longi)
        {
            HttpClientHandler httpClientHandler = null;
            if (App.Conf.Proxy != null && !string.IsNullOrWhiteSpace(App.Conf.Proxy.ProxyUrl))
            {
                httpClientHandler = AddProxy();
            }


            using (HttpClient client = httpClientHandler != null ? new HttpClient(httpClientHandler, true) : new HttpClient())
            {

                try
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


                    HttpResponseMessage response = await client.GetAsync(builder.Uri);
                    string res = await response.Content.ReadAsStringAsync();

                    WeatherJsonResponse weatherJsonResponse = JsonConvert.DeserializeObject<WeatherJsonResponse>(res);
                    if (weatherJsonResponse == null)
                    {
                        throw new Exception("Erreur lors l'appel HTTP");
                    }

                    weatherJsonResponse.IsCallOk = response.StatusCode == HttpStatusCode.OK;
                    return weatherJsonResponse;
                }
                catch (Exception e)
                {
                    _log.Error(e, "Erreur lors de la récupération des données météo");
                    WeatherJsonResponse weatherJsonResponse = new WeatherJsonResponse() { IsCallOk = false };
                    return weatherJsonResponse;
                }
            }
        }

        private static HttpClientHandler AddProxy()
        {
            WebProxy proxy = new WebProxy
            {
                Address = new Uri(App.Conf.Proxy.ProxyUrl),
                BypassProxyOnLocal = App.Conf.Proxy.BypassProxyOnLocal,
                UseDefaultCredentials = App.Conf.Proxy.UseDefaultCredentials
            };

            // Proxy credentials
            if (!string.IsNullOrWhiteSpace(App.Conf.Proxy.Username))
            {
                NetworkCredential credentials = new NetworkCredential(
                    userName: App.Conf.Proxy.Username,
                    password: App.Conf.Proxy.Password);
                proxy.Credentials = credentials;
            }

            // Create a client handler that uses the proxy
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
            };

            if (App.Conf.Proxy.ProxyUrl.ToLower().StartsWith("https"))
            {
                //httpClientHandler.SslProtocols = 
            }

            // Disable SSL verification
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return httpClientHandler;
        }
    }
}
