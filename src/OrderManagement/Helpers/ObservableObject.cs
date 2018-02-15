using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace OrderManagement
{
    [Serializable]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpresssion);
            RaisePropertyChanged(propertyName);
        }


        protected void RaisePropertyChanged(String propertyName)
        {
            VerifyPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            VerifyPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        ///     Warns the developer if this Object does not have a public property with
        ///     the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(String propertyName)
        {
            // verify that the property name matches a real,  
            // public, instance property on this Object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail("Invalid property name: " + propertyName);
            }
        }
    }
}