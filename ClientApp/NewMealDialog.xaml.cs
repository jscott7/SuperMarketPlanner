
using System.Windows;

namespace SuperMarketPlanner
{
    /// <summary>
    /// Interaction logic for NewMealDialog.xaml
    /// </summary>
    public partial class NewMealDialog : Window
    {
        public NewMealDialog()
        {
            InitializeComponent();
        }

        public string NewMeal
        {
            get { return newMeal.Text;  }
        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
