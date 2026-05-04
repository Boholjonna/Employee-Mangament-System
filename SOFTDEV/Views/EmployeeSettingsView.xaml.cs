using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV.Views
{
    public partial class EmployeeSettingsView : UserControl
    {
        public EmployeeSettingsView()
        {
            InitializeComponent();
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Profile information saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password))
            {
                MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(NewPasswordBox.Password) || NewPasswordBox.Password.Length < 8)
            {
                MessageBox.Show("New password must be at least 8 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("New password and confirmation do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void SavePreferences_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Notification preferences saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Photo upload functionality coming soon!", "Upload Photo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
