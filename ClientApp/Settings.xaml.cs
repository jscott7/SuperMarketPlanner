using System.Windows;

namespace SuperMarketPlanner
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            var serverUrl = SettingsUtilities.LoadSetting("serverurl");
   
            if (serverUrl != null)
            {
                serverUrlBox.Text = serverUrl.ToString();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        { 
            SettingsUtilities.SaveSetting("serverurl", serverUrlBox.Text);
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}