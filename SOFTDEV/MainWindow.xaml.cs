using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SOFTDEV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ── Login ────────────────────────────────────────────────────────────

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;

            var result = ValidateInputs();
            if (!result.IsValid)
            {
                ErrorMessageText.Text = result.Message;
                ErrorMessageText.Visibility = Visibility.Visible;
                return;
            }

            string username = UsernameTextBox.Text.Trim();
            string email    = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // ── Database authentication ──────────────────────────────
            if (RoleAdmin.IsChecked == true)
            {
                string? adminUsername = DatabaseHelper.GetAdminUsername(username, password);
                if (adminUsername == null)
                {
                    ErrorMessageText.Text = "Invalid admin username or password.";
                    ErrorMessageText.Visibility = Visibility.Visible;
                    return;
                }
                string adminName = DatabaseHelper.GetAdminName(username, password) ?? adminUsername;
                OpenAdminDashboard(adminName);
            }
            else if (RoleEmployee.IsChecked == true)
            {
                if (!DatabaseHelper.AuthenticateEmployee(username, password))
                {
                    ErrorMessageText.Text = "Invalid employee username or password.";
                    ErrorMessageText.Visibility = Visibility.Visible;
                    return;
                }
                // Use the employee's real name (matches the attendance table's employeename column)
                string employeeName = DatabaseHelper.GetEmployeeName(username, password);
                OpenEmployeeDashboard(employeeName);
            }
            else if (RoleManager.IsChecked == true)
            {
                string? managerName = DatabaseHelper.GetManagerName(username, password);
                if (managerName == null)
                {
                    ErrorMessageText.Text = "Invalid manager username or password.";
                    ErrorMessageText.Visibility = Visibility.Visible;
                    return;
                }
                OpenManagerDashboard(managerName);
            }
            else
            {
                ErrorMessageText.Text = "Please select a user role.";
                ErrorMessageText.Visibility = Visibility.Visible;
                return;
            }

            Close();
        }

        private ValidationResult ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                return ValidationResult.Fail("Username is required.");

            if (PasswordBox.Password.Length < 1)
                return ValidationResult.Fail("Password is required.");

            return ValidationResult.Ok();
        }

        // ── Role checkboxes (mutually exclusive) ────────────────────────────

        private void RoleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox selected) return;

            // Uncheck the other two so only one role can be active at a time
            if (selected != RoleEmployee) RoleEmployee.IsChecked = false;
            if (selected != RoleManager)  RoleManager.IsChecked  = false;
            if (selected != RoleAdmin)    RoleAdmin.IsChecked    = false;
        }

        private void RoleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Nothing extra needed; unchecking is always allowed
        }

        // ── Language dropdown ────────────────────────────────────────────────

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            LanguageContextMenu.IsOpen = true;
        }

        private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
                LanguageButton.Content = item.Header?.ToString() ?? "EN";
        }

        // ── Google sign-in placeholder ───────────────────────────────────────

        private void GoogleSignInButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: OAuth2 Google sign-in flow
        }

        // ── Sign-up link placeholder ─────────────────────────────────────────

        private void SignUpLink_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to registration window
        }

        // ── Navigation ───────────────────────────────────────────────────────

        public void OpenManagerDashboard(string managerName)
        {
            new ManagerDashboard(managerName).Show();
        }

        public void OpenAdminDashboard(string username)
        {
            new AdminDashboard(username).Show();
        }

        public void OpenEmployeeDashboard(string username)
        {
            new EmployeeDashboard(username).Show();
        }
    }
}
