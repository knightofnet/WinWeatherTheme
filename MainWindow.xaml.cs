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
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UsefulCsharpCommonsUtils.lang.ext;
using WinWeatherTheme.business;
using WinWeatherTheme.dto;
using WinWeatherTheme.dto.jsonresponse;
using WinWeatherTheme.utils;
using WinWeatherTheme.views;
using MessageBox = System.Windows.MessageBox;
using FormContextMenu = System.Windows.Forms.ContextMenu;
using FormMenuItem = System.Windows.Forms.MenuItem;


namespace WinWeatherTheme
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private NotifyIcon _notifyIcon;
        private DispatcherTimer _mainTimer;

        public bool IsRealClose { get; set; }

        private readonly WeatherServices _weatherServices = new WeatherServices();

        public MainWindow()
        {
            InitializeComponent();

            InitNotifyIcon();

            InitElement();

            App.Session.WeatherResultsCache =
                new ResultWeatherCache(RefreshWeatherRes, TimeSpan.FromMinutes(App.Conf.WeatherParams.RefreshInterval), App.Conf.WeatherParams);
        }

        private async Task<WeatherJsonResponse> RefreshWeatherRes(WeatherInputParams arg)
        {
            Log.Debug("refresh weather");
            WeatherJsonResponse weatherJsonResponse =
                await _weatherServices.GetWeather(App.Conf.WeatherParams.Latitude,
                    App.Conf.WeatherParams.Longitude,
                    App.Conf.WeatherParams.Model);

            if (weatherJsonResponse.IsCallOk)
            {
                return weatherJsonResponse;
            }

            return App.Session.WeatherResultsCache.LastWeatherJsonResponse;
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

            tbLatt.Text = App.Conf.WeatherParams.Latitude.ToString(CultureInfo.InvariantCulture);
            tbLong.Text = App.Conf.WeatherParams.Longitude.ToString(CultureInfo.InvariantCulture);
            tbHourStart.Text = App.Conf.HourStart.ToString("h\\:mm");
            tbHourEnd.Text = App.Conf.HourEnd.ToString("h\\:mm");

            VerifyConf();

            AdaptLblShowOptions();
        }

        private void InitNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            _notifyIcon.Visible = true;
            _notifyIcon.MouseDoubleClick += (sender, args) =>
            {
                ToggleWindow(true);

            };

            FormMenuItem quitMen = new FormMenuItem();
            quitMen.Text = "Quitter";
            quitMen.Click += (sender, args) =>
            {
                IsRealClose = true;
                Close();
            };

            FormMenuItem activeMen = new FormMenuItem();
            activeMen.Text = "Restaurer la fenêtre";
            activeMen.Click += (sender, args) =>
            {
                ToggleWindow(true);
            };

            FormMenuItem forceToggleTheme = new FormMenuItem();
            forceToggleTheme.Text = "Inverser le thême";
            forceToggleTheme.Click += (sender, args) =>
            {
                if (_mainTimer != null && _mainTimer.IsEnabled)
                {
                    _mainTimer.Stop();
                    lblStatus.Content = "Arrêté";
                }

                ThemeWorker.ThemeChoice themeChoice = ToggleTheme(ThemeWorker.ThemeChoice.Undetermined);

                bool isLightTheme = themeChoice == ThemeWorker.ThemeChoice.Light;
                rdThLight.IsChecked = isLightTheme;
                rdThDark.IsChecked = !isLightTheme;

            };

            _notifyIcon.ContextMenu = new FormContextMenu();
            _notifyIcon.ContextMenu.MenuItems.Add(activeMen);
            _notifyIcon.ContextMenu.MenuItems.Add(forceToggleTheme);
            _notifyIcon.ContextMenu.MenuItems.Add(quitMen);
        }

        private void ToggleWindow(bool b)
        {
            if (b)
            {
                Activate();
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
            }
            else
            {
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;
            }
        }

        private void btnApplyCoord_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && _mainTimer != null && _mainTimer.IsEnabled)
            {
                Log.Debug("Arrêt du timer");
                lblStatus.Content = "Arrêté";
                _mainTimer.Stop();
                return;
            }

            if (!VerifyConf())
            {
                return;
            }

            float coordsLatitude = float.Parse(tbLatt.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
            float coordsLongitude = float.Parse(tbLong.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
            bool coordHasChanged = Math.Abs(App.Conf.WeatherParams.Latitude - coordsLatitude) > 0.001 || Math.Abs(App.Conf.WeatherParams.Longitude - coordsLongitude) > 0.001;

            if (coordHasChanged)
            {
                Log.Debug("Coordonnées GPS ont changé => RaZ cache");
                App.Session.WeatherResultsCache.DateLastUpdate = new DateTime();
            }

            App.Conf.WeatherParams.Latitude = coordsLatitude;
            App.Conf.WeatherParams.Longitude = coordsLongitude;
            App.Conf.HourStart = TimeSpan.Parse(tbHourStart.Text);
            App.Conf.HourEnd = TimeSpan.Parse(tbHourEnd.Text);
            App.Conf.IsWithCoord = chkCoord.IsChecked ?? false;
            App.Conf.IsWithTime = chkTime.IsChecked ?? false;


            if (_mainTimer != null)
            {
                _mainTimer.Stop();
                _mainTimer = null;
            }

            _mainTimer = new DispatcherTimer();
            _mainTimer.Interval = new TimeSpan(0, 0, 30);
            _mainTimer.Tick += MainTimerOnTick;

            _mainTimer.Start();
            lblStatus.Content = "Démarré";
            MainTimerOnTick(null, null);

            if (!App.SaveAppConf())
            {
                MessageBox.Show("Erreur : impossible de sauvegarder la configuration.", "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

                bool cloudCoverChoice = cloudCover >= App.Conf.WeatherParams.CloudCoverThresholdLight;
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

            bool isChangeToDo = (isCurrentLightTheme && !isLightTheme || !isCurrentLightTheme && isLightTheme);
            if (App.Conf.IsNoChangeIfFocusAssist)
            {
                bool isFocusAssistEnabled = ThemeWorker.IsFocusAssistEnabled();


                isChangeToDo &= !isFocusAssistEnabled;
            }

            Log.Debug(
                $"currentIsLight: {isCurrentLightTheme}, isNewIsLight: {isLightTheme}, isChangeToDo: {isChangeToDo}");

            if (isChangeToDo)
            {
                Log.Debug("=> Change");

                ThemeWorker.ThemeChoice toggleTheme = ToggleTheme(isLightTheme
                    ? ThemeWorker.ThemeChoice.Light
                    : ThemeWorker.ThemeChoice.Dark);


                rdThLight.IsChecked = isLightTheme;
                rdThDark.IsChecked = !isLightTheme;

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

        private void HandleRadioBtnTheme(bool isForceDark = false)
        {
            if (rdThDark.IsChecked ?? false || isForceDark)
            {
                ToggleTheme(ThemeWorker.ThemeChoice.Dark);
            }
            else
            {
                ToggleTheme(ThemeWorker.ThemeChoice.Light);
            }
        }

        private static ThemeWorker.ThemeChoice ToggleTheme(ThemeWorker.ThemeChoice theme)
        {
            if (theme == ThemeWorker.ThemeChoice.Undetermined)
            {
                theme = ThemeWorker.GetSystemTheme() == ThemeWorker.ThemeChoice.Dark
                    ? ThemeWorker.ThemeChoice.Light
                    : ThemeWorker.ThemeChoice.Dark;
            }

            ThemeWorker.SetAppTheme(theme);
            ThemeWorker.SetSystemTheme(theme);

            return theme;


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
                    $"Le theme clair sera activité si la couverture nuageuse est supérieure à {App.Conf.WeatherParams.CloudCoverThresholdLight}% et entre {timeStart} et {timeEnd}.";
            }
            else if (isWithCoord)
            {
                txtLbl =
                    $"Le theme clair sera activé si la couverture nuagueuse est supérieure à {App.Conf.WeatherParams.CloudCoverThresholdLight}%.";
            }
            else if (isWithTime)
            {
                txtLbl = $"Le theme clair sera activé entre {timeStart} et {timeEnd}.";
            }


            lblShowOptions.Text = txtLbl;
        }

        private bool VerifyConf()
        {
            Log.Debug("VerifyConf - Start");

            AppConf appConf = new AppConf();

            appConf.IsWithCoord = chkCoord.IsChecked ?? false;
            appConf.IsWithTime = chkTime.IsChecked ?? false;

            appConf.WeatherParams = new WeatherInputParams();

            if (appConf.IsWithCoord)
            {
                try
                {
                    appConf.WeatherParams.Latitude =
                        float.Parse(tbLatt.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
                    appConf.WeatherParams.Longitude =
                        float.Parse(tbLong.Text.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    MessageBox.Show("Erreur : la latitude ou la longitude ne sont pas valides.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                if (Math.Abs(appConf.WeatherParams.Latitude) > 90F)
                {
                    MessageBox.Show("Erreur : la latitude doit être comprise entre -90 et 90.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                if (Math.Abs(appConf.WeatherParams.Longitude) > 180F)
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
                    MessageBox.Show("Erreur : l'heure de fin est après l'heure de début.", "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return false;
                }
            }

            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsRealClose || (_mainTimer == null || !_mainTimer.IsEnabled))
            {
                _notifyIcon.Visible = false;
            }
            else
            {
                e.Cancel = true;
                _notifyIcon.Visible = true;

                ToggleWindow(false);
            }
        }

        private void lblRes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.Session.WeatherResultsCache.DateLastUpdate = new DateTime();
            System.Media.SystemSounds.Beep.Play();


        }

        private void btnOptWeather_Click(object sender, RoutedEventArgs e)
        {
            WeatherOptions wOpt = new WeatherOptions();
            wOpt.LoadsWith(App.Conf.WeatherParams);

            if (wOpt.ShowDialog() ?? false)
            {
                wOpt.UpdateObj(App.Conf.WeatherParams);
                App.Session.WeatherResultsCache.DateLastUpdate = new DateTime();

                App.Session.WeatherResultsCache.TsToRefreshInterval = TimeSpan.FromMinutes(App.Conf.WeatherParams.RefreshInterval);

                App.SaveAppConf();
            }
        }
    }
}