using AppKit;
using Foundation;
using IVPN.ViewModels;

namespace IVPN
{
    public class ProofsViewModelAdapter : ObservableObject
    {
        private ProofsViewModel __ProofsViewModel;
        public ProofsViewModelAdapter(ProofsViewModel proofsViewModel) : base(proofsViewModel)
        {
            __ProofsViewModel = proofsViewModel;
            __ProofsViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals(nameof(ProofsViewModel.GeoLookup)))
                {
                    if (__ProofsViewModel.GeoLookup == null)
                        GeoLookup = null;
                    else
                        GeoLookup = new ObservableObject(__ProofsViewModel.GeoLookup);
                }
                else if (e.PropertyName.Equals(nameof(ProofsViewModel.State)))
                {
                    WillChangeValue("IsProgress");
                    DidChangeValue("IsProgress");
                }
            };


        }

        [Export("geoLookup")]
        public ObservableObject GeoLookup 
        {
            get => __GeoLookup;
            private set
            {
                WillChangeValue("CountryImage");
                WillChangeValue("isSecured");
                WillChangeValue("isNotSecured");
                WillChangeValue("geoLookup");
                __GeoLookup = value;
                DidChangeValue("geoLookup");
                DidChangeValue("isNotSecured");
                DidChangeValue("isSecured");
                DidChangeValue("CountryImage");
            } 
        }
        private ObservableObject __GeoLookup;

        [Export("isSecured")]
        public bool IsSecured
        {
            get
            {
                var geoInfo = __ProofsViewModel.GeoLookup;
                if (geoInfo == null || geoInfo.IsIvpnServer == false)
                    return false;
                return true;
            }
        }
        [Export("isNotSecured")]
        public bool IsNotSecured => !IsSecured;

        [Export("CountryImage")]
        public NSImage CountryImage
        {
            get
            {
                var geoInfo = __ProofsViewModel.GeoLookup;
                if (geoInfo != null)
                    return GuiHelpers.CountryCodeToImage.GetCountryFlagFromAllCountriesSet(geoInfo.CountryCode);
                return null;
            }
        }

        [Export("IsProgress")]
        public bool IsProgress
        {
            get
            {
                return __ProofsViewModel.State == ProofsViewModel.StateEnum.Updating;
            }
        }
    }
}
