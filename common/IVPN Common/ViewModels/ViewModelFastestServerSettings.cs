using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using IVPN.Interfaces;
using IVPN.Models;

namespace IVPN.ViewModels
{
    public class ViewModelFastestServerSettings : ViewModelBase
    {
        #region Internal properties
        private readonly IApplicationServices __AppServices;
        private readonly IAppNavigationService __NavigationService;
        private readonly IService __Service;
        readonly MainViewModel __MainViewModel;
        #endregion //Internal properties

        public ViewModelFastestServerSettings(IApplicationServices appServices,
                IAppNavigationService navigationService,
                IService service,
                MainViewModel mainViewModel)
        {
            __AppServices = appServices;
            __NavigationService = navigationService;
            __Service = service;
            __MainViewModel = mainViewModel;

            ReturnBackCommand = new RelayCommand(ReturnBack);

            UpdateItems();
            __Service.Servers.PropertyChanged += Servers_PropertyChanged;
            __MainViewModel.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        void Servers_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(__Service.Servers.ServersList))            
                UpdateItems();
        }

        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(__MainViewModel.Settings.ServersFilter))
                UpdateItems();
        }

        private void UpdateItems()
        {
            var vpnType = __MainViewModel.Settings.VpnProtocolType;

            var allItemsToSelect = __Service.Servers.ServersList.Select(x => 
                    new SelectionItem(
                        x.VpnServer, 
                        __MainViewModel.Settings.ServersFilter.IsFastestServerInUse(x.VpnServer.GatewayId, vpnType)
                        )
                ).ToList();

            foreach(SelectionItem item in allItemsToSelect)
            {
                // save settings each selection change
                item.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName.Equals(nameof(SelectionItem.IsSelected)))
                    {
                        if (item.IsSelected)
                            __MainViewModel.Settings.ServersFilter.AddFastestServer(item.ServerInfo.GatewayId, 
                                __MainViewModel.Settings.VpnProtocolType,
                                __Service.Servers.ServersList.Select(x => x.VpnServer.GatewayId)
                                );
                        else
                            __MainViewModel.Settings.ServersFilter.RemoveFastestServer(
                                item.ServerInfo.GatewayId, 
                                __MainViewModel.Settings.VpnProtocolType,
                                __Service.Servers.ServersList.Select(x => x.VpnServer.GatewayId)
                                );
                    }
                };

                // at least one item in a list should be unchecked
                item.WillSelectioChange += (SelectionItem i, bool willChangeTo) =>
                {
                    if (i == null) 
                        return true;
                    if (willChangeTo == false)
                    {
                        var selectedItems = Items.Where(x => x.IsSelected);
                        if (selectedItems.Count() <= 1)
                        {
                            NotifyError(__AppServices.LocalizedString("Message_SelectOneItem", "Please select at least one item.") );
                            return false;
                        }
                    }
                    return true;
                };
            }

            Items = allItemsToSelect;
        }

        public class SelectionItem : ModelBase
        {
            internal delegate bool OnWillSelectionChangeDelegate(SelectionItem item, bool willChangeTo);
            internal event OnWillSelectionChangeDelegate WillSelectioChange = delegate { return true; };

            public SelectionItem(VpnProtocols.VpnServerInfoBase item, bool isSelected)
            {
                ServerInfo = item;
                IsSelected = isSelected;
            }

            public VpnProtocols.VpnServerInfoBase ServerInfo { get; }
            public bool IsSelected 
            { 
                get => __IsSelected;
                set
                {
                    if (__IsSelected == value)
                        return;
                    bool isCanChange = WillSelectioChange(this, value);
                    if (!isCanChange)
                    {
                        DoPropertyChanged(); // to correctly update UI - notify 'property changed' even if it was not changed
                        return;
                    }

                    DoPropertyWillChange();
                    __IsSelected = value;
                    DoPropertyChanged();
                } 
            }
            private bool __IsSelected;
        }

        public List<SelectionItem> Items
        {
            get => __Items;
            set
            {
                if (value == null)
                    return;
                RaisePropertyWillChange();
                __Items = value;
                RaisePropertyChanged();
            }

        }
        private List<SelectionItem> __Items = new List<SelectionItem>();

        #region Properties
        #endregion //Properties

        #region Navigation
        public ICommand ReturnBackCommand { get; }

        private void ReturnBack()
        {
            __NavigationService.NavigateToMainPage(NavigationAnimation.FadeToRight);
        }
        #endregion // Navigation

        #region Private methods
        #endregion //Private methods
    }
}

