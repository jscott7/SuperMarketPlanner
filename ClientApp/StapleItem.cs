using System.ComponentModel;

namespace SuperMarketPlanner
{
    public class StapleItem : INotifyPropertyChanged
    {
        private string _staple;

        public string Staple
        {
            get
            {
                return _staple;
            }
            set
            {
                _staple = value;
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

        public override bool Equals(object obj)
        {
            var rhs = obj as StapleItem;
            if (rhs != null && rhs.Staple == _staple)
            {
                return true;
            }

            return false;
      
        }

        public override int GetHashCode()
        {
            if (_staple != null)
            {
                return _staple.GetHashCode();
            }

            return 0;
        }
    }
}
