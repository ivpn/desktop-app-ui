using System;
using System.Windows;
using IVPN.ViewModels;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for WireguardSettingsWindow.xaml
    /// </summary>
    public partial class WireguardSettingsWindow : Window
    {
        public ViewModelWireguardSettings ViewModel { get; }

        public WireguardSettingsWindow(ViewModelWireguardSettings viewModel)
        {
            ViewModel = viewModel;
            
            InitializeComponent();
            
            DataContext = this;
        }

        private async void UiButtonRegenerate_OnClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.RegenerateNewKeyAsync();
        }
    }
}
