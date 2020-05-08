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
using IVPN.ViewModels;
using CoreGraphics;
using System.Timers;

namespace IVPN
{
    public partial class ServerSelectionViewController : AppKit.NSViewController
    {
        private MainViewModel __MainViewModel;

        public EventHandler OnEntryServerClick = delegate {}; 
        public EventHandler OnExitServerClick  = delegate {};

        public delegate void OnFrameChangedDelegate (CGRect oldFrame, CGRect newFrame);
        public OnFrameChangedDelegate OnFrameChanged  = delegate {}; 

        private NSPopover __GuiPopoverVPNisConnected;
        private Timer __TimerAutoclosePopover;

        #region Constructors
        // Called when created from unmanaged code
        public ServerSelectionViewController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ServerSelectionViewController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Call to load from the XIB/NIB file
        public ServerSelectionViewController () : base ("ServerSelectionView", NSBundle.MainBundle)
        {
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
        }
        #endregion

        public void SetViewModel (MainViewModel viewModel)
        {
            __MainViewModel = viewModel ?? throw new Exception ("ViewModel is not defined");
            __MainViewModel.PropertyChanged += __MainViewModel_PropertyChanged;
            UpdateGuiData ();
        }

        //strongly typed view accessor
        public new ServerSelectionView View {
            get {
                return (ServerSelectionView)base.View;
            }
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            // stylyze SingleHop button
            InitializeHopButton (GuiBtnSingeHop);
            GuiBtnSingeHop.TitleText = LocalizedStrings.Instance.LocalizedString ("Label_SingleHop");
            SetDisabledHopButton (GuiBtnSingeHop);

            // stylyze MultiHop button
            InitializeHopButton (GuiBtnMultiHop);
            GuiBtnMultiHop.TitleText = LocalizedStrings.Instance.LocalizedString ("Label_MultiHop");
            SetEnabledHopButton (GuiBtnMultiHop);

            GuiEntryServerDescription.AttributedStringValue = CreateServerDescriptionAttributedString (
                LocalizedStrings.Instance.LocalizedString ("Label_EntryServer"));
            GuiExitServerDescription.AttributedStringValue = CreateServerDescriptionAttributedString (
                LocalizedStrings.Instance.LocalizedString ("Label_ExitServer"));

            GuiEntryServerAutomaticText.AttributedStringValue = CreateServerDescriptionAttributedString("fastest server");

            View.OnMouseDown += OnMouseDown;
            View.OnMouseUp += OnMouseUp;
            View.OnMouseMoved += OnMouseMoved;
            View.OnMouseExited += OnMouseExited;

            // stylyze EntryServerView
            UpdateServerButtonColors(GuiEntryServerView);
            // stylyze ExityServerView
            UpdateServerButtonColors(GuiExitServerView);

            // initialize popover
            GuiLabelServerChangePopoverTitle.StringValue = LocalizedStrings.Instance.LocalizedString ("Label_ChangeServerOnVpnPopoverTitle");
            GuiLabelServerChangePopoverText.StringValue = LocalizedStrings.Instance.LocalizedString ("Label_ChangeServerOnVpnPopoverText");

            // Update GUI data according to ModelView
            UpdateGuiData ();

            View.OnApperianceChanged += () =>
            {
                UpdateGuiData();
            };
        }

        #region GUI style functionality
        private static readonly NSColor __ServerDescriptionTextColor = NSColor.FromRgb (152, 165, 179);

        private void InitializeHopButton (CustomButton btn)
        {
            btn.CornerRadius = 0;
            btn.BorderLineWidth = 1f;
            btn.BorderColor = Colors.HopBtnBorderColor;
            btn.TitleFont = UIUtils.GetSystemFontOfSize (14f, NSFontWeight.Semibold);
            SetDisabledHopButton (btn);
        }

        private void UpdateServerButtonColors(NSBox box)
        {
            box.FillColor = Colors.HopBtnColor;
            box.BorderColor = Colors.HopBtnBorderColor;
        }

        private void SetEnabledHopButton (CustomButton btn)
        {
            btn.BackgroundColor = Colors.HopBtnColor;
            btn.TitleForegroundColor = Colors.HopBtnTextEnabledColor;
        }

        private void SetDisabledHopButton (CustomButton btn)
        {
            btn.BackgroundColor = Colors.HopBtnColor;
            btn.TitleForegroundColor = Colors.HopBtnTextDisabledColor;
        }

        /// <summary>
        /// Update GUI data according to ModelView
        /// </summary>
        private void UpdateGuiData ()
        {
            if (__MainViewModel == null)
                return;

            if (!NSThread.IsMain)
            {
                // This code should be executed in main thread 
                InvokeOnMainThread (() => UpdateGuiData());
                return;
            }

            if (ViewLoaded == false)
                return;
                    
            string entryServerText = LocalizedStrings.Instance.LocalizedString ("Label_ConnectTo");
            switch (__MainViewModel.ConnectionState)
            {
            case Models.ServiceState.Connected: 
                    entryServerText = LocalizedStrings.Instance.LocalizedString ("Label_ConnectedTo");
                break;
            case Models.ServiceState.Connecting:
                    entryServerText = LocalizedStrings.Instance.LocalizedString ("Label_TestingOurServers");
                break;
            case Models.ServiceState.Disconnecting:
                    entryServerText = LocalizedStrings.Instance.LocalizedString ("Label_ClosingConnection");
                break;
            case Models.ServiceState.Disconnected:
                if (__MainViewModel.IsMultiHop)
                        entryServerText = LocalizedStrings.Instance.LocalizedString ("Label_EntryServer");
                else
                        entryServerText = LocalizedStrings.Instance.LocalizedString ("Label_ConnectTo");
                break;
            }

            GuiEntryServerDescription.AttributedStringValue = CreateServerDescriptionAttributedString (entryServerText);

            UpdateServerButtonColors(GuiEntryServerView);
            UpdateServerButtonColors(GuiExitServerView);

            GuiBtnSingeHop.BorderColor = Colors.HopBtnBorderColor;
            GuiBtnMultiHop.BorderColor = Colors.HopBtnBorderColor;

            if (__MainViewModel.IsMultiHop) 
            {
                SetDisabledHopButton (GuiBtnSingeHop);
                SetEnabledHopButton (GuiBtnMultiHop);
                GuiExitServerView.Hidden = false;
            } 
            else 
            {
                SetDisabledHopButton (GuiBtnMultiHop);
                SetEnabledHopButton (GuiBtnSingeHop);
                GuiExitServerView.Hidden = true;
            }

            ShowOrHideHopButtons();
            UpdateFrameSize();

            if (__MainViewModel.IsMultiHop == false // automatic server selection is not allowed in multihop
                && __MainViewModel.IsAutomaticServerSelection
                && __MainViewModel.IsFastestServerInUse)
            {
                GuiEntryServerAutomaticText.Hidden = false;
            }
            else
                GuiEntryServerAutomaticText.Hidden = true;

            if (__MainViewModel.IsMultiHop == false // automatic server selection is not allowed in multihop
                && __MainViewModel.IsAutomaticServerSelection
                && (__MainViewModel.ConnectionState == Models.ServiceState.Disconnected 
                     || __MainViewModel.ConnectionState == Models.ServiceState.Connecting)
                && __MainViewModel.IsFastestServerInUse == false
               )
            {
                GuiEntryServerAutomaticText.Hidden = true;

                GuiEntryServerImage.Image = NSImage.ImageNamed("iconAutomaticServerSelection");
                GuiEntryServerName.StringValue = "Fastest server";
                UpdatePingStatusImage(GuiEntryServerPingStatusImage, 0);
                SetXPositionOfPingStatusImage(GuiEntryServerPingStatusImage, GuiEntryServerName);
            }
            else if (__MainViewModel.SelectedServer != null) 
            {
                GuiEntryServerImage.Image = GuiHelpers.CountryCodeToImage.GetCountryFlag(__MainViewModel.SelectedServer.CountryCode);

                GuiEntryServerName.StringValue = __MainViewModel.SelectedServer.Name;

                UpdatePingStatusImage (GuiEntryServerPingStatusImage, __MainViewModel.SelectedServer.PingTimeRelative);
                SetXPositionOfPingStatusImage (GuiEntryServerPingStatusImage, GuiEntryServerName);
            }

            if (__MainViewModel.SelectedExitServer != null) 
            {
                GuiExitServerImage.Image = GuiHelpers.CountryCodeToImage.GetCountryFlag(__MainViewModel.SelectedExitServer.CountryCode);

                GuiExitServerName.StringValue = __MainViewModel.SelectedExitServer.Name;

                UpdatePingStatusImage (GuiExitServerPingStatusImage, __MainViewModel.SelectedExitServer.PingTimeRelative);
                SetXPositionOfPingStatusImage (GuiExitServerPingStatusImage, GuiExitServerName);
            }
        }

        /// <summary>
        /// Create attribytes string with a style: aligment-center, gray foreground color
        /// </summary>
        private static NSAttributedString CreateServerDescriptionAttributedString (string text)
        {
            return new NSAttributedString (text,
                                           new NSStringAttributes {
                                               ForegroundColor = __ServerDescriptionTextColor,
                                               ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Center }
                                           });
        }

        private static void UpdatePingStatusImage (NSImageView image, double pingTimeRelative)
        {
            if (pingTimeRelative <= 0.5)
                image.Image = NSImage.ImageNamed ("iconStatusGood");
            else if (pingTimeRelative <= 0.8)
                image.Image = NSImage.ImageNamed ("iconStatusModerate");
            else
                image.Image = NSImage.ImageNamed ("iconStatusBad");
        }

        /// <summary>
        /// PingStatus image should be located next to ServerName.
        /// Here we are changing X position of an image
        /// </summary>
        private static void SetXPositionOfPingStatusImage(NSImageView statusImage, NSTextField serverNameField)
        {
            CGRect oldFrame = statusImage.Frame;

            nfloat textCenterX = serverNameField.Frame.X + serverNameField.Frame.Width / 2;
            nfloat textEndX = textCenterX + serverNameField.AttributedStringValue.Size.Width / 2;

            statusImage.Frame = new CGRect (textEndX + 10,
                                                         oldFrame.Y, 
                                                         oldFrame.Width, 
                                                         oldFrame.Height);
        }

        public nfloat Height { get; private set; }

        private void UpdateFrameSize()
        {
            CGRect oldFrame = View.Frame;
            nfloat height = GuiBtnMultiHop.Frame.Height + GuiEntryServerView.Frame.Height;
            if (__MainViewModel.IsMultiHop) 
                height += GuiExitServerView.Frame.Height;

            if (GuiBtnMultiHop.Hidden && GuiBtnSingeHop.Hidden)
                height -= GuiBtnMultiHop.Frame.Height;

            if (Height == height)
                return;

            Height = height;

            OnFrameChanged (oldFrame, View.Frame);
        }

        private void ShowOrHideHopButtons()
        {
            if (!__MainViewModel.IsAllowedMultiHop)
            {
                GuiBtnMultiHop.Hidden = true;
                GuiBtnSingeHop.Hidden = true;
                GuiExitServerView.Hidden = true;

                var f = GuiEntryServerView.Frame;
                GuiEntryServerView.Frame = new CGRect(f.X,
                                                      GuiBtnMultiHop.Frame.Y + GuiBtnMultiHop.Frame.Height - f.Height,
                                                      f.Width, 
                                                      f.Height);
            }
            else
            {
                GuiBtnMultiHop.Hidden = false;
                GuiBtnSingeHop.Hidden = false;
                GuiExitServerView.Hidden = false;

                var f = GuiEntryServerView.Frame;
                GuiEntryServerView.Frame = new CGRect(f.X,
                                                      GuiBtnMultiHop.Frame.Y - f.Height + 1,
                                                      f.Width,
                                                      f.Height);
            }
        }

        private void ShowPopoverVPNisConnected ()
        {
            // create and show popover
            if (__GuiPopoverVPNisConnected == null) 
            {
                __GuiPopoverVPNisConnected = new NSPopover ();
                NSViewController popoverController = new NSViewController ();
                popoverController.View = GuiUnbaleToChangeWhenConnectedView;

                __GuiPopoverVPNisConnected.ContentViewController = popoverController;
                __GuiPopoverVPNisConnected.Behavior = NSPopoverBehavior.Transient;
                __GuiPopoverVPNisConnected.Appearance = NSPopoverAppearance.HUD;

                // auto-close after 4 seconds inteval
                __TimerAutoclosePopover = new Timer () { Interval = 4000, AutoReset = false };
                __TimerAutoclosePopover.Elapsed += (theSender, evt) => 
                {
                    InvokeOnMainThread (() => 
                    {
                        try 
                        {
                            if (__GuiPopoverVPNisConnected != null
                                && __GuiPopoverVPNisConnected.Shown)
                                __GuiPopoverVPNisConnected.Close ();
                        } catch { }
                    });
                };
            }
            __TimerAutoclosePopover.Stop ();
            __GuiPopoverVPNisConnected.Show (View.Bounds, View, NSRectEdge.MaxYEdge);
            __TimerAutoclosePopover.Start ();

        }
        #endregion //GUI style functionality

        #region Event handlers
        private bool __IsMouseDownPressed;

        void __MainViewModel_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals (nameof (__MainViewModel.SelectedServer))
                || e.PropertyName.Equals (nameof (__MainViewModel.SelectedExitServer))
                || e.PropertyName.Equals (nameof (__MainViewModel.FastestServer))
                || e.PropertyName.Equals (nameof (__MainViewModel.IsMultiHop))
                || e.PropertyName.Equals (nameof (__MainViewModel.ConnectionState))
                || e.PropertyName.Equals (nameof (__MainViewModel.IsAutomaticServerSelection))
                || e.PropertyName.Equals (nameof (__MainViewModel.IsFastestServerInUse))
                || e.PropertyName.Equals (nameof(__MainViewModel.IsAllowedMultiHop))) 
            {
                UpdateGuiData ();
            }
        }

        partial void OnMultiHopPressed (Foundation.NSObject sender)
        {
            ChangeHopStatus(isMultiHop: true);
        }

        partial void OnSingleHopPressed (Foundation.NSObject sender)
        {
            ChangeHopStatus (isMultiHop: false);
        }

        private void ChangeHopStatus(bool isMultiHop)
        {
            MainWindowController mainWndController = AppDelegate.GetMainWindowController();
            if (mainWndController == null)
                return;
            
            // Not permitted to do any action during view-change animation in progress
            if (mainWndController.IsAnimationInProgress)
                return;
            
            // Not possible to change multihop when connected
            if (__MainViewModel.ConnectionState != Models.ServiceState.Disconnected) 
            {
                ShowPopoverVPNisConnected ();
                return;
            }

            // Cteate task to process the reaction
            // Ne need this to achichive responsive interface 
            // (button should be redrawed as 'unpressed' immediately after it was released)
            System.Threading.Tasks.Task.Run (() => 
            {
                __MainViewModel.IsMultiHop = isMultiHop;
            }); 
        }

        private void OnMouseDown (NSEvent theEvent)
        {
            GuiEntryServerView.FillColor = Colors.HopBtnColor;
            GuiExitServerView.FillColor = Colors.HopBtnColor;

            if (GuiEntryServerView.Frame.Contains (theEvent.LocationInWindow.X, theEvent.LocationInWindow.Y)) 
            {
                __IsMouseDownPressed = true;
                GuiEntryServerView.FillColor = Colors.HopBtnPressedColor;
            }

            if (GuiExitServerView.Frame.Contains (theEvent.LocationInWindow.X, theEvent.LocationInWindow.Y)) 
            {
                __IsMouseDownPressed = true;
                GuiExitServerView.FillColor = Colors.HopBtnPressedColor;
            }
        } 

        private void OnMouseUp (NSEvent theEvent)
        {
            if (!__IsMouseDownPressed)
                return;

            ReleaseMouseDownStatus ();

            MainWindowController mainWndController = AppDelegate.GetMainWindowController();
            if (mainWndController == null)
                return;
            
            // Not permitted to do any action during view-change animation in progress
            if (mainWndController.IsAnimationInProgress)
                return;
         
            if (GuiEntryServerView.Frame.Contains (theEvent.LocationInWindow.X, theEvent.LocationInWindow.Y))
                OnEntryServerClick (this, null);

            if (GuiExitServerView.Frame.Contains (theEvent.LocationInWindow.X, theEvent.LocationInWindow.Y))
                OnExitServerClick (this, null);
        }

        void OnMouseMoved (NSEvent theEvent)
        {
            if (__IsMouseDownPressed) 
            {
                if (!GuiEntryServerView.Frame.Contains (theEvent.LocationInWindow.X, theEvent.LocationInWindow.Y)
                    || !GuiExitServerView.Frame.Contains (theEvent.LocationInWindow.X, theEvent.LocationInWindow.Y)) 
                {
                    ReleaseMouseDownStatus ();
                }
            }
        }

        private void OnMouseExited (NSEvent theEvent)
        {
            ReleaseMouseDownStatus ();
        }
        #endregion //Event handlers

        private void ReleaseMouseDownStatus()
        {
            __IsMouseDownPressed = false;
            GuiEntryServerView.FillColor = Colors.HopBtnColor;
            GuiExitServerView.FillColor = Colors.HopBtnColor;
        }
    }
}
