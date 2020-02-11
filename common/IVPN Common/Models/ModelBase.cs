using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IVPN.Models
{
    public class ModelBase: INotifyPropertyChanged
    {
        public event PropertyChangingEventHandler PropertyWillChange = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void DoPropertyWillChange([CallerMemberName] string propertyName = "")
        {
            PropertyWillChange(this, new PropertyChangingEventArgs(propertyName));
        }

        protected void DoPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
