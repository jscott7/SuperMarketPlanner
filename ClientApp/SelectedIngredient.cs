using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SuperMarketPlanner
{
    public class SelectedIngredient : INotifyPropertyChanged
    {
        private string m_ingredient;
        private string m_dateToUse;

        public string Ingredient
        {
            get
            {
                return m_ingredient;
            }
            set
            {
                m_ingredient = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Ingredient"));
            }
        }

        public string DateToUse
        {
            get
            {
                return m_dateToUse;      
            }
            set
            {
                m_dateToUse = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DateToUse"));
            }
        }

        /// <summary>
        /// Parameterless constructor required for serialization
        /// </summary>
        public SelectedIngredient()
        { }

        public SelectedIngredient(string ingredient, string dateToUse)
        {
            m_ingredient = ingredient;
            m_dateToUse = dateToUse;
        }

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
