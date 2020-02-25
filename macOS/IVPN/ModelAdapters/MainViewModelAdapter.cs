using System.ComponentModel;

using Foundation;
using IVPN.ViewModels;

namespace IVPN
{
    public class MainViewModelAdapter: ObservableObject
    {
        private ConnectionInfoAdapter __ConnectionInfo;

        public MainViewModelAdapter(MainViewModel mainViewModel): base(mainViewModel)
        {
            mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            mainViewModel.PropertyWillChange += MainViewModel_PropertyWillChange;

            if (ViewModel.ConnectionInfo != null)
                ConnectionInfoAdapter = new ConnectionInfoAdapter(ViewModel.ConnectionInfo);
        }

        void MainViewModel_PropertyWillChange(object sender, PropertyChangingEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => MainViewModel_PropertyWillChange(sender, e));
                return;
            }

            WillChangeValue(e.PropertyName);
        }

        void MainViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => MainViewModel_PropertyChanged(sender, e));
                return;
            }

            DidChangeValue(e.PropertyName);

            if (e.PropertyName == ViewModel.GetPropertyName(() => ViewModel.ConnectionInfo)) 
            {
                if (ViewModel.ConnectionInfo != null)
                    ConnectionInfoAdapter = new ConnectionInfoAdapter(ViewModel.ConnectionInfo);                
            }
            else if (e.PropertyName.Equals(nameof(MainViewModel.ConnectionState))
                    || e.PropertyName.Equals(nameof(MainViewModel.PauseStatus))
                    )
            {
                WillChangeValue("isConnected");
                DidChangeValue("isConnected");

                WillChangeValue("isDisconnected");
                DidChangeValue("isDisconnected");
            }

        }
            
        [Export("connectionInfoAdapter")]
        public ConnectionInfoAdapter ConnectionInfoAdapter
        {
            get {
                return __ConnectionInfo;
            }
            set {
                WillChangeValue("connectionInfoAdapter");
                __ConnectionInfo = value;
                DidChangeValue("connectionInfoAdapter");
            }
        }


        [Export("isConnected")]
        public bool IsConnected => ViewModel.ConnectionState == Models.ServiceState.Connected && ViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Resumed;

        [Export("isDisconnected")]
        public bool IsDisconnected => !IsConnected;


        public MainViewModel ViewModel
        {
            get 
            {
                return (MainViewModel)ObservedObject;
            }
        }
    }
}

