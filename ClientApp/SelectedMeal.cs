using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SuperMarketPlanner
{
    public class SelectedMeal : INotifyPropertyChanged
    {
        private DateTime m_Date;
        private string m_Meal;
        private List<string> m_ingredients = new List<string>();

        public DateTime DateTime
        {
            get
            {
                return m_Date;
            }
            set
            {
                m_Date = value;
            }
        }

        public string Date
        {
            get {
                return m_Date.ToString("dddd MMM dd");
            }
        }

        public string Meal
        {
            get
            {
                return m_Meal;
            }
            set
            {
                // Each meal gets added to the date as a new line
                m_Meal += value + "\r\n";
                OnPropertyChanged(new PropertyChangedEventArgs("Meal"));
            }
        }

        public void Clear()
        {
            m_ingredients.Clear();
            m_Meal = "";
            OnPropertyChanged(new PropertyChangedEventArgs("Meal"));
        }

        public List<string> Ingredients
        {
            get
            {
                return m_ingredients;
            }
            set
            {
                m_ingredients.AddRange(value);
            }
        }

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