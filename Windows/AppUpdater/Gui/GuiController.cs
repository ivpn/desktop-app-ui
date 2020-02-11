using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppUpdater.Gui
{
    public class GuiController
    {
        internal static Icon AppIcon;
        internal static string CompanyName;
        internal static string AppName;
        internal static string AppVersion;

        internal static void InitializeInternalGui()
        {
            // Get assembly info 
            Assembly assembly = Assembly.GetEntryAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            CompanyName = versionInfo.CompanyName.Trim();
            AppName = assembly.GetName().Name.Trim();
            AppVersion = assembly.GetName().Version.ToString().Trim();
            if (AppIcon==null)
                AppIcon = Icon.ExtractAssociatedIcon(assembly.Location);

            // subscribing on events
            Updater.NewUpdateAvailable += Updater_NewUpdateAvailable;
            Updater.DownloadFinished += Updater_DownloadFinished;

            Updater.AppcastDownloadFinished += Updater_AppcastDownloadFinished;
            Updater.Error += Updater_Error;
            Updater.CheckingForUpdate += Updater_CheckingForUpdate;
            Updater.Downloaded += Updater_Downloaded;
            Updater.NothingToUpdate += Updater_NothingToUpdate;
        }
        
        private static void HideWindows()
        {
            CheckingForUpdateWindow.HideWindow();
            UpdateAvailableDialog.HideWindow();
            DownloadProgressWindow.HideWindow();
        }

        public static void SetApplicationIcon(Icon appIcon)
        {
            AppIcon = appIcon;
        }

        #region Event handlers
        private static void Updater_NewUpdateAvailable(Appcast appcast)
        {
            UpdateAvailableDialog.ShowWindow(appcast);
        }

        internal static void Updater_DownloadFinished(bool isCanceled, Exception ex)
        {
            HideWindows();

            bool isCanStartInstaller = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isCanceled)
                {
                    if (ex is UpdaterExceptionUpdateDownloadTimeout)
                        MessageBox.Show(Application.Current.MainWindow, StringUtils.String("failed_to_download_update_timeout"), StringUtils.String("update_failed"), MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(Application.Current.MainWindow, StringUtils.String("application_update_cancelled"), StringUtils.String("update_cancelled"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (ex != null)
                {
                    MessageBox.Show(Application.Current.MainWindow, StringUtils.String("failed_to_download_update"), StringUtils.String("update_failed"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show(Application.Current.MainWindow, StringUtils.String("update_successfully_downloaded"), StringUtils.String("update_ready"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    isCanStartInstaller = true;
                else
                    isCanStartInstaller = false;
            });

            if (isCanStartInstaller)
                Updater.Continue();
            else
                Updater.Cancel();
        }

        internal static void Updater_Error(Exception ex)
        {
            HideWindows();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ex is UpdaterException && !string.IsNullOrEmpty(ex.Message))
                    MessageBox.Show(Application.Current.MainWindow, ex.Message, StringUtils.String("update_failed"), MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(Application.Current.MainWindow, StringUtils.String("installing_update_error"), StringUtils.String("update_failed"), MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        internal static void Updater_CheckingForUpdate()
        {
            CheckingForUpdateWindow.ShowWindow();
        }

        internal static void Updater_NothingToUpdate()
        {
            HideWindows();
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, StringUtils.String("you_have_latest_version"), StringUtils.String("nothing_to_update"), MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        internal static void Updater_Downloaded(long downloadedBytes, long totalBytes)
        {
            DownloadProgressWindow.Progress(downloadedBytes, totalBytes);
        }

        static void Updater_AppcastDownloadFinished(Appcast appcast)
        {
            HideWindows();
        }
        #endregion //Event handlers

        #region Helper functions
        internal static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
        #endregion //Helper functions
    }
}
