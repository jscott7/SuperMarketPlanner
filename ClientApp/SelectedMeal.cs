using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SuperMarketPlanner
{
    public class SelectedMeal : INotifyPropertyChanged
    {
        private List<string> _meals = new List<string>();
        private List<string> _ingredients = new List<string>();

        public DateTime DateTime { get; set; }

        public string Date
        {
            get
            {
                return DateTime.ToString("dddd MMM dd");
            }
        }

        public List<string> Meals
        {
            get
            {
                return _meals;
            }
            set
            {
                _meals.AddRange(value);
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
                return string.Join("\r\n", _meals);
            }
        }

        public void addMeal(string meal)
        {
            _meals.Add(meal);
            OnPropertyChanged(new PropertyChangedEventArgs("MealsString"));
        }

        public List<string> Ingredients
        {
            get
            {
                return _ingredients;
            }
            set
            {
                _ingredients.AddRange(value);
            }
        }

        public void Clear()
        {
            _ingredients.Clear();
            _meals.Clear();
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