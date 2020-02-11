using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for TimeIntervalDialog.xaml
    /// </summary>
    public partial class TimeIntervalDialog : Window
    {
        public static bool ShowInputTimeIntervalDialog(out double retIntervalSeconds, Window owner = null, string descriptionText = null)
        {
            retIntervalSeconds = 0;

            var dlg = new TimeIntervalDialog();

            if (string.IsNullOrEmpty(descriptionText) == false)
                dlg.GuiTextBlockTextDescription.Text = descriptionText;

            if (owner != null)
                dlg.Owner = owner;

            if (dlg.ShowDialog() == true)
            {
                retIntervalSeconds = dlg.__ResultInSeconds;
                return true;
            }

            return false;
        }

        private double __ResultInSeconds;

        private TimeIntervalDialog()
        {
            InitializeComponent();
            GuiTextBoxHours.Focus();
        }

        private void GuiButton_OnOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                __ResultInSeconds = double.Parse(GuiTextBoxHours.Text) * 60 * 60;
                __ResultInSeconds += double.Parse(GuiTextBoxMinutes.Text) * 60;
            }
            catch
            {
                MessageBox.Show(this, "Please, enter correct time interval", "Wrong data", MessageBoxButton.OK);
                return;
            }

            DialogResult = true;
            Close();
        }
        
        private void GuiTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex RegexNumbers = new Regex("[^0-9]+$"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !RegexNumbers.IsMatch(text);
        }
    }
}
