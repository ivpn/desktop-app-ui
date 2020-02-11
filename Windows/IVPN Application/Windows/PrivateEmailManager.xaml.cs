using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IVPN.Lib;
using IVPN.Models.PrivateEmail;
using IVPN.ViewModels;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for PrivateEmailManager.xaml
    /// </summary>
    public partial class PrivateEmailManager : Window
    {
        #region Static functionality
        private static PrivateEmailManager __Instance;
        public static void Show(PrivateEmailsManagerViewModel model)
        {
            __Instance?.Close();
            __Instance = new PrivateEmailManager(model);
            __Instance.Show();
        }

        public static void CloseAllWindows()
        {
            __Instance?.Close();
        }
        #endregion // Static functionality

        public PrivateEmailsManagerViewModel Model { get; private set; }

        private PrivateEmailManager(PrivateEmailsManagerViewModel model)
        {
            Model = model;
            InitializeComponent();

            DataContext = this;

            Model.OnError += Model_OnError;
            Model.OnWillExecute += ModelOnWillExecute;
            Model.OnDidExecute += ModelOnDidExecute;
            ProgressView.Visibility = Visibility.Collapsed;
        }

        private void ModelOnDidExecute(IOperationStartStopNotifier sender)
        {
            GuiDataGrid.Opacity = 1;
            GuiDataGrid.IsEnabled = true;
            ProgressView.Visibility = Visibility.Collapsed;
            UIProgressBar.IsIndeterminate = false;
        }

        private void ModelOnWillExecute(IOperationStartStopNotifier sender)
        {
            GuiDataGrid.Opacity = 0.4;
            GuiDataGrid.IsEnabled = false;
            ProgressView.Visibility = Visibility.Visible;
            UIProgressBar.IsIndeterminate = true;
        }

        private void Model_OnError(string errorText, string errorDescription = "")
        {
            MessageBox.Show(errorText + Environment.NewLine + errorDescription, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void PrivateEmailManager_OnLoaded(object sender, RoutedEventArgs e)
        {
            // hide 'Minimize' button
            Button minimizeButton = Template.FindName("GuiTemplateButtonMinimizeInTitle", this) as Button;
            if (minimizeButton != null) minimizeButton.Visibility = Visibility.Collapsed;

            Model.ReloadEmailsInfo();
        }

        private void GuiButtonReload_Click(object sender, RoutedEventArgs e)
        {
            Model.ReloadEmailsInfo();
        }

        private void GuiButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            PrivateEmailGenerateWindow.GenerateEmail(Model);
        }

        private void GuiDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GuiDataGrid.SelectedItems.Count <= 0)
                GuiContextMenu.Visibility = Visibility.Collapsed;
            else
                GuiContextMenu.Visibility = Visibility.Visible;

            if (GuiDataGrid.SelectedItems.Count != 1)
            {
                GuiCopyMenuItem.Visibility = Visibility.Collapsed;
                GuiEditMenuItem.Visibility = Visibility.Collapsed;
                GuiDeleteMenuItemHeader.Text = $"Delete {GuiDataGrid.SelectedItems.Count} e-mails";
            }
            else
            {
                GuiCopyMenuItem.Visibility = Visibility.Visible;
                GuiEditMenuItem.Visibility = Visibility.Visible;
                GuiDeleteMenuItemHeader.Text = "Delete e-mail";
            }
        }

        private void GuiDataGrid_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            GuiContextMenu.IsOpen = false;
            if (GuiDataGrid.SelectedItems.Count <= 0)
                return;
            GuiContextMenu.IsOpen = true;
        }

        private void GuiCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GuiContextMenu.IsOpen = false;
            if (GuiDataGrid.SelectedItems.Count != 1)
                return;
            if (!(GuiDataGrid.SelectedItems[0] is PrivateEmailInfo emailInfo))
                return;
            Clipboard.SetText(emailInfo.Email);
        }

        private void GuiEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GuiContextMenu.IsOpen = false;
            if (GuiDataGrid.SelectedItems.Count != 1)
                return;
            if (!(GuiDataGrid.SelectedItems[0] is PrivateEmailInfo emailInfo))
                return;

            PrivateEmailGenerateWindow.EditEmail(Model, emailInfo);
        }

        private void GuiDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GuiEditMenuItem_Click(sender, null);
        }

        private async void GuiDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GuiContextMenu.IsOpen = false;
            if (GuiDataGrid.SelectedItems.Count <= 0)
                return;

            try
            {
                if (GuiDataGrid.SelectedItems.Count == 1)
                {
                    if (!(GuiDataGrid.SelectedItems[0] is PrivateEmailInfo emailInfo))
                        return;

                    if (MessageBox.Show(this,
                            $"Do you really want to delete private e-mail {emailInfo.Email} ?",
                            "Delete e-mail",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.No)
                        return;

                    await Model.DeleteEmail(emailInfo);
                }
                else
                {
                    List< PrivateEmailInfo > emailsToDelete = new List<PrivateEmailInfo>();
                    foreach (PrivateEmailInfo emailInfo in GuiDataGrid.SelectedItems)
                    {
                        if (emailInfo == null)
                            continue;
                        emailsToDelete.Add(emailInfo);
                    }

                    if (MessageBox.Show(this,
                            $"Do you really want to delete {emailsToDelete.Count} private e-mails?",
                            "Delete e-mail",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.No)
                        return;

                    await Model.DeleteEmail(emailsToDelete);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }


    }
}
