using System;
using System.Reflection;
using Foundation;
using IVPN.Models;
using IVPN.ViewModels;

namespace IVPN
{
    public class ObservableObject: NSObject
    {
        public ObservableObject(object obj)
        {
            ObservedObject = obj;

            ViewModelBase viewModelObj = obj as ViewModelBase;
            if (viewModelObj != null)
            {
                viewModelObj.PropertyWillChange+= (object sender, System.ComponentModel.PropertyChangingEventArgs e) => 
                {
                    WillChangeValue(e.PropertyName);
                };

                viewModelObj.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => 
                {
                    DidChangeValue(e.PropertyName);
                };
            }
            else
            {
                ModelBase modelBase = obj as ModelBase;
                if (modelBase != null)
                {
                    modelBase.PropertyWillChange += (object sender, System.ComponentModel.PropertyChangingEventArgs e) =>
                    {
                        WillChangeValue(e.PropertyName);
                    };

                    modelBase.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                    {
                        DidChangeValue(e.PropertyName);
                    };
                }
            }
        }

        private PropertyInfo GetPropertyForName(string propertyName)
        {
            return ObservedObject.GetType().GetProperty(propertyName);
        }

        private NSString AsString(object value)
        {
            if (value == null)
                return new NSString("");

            return new NSString((string)value);
        }

        private int NSObjectToInt(NSObject value)
        {
            if (value is NSString) {
                try
                {
                    return Convert.ToInt32(value.ToString());
                } catch {
                    return 0;
                }
            }

            try
            {
                return (int)(NSNumber)value;
            } catch {
                return 0;
            }
        }

        private string NSObjectToString(NSObject value)
        {
            try{
                return value.ToString();
            } catch {
                return "";
            }
        }

        private bool NSObjectToBool(NSObject value)
        {
            try{
                return (bool)(NSNumber)value;
            } catch {
                return false;
            }
        }

        public override NSObject ValueForKey(NSString key)
        {
            var propertyInfo = GetPropertyForName(key.ToString());

            if (propertyInfo == null)
                return base.ValueForKey(key);    

            object value = propertyInfo.GetValue(ObservedObject);

            if (propertyInfo.PropertyType == typeof(string))
                return AsString(value);

            if (propertyInfo.PropertyType == typeof(bool))
                return new NSNumber((bool)value);

            if (propertyInfo.PropertyType == typeof(int))
                return new NSNumber((int)value);

            if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                return new NSNumber(Convert.ToInt32((Enum)value));

            throw new ArgumentException("Trying to get unsupported property type");
        }

        public override void SetValueForKey(NSObject value, NSString key)
        {
            var propertyName = key.ToString();
            var propertyInfo = GetPropertyForName(propertyName);

            if (propertyInfo == null) {
                base.SetValueForKey(value, key);
                return;
            }

            WillChangeValue(propertyName);

            if (propertyInfo.PropertyType == typeof(string)) 
                propertyInfo.SetValue(ObservedObject, NSObjectToString(value));
            
            else if (propertyInfo.PropertyType == typeof(bool))
                propertyInfo.SetValue(ObservedObject, NSObjectToBool(value));
            else if (propertyInfo.PropertyType == typeof(int)) 
                propertyInfo.SetValue(ObservedObject, NSObjectToInt(value));
            else if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                propertyInfo.SetValue(ObservedObject, Enum.ToObject(propertyInfo.PropertyType, NSObjectToInt(value)));
            else
                throw new ArgumentException("Trying to get unsupported property type");

            DidChangeValue(propertyName);
        }

        public object ObservedObject { get; }

    }
}

