using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IVPN.ViewModels
{
    // TODO: inherite it from ModelBase (will avoid implementation 'property changed' events here)
    public class ViewModelBase : INotifyPropertyChanged
    {
        public delegate void OnErrorDelegate (string errorText, string errorDescription = "");
        public event OnErrorDelegate OnError = delegate { };
        protected void NotifyError(string errorText, string errorDescription = "")
        {
            OnError (errorText, errorDescription);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event PropertyChangingEventHandler PropertyWillChange = delegate { };

        // TODO: redundant method (on method call can be used 'nameof()')
        protected void RaisePropertyChanged<T>(Expression<Func<T>> exp)
        {
            var propertyName = GetPropertyName(exp);
            RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // TODO: redundant method (on method call can be used 'nameof()')
        protected void RaisePropertyWillChange<T>(Expression<Func<T>> exp)
        {
            var propertyName = GetPropertyName(exp);
            RaisePropertyWillChange(propertyName);
        }

        protected void RaisePropertyWillChange([CallerMemberName] string propertyName = "")
        {
            PropertyWillChange(this, new PropertyChangingEventArgs(propertyName));
        }

        // TODO: redundant method (on method call can be used 'nameof()')
        public string GetPropertyName<T>(Expression<Func<T>> exp)
        {
            return (((MemberExpression)(exp.Body)).Member).Name;
        }
    }
}
