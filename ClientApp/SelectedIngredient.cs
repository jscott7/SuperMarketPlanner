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
        private bool m_purchased = false;

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

        public bool IsPurchased
        {
            get
            {
                return m_purchased;
            }
            set
            {
                m_purchased = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsPurchased"));
            }
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SelectedIngredient otherIngredient = (SelectedIngredient)obj;
            if ( otherIngredient.DateToUse == m_dateToUse && otherIngredient.m_ingredient == m_ingredient )
            {
                return true;
            }

            return false;

        }

        public override int GetHashCode()
        {
            return m_dateToUse.GetHashCode() + m_ingredient.GetHashCode();
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
            PropertyChanged?.Invoke(this, e);

            //Replaces the following code
         //   if (PropertyChanged != null)
         //   {
         //       PropertyChanged(this, e);
         //   }
        }
    }
}
