using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;
using MeshInspector.UI;

namespace MeshInspector.Utils
{
    /// <summary>
    /// base class for all classes that need inotifypropertychanged
    /// </summary>
    public class BindableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// get the main ui dispatcher to invoke the property changed on this thread if the calling thread is different
        /// </summary>
        private static Dispatcher ms_currentDispatcher
        {
            get { return MainWindow.Instance == null ? Dispatcher.CurrentDispatcher : MainWindow.Instance.Dispatcher; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// threadsafe call of property changed
        /// parameter is a linq expression on a property. that way you dont need to call the property changed with a string
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="propertyExpr"></param>
        protected void OnPropertyChanged <TPropertyType>(Expression <Func <TPropertyType>> propertyExpr)
        {
            string propertyName = Extensions.GetPropertySymbol(propertyExpr);

            if (BindableObject.ms_currentDispatcher.CheckAccess())
                this.OnPropertyChanged(propertyName);
            else
                BindableObject.ms_currentDispatcher.BeginInvoke(new Action <string>(this.OnPropertyChanged), propertyName);
        }

        /// <summary>
        /// call property changed
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler == null)
                return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
