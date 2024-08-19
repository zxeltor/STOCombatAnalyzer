using log4net;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls
{
    /// <summary>
    /// Interaction logic for AboutControl.xaml
    /// </summary>
    public partial class AboutControl : UserControl
    {
        private readonly ILog Log = log4net.LogManager.GetLogger(typeof(AboutControl));

        private CombatLogManager? CombatLogManagerContext => this.DataContext as CombatLogManager;

        private MainWindow? MainWindow => Application.Current.MainWindow as MainWindow;

        public AboutControl()
        {
            InitializeComponent();
        }

        private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is Button button))
                return;

            var url = string.Empty;

            try
            {
                switch (button.Tag)
                {
                    case "GithubRepoUrl":
                        url = Properties.Resources.GithubRepoUrl;
                        UrlHelper.LaunchUrlInDefaultBrowser(url);
                        break;
                    case "GithubRepoWikiUrl":
                        url = Properties.Resources.GithubRepoWikiUrl;
                        UrlHelper.LaunchUrlInDefaultBrowser(url);
                        break;
                    case "GithubMapDetectRepoUrl":
                        url = Properties.Resources.GithubMapDetectRepoUrl;
                        UrlHelper.LaunchUrlInDefaultBrowser(url);
                        break;
                }
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to open default browser for url={url}.";
                Log.Error(errorMessage, exception);
                MessageBox.Show(MainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
