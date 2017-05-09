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

        /// <summary>
        /// Parameterless constructor required for serialization
        /// </summary>
        public SelectedIngredient()
        { }

        public SelectedIngredient(string ingredient)
        {
            m_ingredient = ingredient;
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
