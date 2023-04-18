using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinWeatherTheme.utils;

namespace WinWeatherTheme.dto
{
    public class AppSession
    {

        public bool IsQuickRun { get; set; }

        public ResultWeatherCache WeatherResultsCache { get; set; }

    }
}
