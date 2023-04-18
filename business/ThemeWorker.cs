using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinWeatherTheme.business
{
    internal static class ThemeWorker
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        internal enum ThemeChoice
        {
            Undetermined = -1,
            Dark = 0,
            Light = 1,
        }

        public static void SetAppTheme(ThemeChoice theme)
        {

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
            if (key != null)
            {
                key.SetValue("AppsUseLightTheme", (int)theme, RegistryValueKind.DWord);
                key.Close();
            }
        }

        public static void SetSystemTheme(ThemeChoice theme)
        {

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
            if (key != null)
            {
                key.SetValue("SystemUsesLightTheme", (int)theme, RegistryValueKind.DWord);
                key.Close();
            }
        }

        public static ThemeChoice GetSystemTheme()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
            if (key != null)
            {
                object value = key.GetValue("SystemUsesLightTheme");
                if (value is int vInt)
                {
                    return (ThemeChoice)vInt;
                }
                else
                {
                    _log.Warn("SystemUsesLightTheme n'est pas un int");
                }
                key.Close();
            }

            return ThemeChoice.Undetermined;
        }
    }
}
