using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UsefulCsharpCommonsUtils.lang.ext;
using WinWeatherTheme.business;
using WinWeatherTheme.dto;
using WinWeatherTheme.dto.jsonresponse;
using WinWeatherTheme.utils;
using MessageBox = System.Windows.MessageBox;

namespace WinWeatherTheme
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private NotifyIcon notifyIcon;
        private DispatcherTimer mainTimer;

        private WeatherServices weatherServices = new WeatherServices();

        public MainWindow()
        {
            InitializeComponent();

            InitNotifyIcon();

            InitElement();

            App.Session.WeatherResultsCache =
                new ResultWeatherCache(RefreshWeatherRes, TimeSpan.FromHours(1), App.Conf.Coords);

        }

        private async Task<WeatherJsonResponse> RefreshWeatherRes(WeatherInputParams arg)
        {
            Log.Debug("refresh weather");
            return await weatherServices.GetWeather(App.Conf.Coords.Latitude, App.Conf.Coords.Longitude);
        }

        private void InitElement()
        {
            ThemeWorker.ThemeChoice systemTheme = ThemeWorker.GetSystemTheme();
            if (systemTheme != ThemeWorker.ThemeChoice.Undetermined)
            {
                if (systemTheme == ThemeWorker.ThemeChoice.Dark)
                {
                    rdThDark.IsChecked = true;
                }
                else if (systemTheme == ThemeWorker.ThemeChoice.Light)
                {
                    rdThLight.IsChecked = true;
                }
            }

            lblStatus.Content = "Arrêté";

            chkCoord.IsChecked = App.Conf.IsWithCoord;
            chkTime.IsChecked = App.Conf.IsWithTime;

            grCoord.IsEnabled = chkCoord.IsChecked ?? false;
            grTime.IsEnabled = chkTime.IsChecked ?? false;

            tbLatt.Text = App.Conf.Coords.Latitude.ToString();
            tbLong.Text = App.Conf.Coords.Longitude.ToString();
            tbHourStart.Text = App.Conf.HourStart.ToString("h\\:mm");
            tbHourEnd.Text = App.Conf.HourEnd.ToString("h\\:mm");

            VerifyConf();

            AdaptLblShowOptions();
        }

        private void InitNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);

        }

        private void btnApplyCoord_Click(object sender, RoutedEventArgs e)
        {

            if (!VerifyConf())
            {
                return;
            }

            App.Conf.Coords.Latitude = float.Parse(tbLatt.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
            App.Conf.Coords.Longitude = float.Parse(tbLong.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
            App.Conf.HourStart = TimeSpan.Parse(tbHourStart.Text);
            App.Conf.HourEnd = TimeSpan.Parse(tbHourEnd.Text);
            App.Conf.IsWithCoord = chkCoord.IsChecked ?? false;
            App.Conf.IsWithTime = chkTime.IsChecked ?? false;

            if (mainTimer != null)
            {
                mainTimer.Stop();
                mainTimer = null;
            }

            mainTimer = new DispatcherTimer();
            mainTimer.Interval = new TimeSpan(0, 0, 30);
            mainTimer.Tick += MainTimerOnTick;

            mainTimer.Start();
            lblStatus.Content = "Démarré";

            if (!App.SaveAppConf())
            {
                MessageBox.Show("Erreur : impossible de sauvegarder la configuration.", "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }

        private bool VerifyConf()
        {
            Log.Debug("VerifyConf - Start");

            AppConf appConf = new AppConf();

            appConf.IsWithCoord = chkCoord.IsChecked ?? false;
            appConf.IsWithTime = chkTime.IsChecked ?? false;

            appConf.Coords = new WeatherInputParams();

            if (appConf.IsWithCoord)
            {
                try
                {
                    appConf.Coords.Latitude = float.Parse(tbLatt.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
                    appConf.Coords.Longitude = float.Parse(tbLong.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    MessageBox.Show("Erreur : la latitude ou la longitude ne sont pas valides.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                if (Math.Abs(appConf.Coords.Latitude) > 90F)
                {
                    MessageBox.Show("Erreur : la latitude doit être comprise entre -90 et 90.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                if (Math.Abs(appConf.Coords.Longitude) > 180F)
                {
                    MessageBox.Show("Erreur : la longitude doit être comprise entre -180 et 180.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

            }


    

            if (appConf.IsWithTime)
            {

                try
                {
                    appConf.HourStart = TimeSpan.Parse(tbHourStart.Text);
                    appConf.HourEnd = TimeSpan.Parse(tbHourEnd.Text);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    MessageBox.Show("Erreur : l'heure de fin ou l'heure de fin ne sont pas valides.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                if (appConf.HourEnd < appConf.HourStart)
                {
                    MessageBox.Show("Erreur : l'heure de fin est après l'heure de début.", "Erreur", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return false;
                }


            }

            return true;
        }

        private async void MainTimerOnTick(object sender, EventArgs e)
        {
            Log.Debug("MainTimerOnTick - Start");


            bool isLightTheme = true;
            bool isCurrentLightTheme = ThemeWorker.GetSystemTheme() == ThemeWorker.ThemeChoice.Light;

            if (App.Conf.IsWithCoord)
            {
                Log.Debug($"Lat {tbLatt.Text}, Long {tbLong.Text}");


                WeatherJsonResponse wResp = await App.Session.WeatherResultsCache.GetLastWeatherJsonResponse();

                DateTime dt = DateTime.Now;
                dt = dt.ChangeTime(dt.Hour, 00, 00, 00);
                string strDt = dt.ToString("yyyy-MM-ddTHH:mm");

                Log.Debug("Current hour {0}", strDt);

                int ixHour = wResp.Hourly.Time.IndexOf(strDt);
                double temperature = wResp.Hourly.Temperature2m[ixHour];
                double cloudCover = wResp.Hourly.Cloudcover[ixHour];

                lblRes.Content = $"Temperature: {temperature}, cloudCover: {cloudCover}";

                bool cloudCoverChoice = cloudCover >= App.Conf.CloudCoverThresholdLight;
                Log.Debug("Cloud cover choice : {0}", cloudCoverChoice);

                bool isInDay = dt >= wResp.Daily.SunriseDatetime && dt < wResp.Daily.SunsetDatetime;

                isLightTheme &= cloudCoverChoice && isInDay;
            }

            if (App.Conf.IsWithTime)
            {
                TimeSpan ts = DateTime.Now.TimeOfDay;


                bool isHourChoice = ts >= App.Conf.HourStart && ts <= App.Conf.HourEnd;
                Log.Debug("HourChoice : {0}", isHourChoice);

                isLightTheme &= isHourChoice;
            }

            Log.Debug($"currentIsLight: {isCurrentLightTheme}, isNewIsLight: {isLightTheme}");

            if (isCurrentLightTheme && !isLightTheme || !isCurrentLightTheme && isLightTheme)
            {
                Log.Debug("=> Change");

                ThemeWorker.SetAppTheme(isLightTheme
                    ? ThemeWorker.ThemeChoice.Light
                    : ThemeWorker.ThemeChoice.Dark);
                ThemeWorker.SetSystemTheme(isLightTheme
                    ? ThemeWorker.ThemeChoice.Light
                    : ThemeWorker.ThemeChoice.Dark);

            }

            Log.Debug("MainTimerOnTick - End");
        }

        private void rdThDark_Click(object sender, RoutedEventArgs e)
        {
            HandleRadioBtnTheme();
        }

        private void rdThLight_Click(object sender, RoutedEventArgs e)
        {
            HandleRadioBtnTheme();
        }

        private void HandleRadioBtnTheme()
        {
            if (rdThDark.IsChecked ?? false)
            {
                ThemeWorker.SetAppTheme(ThemeWorker.ThemeChoice.Dark);
                ThemeWorker.SetSystemTheme(ThemeWorker.ThemeChoice.Dark);
            }
            else
            {
                ThemeWorker.SetAppTheme(ThemeWorker.ThemeChoice.Light);
                ThemeWorker.SetSystemTheme(ThemeWorker.ThemeChoice.Light);
            }
        }

        private void chkCoord_Click(object sender, RoutedEventArgs e)
        {
            grCoord.IsEnabled = chkCoord.IsChecked ?? false;
            AdaptLblShowOptions();
        }



        private void chkTime_Click(object sender, RoutedEventArgs e)
        {
            grTime.IsEnabled = chkTime.IsChecked ?? false;
            AdaptLblShowOptions();
        }

        private void AdaptLblShowOptions()
        {
            bool isWithCoord = chkCoord.IsChecked ?? false;
            bool isWithTime = chkTime.IsChecked ?? false;

            TimeSpan timeStart = TimeSpan.Parse(tbHourStart.Text);
            TimeSpan timeEnd = TimeSpan.Parse(tbHourEnd.Text);

            string txtLbl = "";
            if (isWithCoord && isWithTime)
            {
                txtLbl =
                    $"Le theme clair sera activité si la couverture nuageuse est supérieure à {App.Conf.CloudCoverThresholdLight}% et entre {timeStart} et {timeEnd}.";
            }
            else if (isWithCoord)
            {
                txtLbl = $"Le theme clair sera activé si la couverture nuagueuse est supérieure à {App.Conf.CloudCoverThresholdLight}%.";
            }
            else if (isWithTime)
            {
                txtLbl = $"Le theme clair sera activé entre {timeStart} et {timeEnd}.";
            }


            lblShowOptions.Content = txtLbl;
        }
    }
}
