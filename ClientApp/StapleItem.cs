using System.ComponentModel;
using System.Xml.Serialization;

namespace SuperMarketPlanner
{
    public class StapleItem : INotifyPropertyChanged
    {
        [XmlAttribute(AttributeName = "name")]
        public string Staple { get; set; }

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
            if (rhs != null && rhs.Staple == Staple)
            {
                return true;
            }

            return false;
      
        }

        public override int GetHashCode()
        {
            if (Staple != null)
            {
                return Staple.GetHashCode();
            }

            return 0;
        }
    }
}
