using System;
using System.Windows;
using System.Windows.Controls;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_Starting.xaml
    /// </summary>
    public partial class wp_Init : Page
    {
        public wp_Init()
        {
            InitializeComponent();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            DataContext = mainWindow.InitViewModel;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (IsVisible && mainWindow != null)
            {
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.ShowInTaskbar = true;
            }
        }
     
    }
}
