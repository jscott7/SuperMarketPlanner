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
        private List<string> m_Meals = new List<string>();
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

        public List<string> Meals
        {
            get
            {
                return m_Meals;
            }
            set
            {
                m_Meals.AddRange(value);
                // When we update we want to notifiy the UI to update the MealsString binding
                OnPropertyChanged(new PropertyChangedEventArgs("MealsString"));
            }
        }

        /// <summary>
        /// For display, binding to the textblock, concatenate the list with newlines
        /// </summary>
        public string MealsString
        {
            get
            {
                return string.Join("\r\n", m_Meals);
            }
        }

        public void addMeal(string meal)
        {
            m_Meals.Add(meal);
            OnPropertyChanged(new PropertyChangedEventArgs("MealsString"));
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

        public void Clear()
        {
            m_ingredients.Clear();
            m_Meals.Clear();
            OnPropertyChanged(new PropertyChangedEventArgs("MealsString"));
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