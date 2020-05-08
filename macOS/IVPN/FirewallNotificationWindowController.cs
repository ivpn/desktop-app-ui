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

using Foundation;
using AppKit;
using IVPNCommon.ViewModels;
using IVPN.GuiHelpers;
using IVPN.Models;
using IVPN.ViewModels;

namespace IVPN
{
    public partial class FirewallNotificationWindowController : NSWindowController, GuiHelpers.IClickDetectable
    {
        private FloatingOverlayWindowViewModel __viewModel;
        private Interfaces.IApplicationServices __appServices;
        private Interfaces.IService __service;
        private AppState __AppState;
        private MainViewModel __MainViewModel { get; }
        private MainPageViewController __MainPageViewController { get; }

        private System.Timers.Timer _timerHideWnd = new System.Timers.Timer (1000) { Enabled = false, AutoReset = false};

        public event OnClickDelegate OnClick;
        public event OnDoubleClickDelegate OnDoubleClick;

        // Called when created from unmanaged code
        public FirewallNotificationWindowController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
        public FirewallNotificationWindowController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
		// Call to load from the XIB/NIB file
        public FirewallNotificationWindowController (AppState appState
                                                     , Interfaces.IService service
                                                     , Interfaces.IApplicationServices appServices
                                                     , MainViewModel mainViewModel
                                                     , MainPageViewController mainPageViewController) : base ("FirewallNotificationWindow")
        {
            __AppState = appState;
            __service = service;
            __appServices = appServices;
            __MainViewModel = mainViewModel;
            __MainPageViewController = mainPageViewController;
            Initialize ();
        }

        public override void WindowDidLoad ()
        {
			// set initiali window position (top right corner of the screen)
			CoreGraphics.CGRect frame = this.Window.Frame;
			CoreGraphics.CGRect screenFrame = Window.Screen.VisibleFrame;
			Window.SetFrame (
                new CoreGraphics.CGRect (
                    screenFrame.X + screenFrame.Width - frame.Width - 50, 
                    screenFrame.Height - 50, 
                    frame.Width, 
                    frame.Height), 
                false, false);

			// Save\restore window position mechanism
			ShouldCascadeWindows = false;
			Window.FrameAutosaveName = "FirewallNotificationWindowLastPosition";

            base.WindowDidLoad ();
        }

        private readonly NSColor TextStatusColor = NSColor.FromRgb (0xb4, 0xb4, 0xb4);
        private readonly NSColor TextLabelColor = NSColor.FromRgb (0xdf, 0xdf, 0xdf);
        private readonly NSColor WindowBackgroundColor = NSColor.FromRgb (38, 57, 77);

        private void Initialize()
        {
            _timerHideWnd.Elapsed += OnTimerHideWindowEvent;
                         
            this.Window.OnDoubleClick+= () => { OnDoubleClick?.Invoke (); };
            this.Window.OnClick+= () => { OnClick?.Invoke (); };
			
            Window.IsVisible = false;
            Window.AlphaValue = 0.9f;

            // set background colors
            Window.BackgroundColor = WindowBackgroundColor;

            FirewallStatusLabel.TextColor = TextStatusColor;
            VPNStatusLabel.TextColor = TextStatusColor;
            PauseTimeLeftLabel.TextColor = TextStatusColor;

            FirewallLabel.TextColor = TextLabelColor;
            VPNLabel.TextColor = TextLabelColor;
            ResumeInLabel.TextColor = TextLabelColor;

            NSImage resumeBtnImage = NSImage.ImageNamed("iconPlayWhite");
            resumeBtnImage.Size = new CoreGraphics.CGSize(9, 12); 
            ResumeBtn.Image = resumeBtnImage;
            ResumeBtn.TitleTextAttributedString = AttributedString.Create(" " + __appServices.LocalizedString("Button_Resume"), NSColor.White, null, UIUtils.GetSystemFontOfSize(12f, NSFontWeight.Semibold));
            ResumeBtn.BackgroundColor = NSColor.Black;
            ResumeBtn.IconLocation = CustomButton.IconLocationEnum.Right;
            ResumeBtn.CornerRadius = 7;
                     
            // locationg UI elements on right place
            // (some elements are located not on rihght places (in order to easiest view in Xcode designer during developing))
            nfloat offset = FirewallLabel.Frame.Y - ResumeInLabel.Frame.Y;
            CoreGraphics.CGRect oldFrame = Window.Frame;
            Window.SetFrame(new CoreGraphics.CGRect(oldFrame.X,oldFrame.Y+offset,oldFrame.Width,oldFrame.Height-offset), false);

            // update data according to ViewModel (on property changed)
            __viewModel = new FloatingOverlayWindowViewModel (__AppState, __service, __appServices, __MainViewModel);
            __viewModel.PropertyChanged += _viewModel_PropertyChanged;
            __viewModel.Initialize();
            __MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            EnsureUIConsistent();
        }

        partial void OnCloseButton(Foundation.NSObject sender)
        {
            Window.IsVisible = false;
        }

        private bool __lastVisiblityState;
        void _viewModel_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread (() => _viewModel_PropertyChanged (sender, e));
                return;
            }

            VPNStatusLabel.StringValue = (__viewModel.VpnStatus==null)? "" : __viewModel.VpnStatus;
            FirewallStatusLabel.StringValue = (__viewModel.FirewallStatus==null) ? "" : __viewModel.FirewallStatus;

            if (__viewModel.IsBlockingAllTraffic) 
                FirewallStatusLabel.TextColor = NSColor.Green;
            else
                FirewallStatusLabel.TextColor = TextStatusColor;

            if (e.PropertyName.Equals(nameof(__viewModel.Visible)))
            {
                if (__viewModel.Visible)
                {
                    if (Window.IsVisible == false && __lastVisiblityState == false)
                    {
                        _timerHideWnd.Stop();
                        EnsureUIConsistent();
                        Window.IsVisible = true;
                        __lastVisiblityState = true;
                    }
                }
                else
                {
                    if (Window.IsVisible == true)
                    {
                        if (!_timerHideWnd.Enabled)
                            _timerHideWnd.Start();
                    }
                    __lastVisiblityState = false;
                }
            }
        }

        void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals(nameof(__MainViewModel.TimeToResumeLeft)) && !e.PropertyName.Equals(nameof(__MainViewModel.PauseStatus)))
                return;

            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => MainViewModel_PropertyChanged(sender, e));
                return;
            }

            PauseTimeLeftLabel.StringValue = (string.IsNullOrEmpty(__MainViewModel.TimeToResumeLeft)) ? "0:00:00" : __MainViewModel.TimeToResumeLeft;

            EnsureUIConsistent();
        }

        private void EnsureUIConsistent()
        {
            ResumeInLabel.Hidden = !__viewModel.IsPauseNotificationVisible;
            PauseTimeLeftLabel.Hidden = !__viewModel.IsPauseNotificationVisible;

            PauseAddButton.Hidden = __MainViewModel.PauseStatus != MainViewModel.PauseStatusEnum.Paused;
            ResumeBtn.Hidden = __MainViewModel.PauseStatus != MainViewModel.PauseStatusEnum.Paused;

            FirewallStatusLabel.Hidden = __viewModel.IsPauseNotificationVisible;
            FirewallLabel.Hidden = __viewModel.IsPauseNotificationVisible;
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }

		private void OnTimerHideWindowEvent (Object source, System.Timers.ElapsedEventArgs e)
		{
			InvokeOnMainThread (() => 
            {
                if (__viewModel.Visible == false)
                    Window.IsVisible = __viewModel.Visible; 
            });
		}

        public new FirewallNotificationWindow Window {
            get { return (FirewallNotificationWindow)base.Window; }
        }

        NSPopover __PausePopoverMenu;
        partial void OnButtonPauseAddPressed(Foundation.NSObject sender)
        {
            if (__MainViewModel.ConnectionState != ServiceState.Connected)
                return;

            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
            {
                if (__PausePopoverMenu == null)
                {
                    __PausePopoverMenu = new NSPopover();
                    NSViewController pausePopoverMenuController = new NSViewController();
                    pausePopoverMenuController.View = GuiPausePopoverView;

                    __PausePopoverMenu.ContentViewController = pausePopoverMenuController;
                    __PausePopoverMenu.Behavior = NSPopoverBehavior.Transient;
                }

                if (__PausePopoverMenu.Shown)
                    __PausePopoverMenu.Close();
                else
                    __PausePopoverMenu.Show(PauseAddButton.Bounds, PauseAddButton, NSRectEdge.MaxYEdge);// MinYEdge);
            }
        }

        partial void PauseMenuItemButtonPressed(Foundation.NSObject sender)
        {
            if (__PausePopoverMenu != null && __PausePopoverMenu.Shown)
                __PausePopoverMenu.Close();

            NSButton btn = sender as NSButton;
            if (btn == null)
                return;

            double pauseSec = (double)btn.Tag;
            if (pauseSec <= 0)
                __MainPageViewController.ShowPauseTimeDialog();
            else
            {
                if (pauseSec > 0)
                    __MainViewModel.AddPauseTime(pauseSec);
            }
        }

        partial void OnResumeButtonPressed(Foundation.NSObject sender)
        {
            __MainViewModel.ResumeCommand.Execute(null);
        }
    }
}
