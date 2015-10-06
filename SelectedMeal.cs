using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SuperMarketPlanner
{
    public class SelectedMeal : INotifyPropertyChanged
    {
        public string Date { get; set; }
        private string m_Meal;

        public string Meal
        {
            get
            {
                return m_Meal;
            }
            set
            {
                m_Meal = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Meal"));
            }
        }

        public List<string> Ingredients { get; set; }
    
        // INotifyPropertyChanged is used to allow the ObservableCollection to be upated when we update a property
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}