using IVPN.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_SessionLimit.xaml
    /// </summary>
    public partial class wp_SessionLimit : Page
    {
        private ViewModelSessionLimit __SessionLimitModel;
        public wp_SessionLimit()
        {
            InitializeComponent();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            __SessionLimitModel = mainWindow.SessionLimitViewModel;
            DataContext = __SessionLimitModel;
        }

        private void GuiButtonUpgradePlan_OnClick(object sender, RoutedEventArgs e)
        {
            __SessionLimitModel.UpgradeToProPlanCommand.Execute(null);
        }

        private void GuiButtonLogOutAllDevices_OnClick(object sender, RoutedEventArgs e)
        {
            __SessionLimitModel.LogOutAllSessionsCommand.Execute(null);
        }

        private void GuiTryAgain_OnClick(object sender, RoutedEventArgs e)
        {
            __SessionLimitModel.TryAgainCommand.Execute(null);
        }
    }
}
