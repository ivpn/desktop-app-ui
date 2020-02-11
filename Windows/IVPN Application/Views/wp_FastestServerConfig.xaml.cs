using System;
using System.Windows;
using System.Windows.Controls;
using IVPN.ViewModels;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_FastestServerConfig.xaml
    /// </summary>
    public partial class wp_FastestServerConfig : Page
    {
        public wp_FastestServerConfig()
        {
            InitializeComponent();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            DataContext = mainWindow.FastestServerSettingsViewModel;

            mainWindow.FastestServerSettingsViewModel.OnError += delegate(string text, string description)
            {
                MessageBox.Show(text + Environment.NewLine + description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }

        private void ItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            ViewModelFastestServerSettings.SelectionItem item = btn?.Tag as ViewModelFastestServerSettings.SelectionItem;
            if (item == null)
                return;

            item.IsSelected = !item.IsSelected;
        }
    }
}
