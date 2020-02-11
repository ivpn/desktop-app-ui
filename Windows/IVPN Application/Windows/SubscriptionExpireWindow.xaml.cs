using System;
using System.Windows;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for SubscriptionExpireWindow.xaml
    /// </summary>
    public partial class SubscriptionExpireWindow : Window
    {
        #region Static functionality
        private static SubscriptionExpireWindow __Instance;

        public static void Show(Models.Session.SessionStatus sessionInfo, string username)
        {
            if (__Instance==null || __Instance.IsLoaded == false)
                __Instance = new SubscriptionExpireWindow();

            if (string.IsNullOrEmpty(username))
                username = "";
            __Instance.Username = username;

            __Instance.SetSessionInfo(sessionInfo);
            __Instance.Show();
            if (__Instance.WindowState == WindowState.Minimized)
                __Instance.WindowState = WindowState.Normal;
        }

        public new static void Close()
        {
            ((Window) __Instance)?.Close();
        }
        #endregion Static functionality

        public Models.Session.SessionStatus Session { get; private set; }
        
        public string Username { get; private set; }

        private SubscriptionExpireWindow()
        {
            InitializeComponent();
        }
        
        private void SetSessionInfo(Models.Session.SessionStatus accountInfo)
        {
            Session = accountInfo;

            string title;
            string titleDays = "";

            string text;

            string renewButtonText = StringUtils.String("Button_RenewSubscription");
            if (Session.IsOnFreeTrial)
                renewButtonText = StringUtils.String("Button_GetSubscription");

            int progressDaysLeft = 0;

            if (!Session.IsActive)
            {
                title = StringUtils.String("Label_SubscriptionExpired");
                if (Session.IsOnFreeTrial)
                    title = StringUtils.String("Label_FreeTrialExpired");

                text = StringUtils.String("Label_AccountDaysLeftDescription_Expired");
                if (Session.IsOnFreeTrial)
                    text = StringUtils.String("Label_TrialDaysLeftDescription_Expired");
            }
            else
            {
                int daysLeft = (int)(Session.ActiveUtil - DateTime.Now).TotalDays;
                if (daysLeft < 0)
                    daysLeft = 0;

                progressDaysLeft = daysLeft;

                if (daysLeft == 0)
                {
                    title = StringUtils.String("Label_AccountDaysLeftTitle_LastDay");
                    if (Session.IsOnFreeTrial)
                        title = StringUtils.String("Label_TrialDaysLeftTitle_LastDay");

                    text = StringUtils.String("Label_AccountDaysLeftDescription_LastDay");
                    if (Session.IsOnFreeTrial)
                        text = StringUtils.String("Label_TrialDaysLeftDescription_LastDay");

                    titleDays = StringUtils.String("Days_Today");
                }
                else if (daysLeft == 1)
                {
                    title = StringUtils.String("Label_AccountDaysLeftTitle_OneDay");
                    if (Session.IsOnFreeTrial)
                        title = StringUtils.String("Label_TrialDaysLeftTitle_OneDay");

                    text = StringUtils.String("Label_AccountDaysLeftDescription_OneDay");
                    if (Session.IsOnFreeTrial)
                        text = StringUtils.String("Label_TrialDaysLeftDescription_OneDay");

                    titleDays = StringUtils.String("Days_OneDay");
                }
                else
                {
                    title = StringUtils.String("Label_AccountDaysLeftTitle_PARAMETRIZED");
                    if (Session.IsOnFreeTrial)
                        title = StringUtils.String("Label_TrialDaysLeftTitle_PARAMETRIZED");

                    text = StringUtils.String("Label_TrialDaysLeftDescription_PARAMETRIZED");
                    if (Session.IsOnFreeTrial)
                        text = StringUtils.String("Label_AccountDaysLeftDescription_PARAMETRIZED");

                    titleDays = StringUtils.String("Days_Days_PARAMETRIZED");
                    titleDays = string.Format(titleDays, daysLeft);

                    text = string.Format(text, daysLeft);
                }
            }

            GuiTitle.Text = title;
            GuiTitleDaysLeft.Text = " " + titleDays;
            GuiText.Text = text;
            GuiRenewButtonText.Text = renewButtonText;

            GuiProgressBar.Minimum = 0;
            GuiProgressBar.Maximum = 4;
            if (progressDaysLeft > GuiProgressBar.Maximum)
                progressDaysLeft = (int)GuiProgressBar.Maximum;
            GuiProgressBar.Value = GuiProgressBar.Maximum - progressDaysLeft;

        }

        private void GuiButtonRenew_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Constants.GetRenewUrl(Username));
            Close();
        }

        private void GuiButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
