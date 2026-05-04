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

            string username = UsernameTextBox.Text;
            string email    = EmailTextBox.Text;
            string password = PasswordBox.Password;

            // TODO: Pass to AuthService
            // Navigate based on selected role
            if (RoleEmployee.IsChecked == true)
            {
                OpenEmployeeDashboard();
            }
            else if (RoleManager.IsChecked == true)
            {
                // TODO: Open Manager Dashboard
                MessageBox.Show("Manager Dashboard coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (RoleAdmin.IsChecked == true)
            {
                OpenAdminDashboard();
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
                return ValidationResult.Fail("Employee User Name is required.");

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                return ValidationResult.Fail("Email ID is required.");

            if (!EmailValidator.IsValidEmailFormat(EmailTextBox.Text))
                return ValidationResult.Fail("Email ID is not a valid format.");

            if (PasswordBox.Password.Length < 8)
                return ValidationResult.Fail("Password must be at least 8 characters.");

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

        public void OpenAdminDashboard()
        {
            new AdminDashboard().Show();
        }

        public void OpenEmployeeDashboard()
        {
            new EmployeeDashboard().Show();
        }
    }
}
