using System.ComponentModel;

namespace SuperMarketPlanner
{
    public class SelectedIngredient : INotifyPropertyChanged
    {
        private string _ingredient;
        private string _dateToUse;
        private bool _purchased = false;

        public string Ingredient
        {
            get
            {
                return _ingredient;
            }
            set
            {
                _ingredient = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Ingredient"));
            }
        }

        public string DateToUse
        {
            get
            {
                return _dateToUse;      
            }
            set
            {
                _dateToUse = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DateToUse"));
            }
        }

        public bool IsPurchased
        {
            get
            {
                return _purchased;
            }
            set
            {
                _purchased = value;
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
            if ( otherIngredient.DateToUse == _dateToUse && otherIngredient._ingredient == _ingredient )
            {
                return true;
            }

            return false;

        }

        public override int GetHashCode()
        {
            return _dateToUse.GetHashCode() + _ingredient.GetHashCode();
        }

        /// <summary>
        /// Parameterless constructor required for serialization
        /// </summary>
        public SelectedIngredient()
        { }

        public SelectedIngredient(string ingredient, string dateToUse)
        {
            _ingredient = ingredient;
            _dateToUse = dateToUse;
        }

        public event PropertyChangedEventHandler PropertyChanged;
       
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
