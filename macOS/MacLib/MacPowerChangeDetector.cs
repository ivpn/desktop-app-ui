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

﻿using System.Runtime.InteropServices;

namespace MacLib
{
	/// <summary>
	/// Power state change detection
	/// (implementation for Mac)
	/// 
	/// We need this class, because 'SystemEvents.PowerModeChanged' event does not work on Mono.
	/// 
	/// Uses native library 'libivpn'
	/// </summary>
	public class MacPowerChangeDetector : System.IDisposable
    {
		public enum PowerStatus
		{
			SystemWillSleep = 0x280,
			SystemWillPowerOn = 0x320,
			SystemHasPoweredOn = 0x300,
		};

		#region DLLIMPORT
		[UnmanagedFunctionPointer (CallingConvention.StdCall)]
		delegate void PowerChangeCallbackDelegate (PowerStatus value);

        private PowerChangeCallbackDelegate _callbackInstance; // // Ensure it doesn't get garbage collected

		[DllImport ("libivpn", EntryPoint = "power_change_initialize_notifications")]
		extern static int PowerChangeInitializeNotifications ([MarshalAs (UnmanagedType.FunctionPtr)] PowerChangeCallbackDelegate callbackPointer);

		[DllImport ("libivpn", EntryPoint = "power_change_uninitialize_notifications")]
		extern static void PowerChangeUnInitializeNotifications ();
		#endregion //DLLIMPORT

		#region Private functionality
		void OnPowerChangeCallback (PowerStatus value)
		{
			OnPowerChangedEvt?.Invoke (value);
		}
		#endregion //Private functionality

		public void Initialize ()
		{
            _callbackInstance = OnPowerChangeCallback;
			PowerChangeInitializeNotifications (_callbackInstance);
		}

		public void UnInitialize ()
		{
			PowerChangeUnInitializeNotifications ();
		}

		public delegate void OnPowerChangedDelegate (PowerStatus powerStatus);
		public event OnPowerChangedDelegate OnPowerChangedEvt;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose (bool disposing)
        {
            if (!disposedValue) 
            {
                if (disposing) 
                {
                    // dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                UnInitialize ();

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
         ~MacPowerChangeDetector() 
        {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

        // This code added to correctly implement the disposable pattern.
        public void Dispose ()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose (true);
            // the finalizer is overridden above.
            System.GC.SuppressFinalize(this);
        }
        #endregion
    }

	/// <summary>
	/// Static wrapper for 'MacPowerChangeDetector'
	/// </summary>
	public class MacPowerChangeDetectorStatic
	{
		#region Private functionality
		static readonly object _locker = new object ();
		static MacPowerChangeDetector _powerDetector;

		static void OnPowerChangeCallback (MacPowerChangeDetector.PowerStatus powerStatus)
		{
			OnPowerChangedEvt?.Invoke (powerStatus);


			switch (powerStatus) 
            {
			case MacLib.MacPowerChangeDetector.PowerStatus.SystemWillSleep:
				PowerModeChanged (_powerDetector, new Microsoft.Win32.PowerModeChangedEventArgs (Microsoft.Win32.PowerModes.Suspend));
				break;

			case MacLib.MacPowerChangeDetector.PowerStatus.SystemHasPoweredOn:
				PowerModeChanged (_powerDetector, new Microsoft.Win32.PowerModeChangedEventArgs (Microsoft.Win32.PowerModes.Resume));
				break;
			}
		}

		// static "destrucror"
		static void OnApplicationExit (object sender, System.EventArgs e)
		{
			UnInitialize ();
		}

		#endregion //Private functionality
		static MacPowerChangeDetectorStatic ()
		{
			System.AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;
		}

		static public void Initialize ()
		{
			lock (_locker) {
				if (_powerDetector != null)
					return;
				_powerDetector = new MacPowerChangeDetector ();
                _powerDetector.Initialize ();
				_powerDetector.OnPowerChangedEvt += OnPowerChangeCallback;
			}
		}

		static public void UnInitialize ()
		{
			lock (_locker) {
				if (_powerDetector != null) {
					_powerDetector.OnPowerChangedEvt -= OnPowerChangeCallback;
					_powerDetector.UnInitialize ();
					_powerDetector = null;
				}
			}
		}

		static public event MacPowerChangeDetector.OnPowerChangedDelegate OnPowerChangedEvt;

        // To have compatibility with native .Net event 
        public static event Microsoft.Win32.PowerModeChangedEventHandler PowerModeChanged;
	}

}
