using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UsefulCsharpCommonsUtils.ui.linker;
using WinWeatherTheme.dto;

namespace WinWeatherTheme.views
{
    /// <summary>
    /// Logique d'interaction pour WeatherOptions.xaml
    /// </summary>
    public partial class WeatherOptions : Window, IUiLinker<WeatherInputParams>
    {
        private UiLink<WeatherInputParams> _uiLink;

        public bool OkToDoUpdate { get; set; }

        public WeatherOptions()
        {
            InitializeComponent();

           // ,,icon_seamless,gem_seamless,meteofrance_seamless,meteofrance_arome_france
           cbWeatherModels.Items.Add("best_match");
           cbWeatherModels.Items.Add("jma_seamless");
           cbWeatherModels.Items.Add("icon_seamless");
           cbWeatherModels.Items.Add("gem_seamless");
           cbWeatherModels.Items.Add("meteofrance_seamless");
           cbWeatherModels.Items.Add("meteofrance_arome_france");

            LoadsWith(new WeatherInputParams());
        }

     

        public void LoadsWith(WeatherInputParams obj)
        {
            _uiLink = new UiLink<WeatherInputParams>(obj);
            _uiLink.AddBindingTextbox(tbRefreshWeather, nameof(WeatherInputParams.RefreshInterval));
            _uiLink.AddCustumBinding(cbWeatherModels, nameof(WeatherInputParams.Model), (weatherParam, cb) =>
            {
               ((ComboBox)cb).SelectedItem = weatherParam.Model;
               return weatherParam.Model;
            }, (cb, weatherParam) =>
            {
                weatherParam.Model = ((ComboBox)cb).SelectedItem.ToString();
                return weatherParam.Model;
            }  );
            
            _uiLink.DoRead();

            if (App.Session.WeatherResultsCache.LastWeatherJsonResponse != null &&
                App.Session.WeatherResultsCache.LastWeatherJsonResponse.IsCallOk)
            {
                dgLastResults.ItemsSource = App.Session.WeatherResultsCache.LastWeatherJsonResponse.Hourly.Cloudcover;
                dgLastResults.UpdateLayout();
            }
        }

        public WeatherInputParams UpdateObj(WeatherInputParams enviro)
        {
            UiLink<WeatherInputParams>.ResultUpd resultUpd = _uiLink.DoUpdate(enviro);
            return enviro;
        }

        private void btnOkCancel_OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnOkCancel_OnOkClick(object sender, RoutedEventArgs e)
        {
            
            DialogResult = true;
            Close();
        }
    }
}
