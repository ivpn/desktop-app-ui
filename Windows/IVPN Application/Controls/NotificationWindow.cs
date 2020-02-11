using IVPN.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace IVPN.Controls
{
    public class NotificationWindow : Window
    {        
        private bool __IsClosing;
        private Window __ParentWindow;
        
        public NotificationWindow()            
        {            
            Loaded += NotificationWindow_Loaded;
            Deactivated += NotificationWindow_Deactivated;
            DataContext = this;

            CloseCommand = new RelayCommand(CloseWindow);
            WindowStyle = System.Windows.WindowStyle.None;
        }

        void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWindowLocation();
        }

        private void CloseWindow()
        {
            if (!__IsClosing)
                Close();
        }

        void NotificationWindow_Deactivated(object sender, EventArgs e)
        {
            if (!__IsClosing)
                Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            __IsClosing = true;
            base.OnClosing(e);            
        }

        public double VerticalOffset { get; set; }

        public double HorizontalOffset { get; set; }

        public Window ParentWindow
        {
            get
            {
                return __ParentWindow;
            }
            set
            {                
                __ParentWindow = value;                
                __ParentWindow.LocationChanged += (sender, e) =>
                                    {
                                        UpdateWindowLocation();
                                    };

                App.Current.Deactivated += (sender, e) =>
                                    {
                                        if (!__IsClosing)
                                            Close();
                                    };
            }
        }

        private void UpdateWindowLocation()
        {
            Top = ParentWindow.Top + VerticalOffset - ActualHeight;
            Left = ParentWindow.Left + HorizontalOffset - ActualWidth / 2;
        }

        public ICommand CloseCommand { get; }
    }
}
