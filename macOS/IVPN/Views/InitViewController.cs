
using System;
using Foundation;

using IVPN.ViewModels;
using MacLib;

namespace IVPN
{
    public partial class InitViewController : AppKit.NSViewController
    {
        private InitViewModel __InitViewModel;

        #region Constructors

        // Called when created from unmanaged code
        public InitViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public InitViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        
        // Call to load from the XIB/NIB file
        public InitViewController() : base("InitView", NSBundle.MainBundle)
        {
            Initialize();
        }
        
        // Shared initialization code
        void Initialize()
        {

        }

        #endregion

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        //strongly typed view accessor
        public new InitView View {
            get {
                return (InitView)base.View;
            }
        }

        public void SetViewModel(InitViewModel viewModel)
        {
            __InitViewModel = viewModel;
            __InitViewModel.PropertyChanged += (sender, e) => {
                DidChangeValue("viewModel_" + e.PropertyName);
            };
            __InitViewModel.PropertyWillChange += (sender, e) => {
                WillChangeValue("viewModel_" + e.PropertyName);
            };
        }

        public async void StartInitialization()
        {
            await __InitViewModel.InitializeAsync();
        }

        [Export("viewModel_ProgressMessage")]
        public NSString ViewModel_ProgressMessage
        {
            get 
            {
                return MacHelpers.ToNSString(__InitViewModel.ProgressMessage);
            }
        }

        [Export("viewModel_IsInProgress")]
        public bool ViewModel_IsInProgress {
            get {
                return __InitViewModel.IsInProgress;
            }
        }

        [Export("viewModel_ServiceError")]
        public NSString ViewModel_ServiceError {
            get 
            {
                if (__InitViewModel.IsFailedToLoadServers)
                    return MacHelpers.ToNSString(__InitViewModel.ServiceError 
                                                 + Environment.NewLine
                                                 + Environment.NewLine+
                                                 "Unable to download servers list.  Please check your internet connection and try again.");

                return MacHelpers.ToNSString(__InitViewModel.ServiceError);
            }
        }

        [Export("viewModel_ServiceErrorCaption")]
        public NSString ViewModel_ServiceErrorCaption {
            get {
                return MacHelpers.ToNSString(__InitViewModel.ServiceErrorCaption);
            }
        }

        [Export("viewModel_IsServiceError")]
        public bool ViewModel_IsServiceError 
        {
            get {
                return __InitViewModel.IsServiceError;
            }
        }

        partial void Retry(NSObject sender)
        {
            if (__InitViewModel.RetryCommand != null)
                __InitViewModel.RetryCommand.Execute(null);
        }

    }
}

