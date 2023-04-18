using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UsefulCsharpCommonsUtils.lang.ext;
using WinWeatherTheme.dto;
using WinWeatherTheme.dto.jsonresponse;

namespace WinWeatherTheme.utils
{
    public class ResultWeatherCache
    {
      
        public WeatherJsonResponse LastWeatherJsonResponse { get; private set; }
 

        public WeatherInputParams InputParams { get; set; }
        public DateTime DateLastUpdate { get; private set; }

        public Func<WeatherInputParams, Task<WeatherJsonResponse>> FuncRefreshCache { get; private set; }

        public TimeSpan TsToRefreshInterval { get; private set; }

        public ResultWeatherCache(Func<WeatherInputParams, Task<WeatherJsonResponse>> funcRefresh,
            TimeSpan tsToRefreshInterval, WeatherInputParams inputParams)
        {
            TsToRefreshInterval = tsToRefreshInterval;
            FuncRefreshCache = funcRefresh;
            InputParams = inputParams;

        }

        public async Task<WeatherJsonResponse> GetLastWeatherJsonResponse()
        {
            DateTime dtNow = DateTime.Now;
            if (DateLastUpdate.Add(TsToRefreshInterval).IsBefore(dtNow))
            {
                LastWeatherJsonResponse =  await FuncRefreshCache(InputParams);
                DateLastUpdate = dtNow;
            }
            return LastWeatherJsonResponse;
;
            
        }

      
    }
}
