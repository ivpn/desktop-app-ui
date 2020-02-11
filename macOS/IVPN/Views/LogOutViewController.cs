using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using IVPN.GuiHelpers;
using IVPN.ViewModels;

namespace IVPN
{
    public partial class LogOutViewController : AppKit.NSViewController
    {
        private static ViewModelLogOut __LogOutViewModel;

        #region Constructors

        // Called when created from unmanaged code
        public LogOutViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public LogOutViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public LogOutViewController() : base("LogOutView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        [Export("LogOutViewModel")]
        public ObservableObject LogOutViewModel { get; private set; }

        //strongly typed view accessor
        public new LogOutView View
        {
            get
            {
                return (LogOutView)base.View;
            }
        }

        public void SetViewModel(ViewModelLogOut viewModel)
        {
            __LogOutViewModel = viewModel;
            LogOutViewModel = new ObservableObject(__LogOutViewModel);
        }

        public async Task Navigated(bool showSessionLimit)
        {
            try
            {
                GuiProgressSpinner.StartAnimation(this);
                await __LogOutViewModel.DoLogOut(showSessionLimit);
            }
            catch (Exception ex)
            {
                IVPNAlert.Show(LocalizedStrings.Instance.LocalizedString("WG_Error_FailedToDeleteKeyOnLogout"), ex.Message, NSAlertStyle.Critical);
            }
            finally
            {
                GuiProgressSpinner.StopAnimation(this);
            }

        }
    }
}
