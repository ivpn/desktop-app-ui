using System;
using System.Windows;
using System.Windows.Controls;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_ServersList.xaml
    /// </summary>
    public partial class wp_ServersList : Page
    {
        public wp_ServersList()
        {
            InitializeComponent();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            DataContext = mainWindow.ServerListViewModel;

            mainWindow.ServerListViewModel.OnError += ServerListViewModelOnError;
        }

        private void ServerListViewModelOnError(string errorText, string errorDescription)
        {
            GuiPopupTitle.Text = errorText;
            GuiPopupText.Text = errorDescription;

            GuiErrorPopup.IsOpen = true;
        }
    }
}
