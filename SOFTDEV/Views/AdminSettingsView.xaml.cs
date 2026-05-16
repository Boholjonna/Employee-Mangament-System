using System;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV.Views
{
    public partial class AdminSettingsView : UserControl
    {
        public Action? OnBack { get; set; }

        private bool _twoFAEnabled = false;

        // Default holidays list
        private readonly System.Collections.ObjectModel.ObservableCollection<string> _holidays = new()
        {
            "New Year's Day – Jan 1",
            "Independence Day – Jun 12",
            "Christmas Day – Dec 25",
        };

        public AdminSettingsView()
        {
            InitializeComponent();
            HolidayListBox.ItemsSource = _holidays;
        }

        // ── Account & Security ────────────────────────────────────────

        private void UpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            string current = CurrentPasswordBox.Password;
            string newPwd  = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPwd))
            {
                MessageBox.Show("Please fill in all password fields.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPwd != confirm)
            {
                MessageBox.Show("New password and confirmation do not match.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPwd.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: wire to DatabaseHelper.UpdateAdminPassword(current, newPwd)
            MessageBox.Show("Password updated successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void TwoFAToggle_Click(object sender, RoutedEventArgs e)
        {
            _twoFAEnabled = !_twoFAEnabled;

            TwoFAStatusLabel.Text       = _twoFAEnabled ? "Enabled"  : "Disabled";
            TwoFAStatusLabel.Foreground = _twoFAEnabled
                ? new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x4a, 0xd9, 0x6a))
                : new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xe8, 0xc8, 0x4a));

            TwoFAToggleButton.Content = _twoFAEnabled ? "Disable 2FA" : "Enable 2FA";

            MessageBox.Show(
                _twoFAEnabled
                    ? "Two-factor authentication has been enabled."
                    : "Two-factor authentication has been disabled.",
                "2FA", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveSecurity_Click(object sender, RoutedEventArgs e)
        {
            // TODO: persist security preferences
            MessageBox.Show("Security settings saved.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── System Settings ───────────────────────────────────────────

        private void SaveCompanyInfo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompanyNameBox.Text))
            {
                MessageBox.Show("Company name cannot be empty.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // TODO: persist to DB / config
            MessageBox.Show("Company information saved.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveTimezone_Click(object sender, RoutedEventArgs e)
        {
            // TODO: persist selected timezone
            MessageBox.Show("Timezone saved.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveWorkingHours_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WorkStartBox.Text) ||
                string.IsNullOrWhiteSpace(WorkEndBox.Text))
            {
                MessageBox.Show("Please enter both start and end times.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // TODO: persist working hours
            MessageBox.Show("Working hours saved.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddHoliday_Click(object sender, RoutedEventArgs e)
        {
            string name = HolidayNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            _holidays.Add(name);
            HolidayNameBox.Clear();
        }

        private void RemoveHoliday_Click(object sender, RoutedEventArgs e)
        {
            if (HolidayListBox.SelectedItem is string selected)
                _holidays.Remove(selected);
        }

        // ── Notification Settings ─────────────────────────────────────

        private void SaveNotifications_Click(object sender, RoutedEventArgs e)
        {
            // TODO: persist notification preferences
            MessageBox.Show("Notification settings saved.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── User Management ───────────────────────────────────────────

        private void SaveUserPolicies_Click(object sender, RoutedEventArgs e)
        {
            // TODO: persist user policies
            MessageBox.Show("User policies saved.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
