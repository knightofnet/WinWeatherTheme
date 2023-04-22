using NLog.Fluent;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using UsefulCsharpCommonsUtils.file;
using WinWeatherTheme.dto;
using Newtonsoft.Json;
using WinWeatherTheme.dto.jsonresponse;
using Application = System.Windows.Application;

namespace WinWeatherTheme
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static Logger _log = null;

        private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinWeatherTheme");
        private static FileInfo _fileConf;

        public static AppSession Session { get; private set; }
        public static AppConf Conf { get; private set; }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            InitLogger();
            _log.Info("Start program");

            Session = new AppSession() { IsQuickRun = false };
            Conf = null;

            _fileConf = new FileInfo(Path.Combine(AppDataDir, "conf.json"));
            if (_fileConf.Exists)
            {
                string jsonContent = File.ReadAllText(_fileConf.FullName, Encoding.UTF8);
                Conf = JsonConvert.DeserializeObject<AppConf>(jsonContent);
                if (Conf.Proxy == null)
                {
                    Conf.Proxy = new WebProxyAppConf() { ProxyUrl = null };
                    SaveAppConf();
                }
            }
            else
            {

                Conf = new AppConf()
                {
                    IsWithTime = true,
                    HourStart = new TimeSpan(9, 0, 0),
                    HourEnd = new TimeSpan(20, 0, 0),
                    WeatherParams = new WeatherInputParams()
                    {
                        Latitude = 48.866667F,
                        Longitude = 2.333333F,
                        CloudCoverThresholdLight = 70,
                        Model = "meteofrance_seamless",
                        RefreshInterval = 60
                    },
                    Proxy = new WebProxyAppConf()
                    {
                        ProxyUrl = null
                    }

                };

                SaveAppConf();
            }

            try
            {
                MainWindow m = new MainWindow();
                m.ShowDialog();

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Erreur fatale");
                Environment.Exit(1);
            }
        }


        private static void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = Path.Combine(AppDataDir, "log.log") };
            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");


            logfile.ArchiveAboveSize = (long)CommonsFileUtils.HumanReadableSizeToLong("1 Mo");
            //logfile.ArchiveOldFileOnStartup = true;

            //logconsole.Layout = "${message} $";

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            _log = NLog.LogManager.GetCurrentClassLogger();
        }

        public static bool SaveAppConf()
        {
            try
            {
                string serializeObject = JsonConvert.SerializeObject(Conf, Formatting.Indented);
                File.WriteAllText(_fileConf.FullName, serializeObject, Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Erreur lors de la sauvegarde");
                return false;
            }
        }
    }
}
