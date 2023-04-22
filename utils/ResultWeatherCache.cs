using NLog;
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
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        public WeatherJsonResponse LastWeatherJsonResponse { get; private set; }
 

        public WeatherInputParams InputParams { get; set; }
        public DateTime DateLastUpdate { get; set; }

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
            _log.Debug($"ts: {TsToRefreshInterval}, dtNow: {dtNow}, DtLastUpd: {DateLastUpdate}, dtNowDay: {dtNow.DayOfYear}, DtLstDay: {DateLastUpdate.DayOfYear}, dtAdd: {DateLastUpdate.Add(TsToRefreshInterval)}, {DateLastUpdate.Add(TsToRefreshInterval).IsBefore(dtNow)}");

            if (DateLastUpdate.Add(TsToRefreshInterval).IsBefore(dtNow) || dtNow.DayOfYear != DateLastUpdate.DayOfYear)
            {
                _log.Debug("refresh weather from server");
                LastWeatherJsonResponse = await FuncRefreshCache(InputParams);
                DateLastUpdate = dtNow;
            }
            return LastWeatherJsonResponse;
;
            
        }

      
    }
}
