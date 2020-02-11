using System.Windows;
using System.Windows.Media;

namespace AppUpdater.Gui
{
    /// <summary>
    /// Interaction logic for CheckingForUpdateWindow.xaml
    /// </summary>
    internal partial class CheckingForUpdateWindow : Window
    {
        private static CheckingForUpdateWindow _currWindow;
        public static void ShowWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                HideWindow();
                
                _currWindow = new CheckingForUpdateWindow();
                _currWindow.Owner = Application.Current.MainWindow;
                _currWindow.Show();
            });
        }

        public static void HideWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_currWindow != null)
                    _currWindow.DoClose();
                _currWindow = null;
            });
        }

        private bool _isClosed;

        private CheckingForUpdateWindow()
        {
            InitializeComponent();

            if (GuiController.AppIcon != null)
            {
                ImageSource imSource = GuiController.ToImageSource(GuiController.AppIcon);
                Icon = imSource;
            }
        }
        
        private void DoClose()
        {
            _isClosed = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isClosed == false) // if user closed window
                Updater.Cancel();
        }
    }
}
