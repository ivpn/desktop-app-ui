//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
using System.Timers;

using AppKit;
using CoreGraphics;
using Foundation;
using IVPN.GuiHelpers;
using IVPN.Models;
using IVPN.ViewModels;

namespace IVPN
{
    public partial class MainPageViewController : AppKit.NSViewController
    {
        private ServerSelectionViewController __ServerSelectionController;
        private ConnectButtonViewController __ConnectButtonViewController;

        private NSWindow __Window;

        private MainViewModel __MainViewModel;
        #region Constructors

        // Called when created from unmanaged code
        public MainPageViewController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public MainPageViewController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Call to load from the XIB/NIB file
        public MainPageViewController () : base ("MainPageView", NSBundle.MainBundle)
        {
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion

        public void SetViewModel (MainViewModel viewModel, NSWindow window)
        {
            __Window = window;

            __MainViewModel = viewModel;
            __MainViewModel.PropertyChanged += __MainViewModel_PropertyChanged;
            __MainViewModel.OnError += __MainViewModel_OnError;

        }

        //strongly typed view accessor
        public new MainPageView View {
            get {
                return (MainPageView)base.View;
            }
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            //GuiConnectButtonView
            __ConnectButtonViewController = new ConnectButtonViewController ();
            __ConnectButtonViewController.SetViewModel (__MainViewModel);
            InitializeSubView (__ConnectButtonViewController, GuiConnectButtonView);

            //Initialize servers view Single\MultiHop
            __ServerSelectionController = new ServerSelectionViewController ();
            __ServerSelectionController.SetViewModel (__MainViewModel);
            __ServerSelectionController.OnEntryServerClick += __ServerSelectionController_OnEntryServerClick;
            __ServerSelectionController.OnExitServerClick += __ServerSelectionController_OnExitServerClick;
            __ServerSelectionController.OnFrameChanged += __ServerSelectionController_OnFrameChanged;
            InitializeSubView (__ServerSelectionController, GuiServerSelectionView);

            UpdateWindowSize ();

            GuiFirewallSwitchControl.ValueChanged += GuiFirewallSwitchControl_ValueChanged;

            // AntiTracker switcher
            GuiAntiTrackerSwitchControl.ValueChanged += (object sender, EventArgs e) => { __MainViewModel.IsAntiTrackerEnabled = GuiAntiTrackerSwitchControl.Value; };
            Action updateAntiTrackerSwitcherColor = () => 
            {
                if (__MainViewModel.Settings.IsAntiTrackerHardcore)
                    GuiAntiTrackerSwitchControl.SwitchOnBackgroundColor = NSColor.FromRgb(255, 0, 57);      // red
                else
                    GuiAntiTrackerSwitchControl.SwitchOnBackgroundColor = NSColor.FromRgb(61, 161, 235);    // blue

                if (!NSThread.IsMain)
                    InvokeOnMainThread(() => { GuiAntiTrackerSwitchControl.NeedsDisplay = true; });
                else
                    GuiAntiTrackerSwitchControl.NeedsDisplay = true;
            };

            updateAntiTrackerSwitcherColor();
            __MainViewModel.Settings.PropertyChanged += (sender, e) =>
            {
                if (nameof(__MainViewModel.Settings.IsAntiTrackerHardcore).Equals(e.PropertyName))
                    updateAntiTrackerSwitcherColor();
            };

            // Update UI
            UpdateUI();
        }

        private void InitializeSubView (NSViewController controller, PageView ownerView)
        {
            controller.View.Frame = ownerView.Frame;
            View.AddSubview (controller.View);
        }

        void __MainViewModel_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => __MainViewModel_PropertyChanged(sender, e));
                return;
            }

            switch (e.PropertyName) 
            {
                case nameof (__MainViewModel.ConnectionError):
                    if (!__MainViewModel.AppState.IsLoggedIn())
                        break; // not necessary to show connection error when logged-out

                    if (!string.IsNullOrEmpty (__MainViewModel.ConnectionError))
                        IVPNAlert.Show (LocalizedStrings.Instance.LocalizedString("Error_ConnectionError"), 
                                    __MainViewModel.ConnectionError, NSAlertStyle.Warning);
                    break;

                case nameof(__MainViewModel.IsKillSwitchEnabled):
                case nameof(__MainViewModel.KillSwitchIsPersistent):
                case nameof(__MainViewModel.PauseStatus):
                case nameof(__MainViewModel.ConnectionState):
                case nameof(__MainViewModel.IsAntiTrackerEnabled):
                case nameof(__MainViewModel.IsIsAntiTrackerChangingStatus):
                    UpdateUI();
                    break;
            }
        }

        private void UpdateUI()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => UpdateUI());
                return;
            }

            GuiFirewallSwitchControl.Value = __MainViewModel.IsKillSwitchEnabled;
            GuiFirewallSwitchControl.Enabled = !__MainViewModel.KillSwitchIsPersistent;

            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Resumed)
            {
                if (__MainViewModel.KillSwitchIsPersistent == false)
                    GuiFirewallSwitchControl.Enabled = true;
            }
            else
                GuiFirewallSwitchControl.Enabled = false;

            if (__MainViewModel.IsAntiTrackerEnabled && __MainViewModel.ConnectionState == ServiceState.Disconnected)
                GuiAntiTrackerSwitchControl.AlphaValue = (nfloat)0.4;
            else
                GuiAntiTrackerSwitchControl.AlphaValue = 1;

            GuiAntiTrackerSwitchControl.Value = __MainViewModel.IsAntiTrackerEnabled;
            GuiAntiTrackerSwitchControl.Enabled = !__MainViewModel.IsIsAntiTrackerChangingStatus;
        }

        #region Buttons panel 
        partial void OnInfoButtonAntitracker(Foundation.NSObject sender)
        {
            NSViewController popoverController = new NSViewController();

            if (__MainViewModel.IsAntiTrackerEnabled && __MainViewModel.ConnectionState != ServiceState.Connected)
                popoverController.View = GuiAntitrackerWhenConnectedInfo;
            else
                popoverController.View = GuiAntitrackerInfoView;

            NSPopover popover = new NSPopover();
            popover.ContentViewController = popoverController;
            popover.Behavior = NSPopoverBehavior.Transient;
            popover.Appearance = NSPopoverAppearance.HUD;
            popover.Show(GuiAntiTrackerSwitchControl.Bounds, GuiAntiTrackerSwitchControl, NSRectEdge.MinYEdge);
        }

        partial void OnInfoButtonFirewall(Foundation.NSObject sender)
        {
            NSViewController popoverController = new NSViewController();
            popoverController.View = GuiFirewallInfoView;

            NSPopover popover = new NSPopover();
            popover.ContentViewController = popoverController;
            popover.Behavior = NSPopoverBehavior.Transient;
            popover.Appearance = NSPopoverAppearance.HUD;
            popover.Show(GuiFirewallSwitchControl.Bounds, GuiFirewallSwitchControl, NSRectEdge.MinYEdge);
        }

        private NSPopover __GuiPopoverFirewallIsOff;
        private Timer __TimerAutoClosePopoverFirewall;
        void GuiFirewallSwitchControl_ValueChanged(object sender, EventArgs e)
        {
            if (__MainViewModel != null)
            {
                if (__MainViewModel.IsKillSwitchEnabled != GuiFirewallSwitchControl.Value)
                    __MainViewModel.IsKillSwitchEnabled = GuiFirewallSwitchControl.Value;
            }

            // If VPN is ON but Firewall is off - show notification:
            // e.g."We recommend you to keep it always turned on when you are connected to VPN."
            CustomSwitchControl switchCtrl = sender as CustomSwitchControl;
            if (__MainViewModel.IsKillSwitchEnabled == false
                && __MainViewModel.ConnectionState != ServiceState.Disconnected
                && switchCtrl != null)
            {
                if (__GuiPopoverFirewallIsOff == null)
                {
                    // create and show popover
                    __GuiPopoverFirewallIsOff = new NSPopover();
                    NSViewController popoverController = new NSViewController();
                    popoverController.View = GuiFirewallOffInfoView;

                    __GuiPopoverFirewallIsOff.ContentViewController = popoverController;
                    __GuiPopoverFirewallIsOff.Behavior = NSPopoverBehavior.Transient;
                    __GuiPopoverFirewallIsOff.Appearance = NSPopoverAppearance.HUD;

                    // auto-close after 6 seconds inteval
                    __TimerAutoClosePopoverFirewall = new Timer() { Interval = 6000, AutoReset = false };
                    __TimerAutoClosePopoverFirewall.Elapsed += (theSender, evt) =>
                    {
                        InvokeOnMainThread(() =>
                        {
                            try
                            {
                                if (__GuiPopoverFirewallIsOff != null && __GuiPopoverFirewallIsOff.Shown)
                                    __GuiPopoverFirewallIsOff.Close();
                            }
                            catch { }
                        });
                    };
                }

                __TimerAutoClosePopoverFirewall.Stop();
                __GuiPopoverFirewallIsOff.Show(switchCtrl.Bounds, switchCtrl, NSRectEdge.MinYEdge);
                __TimerAutoClosePopoverFirewall.Start();
            }
            else
            {
                if (__GuiPopoverFirewallIsOff != null)
                    __GuiPopoverFirewallIsOff.Close();
            }
        }
        #endregion //Buttons panel

        void __MainViewModel_OnError (string errorText, string errorDescription = "")
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => __MainViewModel_OnError(errorText, errorDescription));
                return;
            }

            if (string.IsNullOrEmpty (errorDescription))
                IVPNAlert.Show (errorText);
            else
                IVPNAlert.Show (errorText, errorDescription);
        }

        /// <summary>
        /// Get Frame of connect button
        /// Required, for example, by user introduction stage (to highlight it)
        /// </summary>
        public CGRect GetConnectButtonViewRect ()
        {
            CGRect connectBtnRect = __ConnectButtonViewController.GetConnectButtonRect ();
            connectBtnRect = new CGRect (connectBtnRect.X,
                                         connectBtnRect.Y + __ServerSelectionController.Height - 5,
                                         connectBtnRect.Width,
                                         connectBtnRect.Height);

            return connectBtnRect;
        }

        /// <summary>
        /// Get Frame of ServersSelection View
        /// Required, for example, by user introduction stage (to highlight it)
        /// </summary>
        public CGRect GetServerSelectionViewRect ()
        {
            CGRect rect = GuiServerSelectionView.Frame;
            rect = new CGRect (rect.X,
                               rect.Y,
                               rect.Width,
                               __ServerSelectionController.Height);

            return rect;
        }

        /// <summary>
        /// Get Frame of Firewall switcher View
        /// Required, for example, by user introduction stage (to highlight it)
        /// </summary>
        public CGRect GetFirewallControlViewRect()
        {
            CGRect labelRect = GuiFirewallLabel.Frame;
            CGRect switcherRect = GuiFirewallSwitchControl.Frame;

            CGRect rect = new CGRect(labelRect.X - 3,
                               labelRect.Y + GuiButtonsPanel.Frame.Y - 3,
                               switcherRect.X + switcherRect.Width - labelRect.X + 6,
                               labelRect.Height + 6);

            return rect;
        }

        public void ShowPauseTimeDialog()
        {
            __ConnectButtonViewController.ShowPauseTimeDialog();
        }

        #region Servers
        void __ServerSelectionController_OnEntryServerClick (object sender, EventArgs e)
        {
            if (__MainViewModel.IsMultiHop)
                __MainViewModel.SelectEntryServerCommand.Execute (null);
            else
                __MainViewModel.SelectServerCommand.Execute (null);
        }

        void __ServerSelectionController_OnExitServerClick (object sender, EventArgs e)
        {
            __MainViewModel.SelectExitServerCommand.Execute (null);
        }

        void __ServerSelectionController_OnFrameChanged (CoreGraphics.CGRect oldF, CoreGraphics.CGRect newF)
        {
            UpdateWindowSize ();
        }

        public void UpdateWindowSize ()
        {
            if (!NSThread.IsMain)
            {            
                InvokeOnMainThread(() => UpdateWindowSize());
                return;
            }

            MainWindowController mainWndController = AppDelegate.GetMainWindowController();
            if (mainWndController == null)
                return;

            // Not permitted to do any action during view-change animation in progress
            if (mainWndController.IsAnimationInProgress)
            {
                // Trying to update window size later
                System.Threading.Tasks.Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(10);
                    UpdateWindowSize();
                });
                return;
            }

            nfloat height = GuiButtonsPanel.Frame.Height
                + __ConnectButtonViewController.View.Frame.Height
                + __ServerSelectionController.Height
                + 32;

            if (__Window.Frame.Height == height)
                return;

            __Window.SetFrame(
                UIUtils.UpdateHeight(
                    __Window.Frame,
                    height
                    ),
                true, true);
        }

        #endregion //Servers
    }
}
