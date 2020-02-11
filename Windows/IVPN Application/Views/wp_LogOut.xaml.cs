using System;
using System.Windows;
using System.Windows.Controls;
using IVPN.ViewModels;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_LogOut.xaml
    /// </summary>
    public partial class wp_LogOut : Page
    {
        public ViewModelLogOut ViewModel { get; }

        public wp_LogOut()
        {
            InitializeComponent();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            ViewModel = mainWindow.LogOutViewModel;
            DataContext = this;
        }
    }
}
