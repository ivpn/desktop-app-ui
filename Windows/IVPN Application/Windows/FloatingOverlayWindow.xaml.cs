using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IVPN.Interfaces;
using IVPNCommon.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IVPN.Models;
using IVPN.ViewModels;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for FloatingOverlayWindow.xaml
    /// </summary>
    public partial class FloatingOverlayWindow : Window, INotifyPropertyChanged
    {
        private readonly Window __MainWindow;

        public FloatingOverlayWindowViewModel ViewModel { get; }
        
        public FloatingOverlayWindow(AppState appState, IApplicationServices applicationServices, IService service, Window mainWindow, MainViewModel mainViewModel)
        {
            InitializeComponent();
            SetInitialWindowPosition();
            __MainWindow = mainWindow;

            ViewModel = new FloatingOverlayWindowViewModel(appState, service, applicationServices, mainViewModel);
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            DataContext = this;//__ViewModel;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(ViewModel.Visible)))
                EnsureVisible();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CloseVindow();

            SetInitialWindowPosition();
        }

        private void SetInitialWindowPosition()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = Width;

            // TOP-RIGHT
            Left = screenWidth - windowWidth - 40;
            Top = 40;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            __MainWindow.WindowState = WindowState.Normal;
            __MainWindow.Activate();
        }

        private void EnsureVisible()
        {
            void Act()
            {
                try
                {
                    if (ViewModel.Visible && Visibility != Visibility.Visible)
                    {
                        Visibility = Visibility.Visible;

                        IsVisibleFirewallInfo = !ViewModel.IsPauseNotificationVisible;
                        IsVisiblePauseInfo = ViewModel.IsPauseNotificationVisible;

                        // manually request window to resize to context
                        SizeToContent = SizeToContent.Height;
                        SizeToContent = SizeToContent.WidthAndHeight;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Info("ERROR: " + ex);
                }
            }

            if (Dispatcher.CheckAccess())
                Act();
            else
                Dispatcher.BeginInvoke((Action) Act);
        }

        private void StoryboardAnimationHide_OnCompleted(object sender, EventArgs e)
        {
            CloseVindow();
        }

        private void GuiButtonClosePauseNotifocation_OnClick(object sender, RoutedEventArgs e)
        {
            CloseVindow();
        }

        private void CloseVindow()
        {
            Visibility = Visibility.Collapsed;
        }

        #region PauseButtons
        private void GuiButtonPause_OnClick(object sender, RoutedEventArgs e)
        {
            GuiPauseAddTimeMenuPopup.IsOpen = false;

            Button btn = sender as Button;
            if (btn == null)
                return;

            string strPauseTimeSec = btn.Tag as string;
            if (string.IsNullOrEmpty(strPauseTimeSec))
                strPauseTimeSec = "0";

            if (double.TryParse(strPauseTimeSec, out var pauseSec) == false)
                return;

            if (pauseSec < 1)
            {
                if (TimeIntervalDialog.ShowInputTimeIntervalDialog(out pauseSec, App.Current.MainWindow) == false)
                    return;

                ViewModel.MainViewModel.SetPauseTime(pauseSec);
                return;
            }

            ViewModel.MainViewModel.AddPauseTime(pauseSec);
        }

        private void GuiButtonAddTime_OnClick(object sender, RoutedEventArgs e)
        {
            GuiPauseAddTimeMenuPopup.IsOpen = true;
        }
        #endregion //PauseButtons

        #region Properties
        private bool __IsVisiblePauseInfo;
        public bool IsVisiblePauseInfo
        {
            get => __IsVisiblePauseInfo;
            set
            {
                __IsVisiblePauseInfo = value;
                NotifyOnPropertyChanged();
            }
        }

        private bool __IsVisibleFirewallInfo;
        public bool IsVisibleFirewallInfo
        {
            get => __IsVisibleFirewallInfo;
            set
            {
                __IsVisibleFirewallInfo = value;
                NotifyOnPropertyChanged();
            }
        }
        #endregion //Properties

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //INotifyPropertyChanged
    }
}
