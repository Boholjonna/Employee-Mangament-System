using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV
{
    /// <summary>
    /// Code-behind for AttendanceDashboard.xaml.
    /// Displays all employee attendance records with filter and sort support.
    /// </summary>
    public partial class AttendanceDashboard : Window
    {
        // ── Private fields ────────────────────────────────────────────
        private readonly string              _username;
        private readonly AttendanceViewModel _viewModel;

        // ── Constructor ───────────────────────────────────────────────

        /// <summary>
        /// Initialises the window, creates the ViewModel, and sets the DataContext.
        /// </summary>
        /// <param name="username">The logged-in admin's username.</param>
        public AttendanceDashboard(string username)
        {
            InitializeComponent();

            _username  = username;
            _viewModel = new AttendanceViewModel(username);
            DataContext = _viewModel;

            UserNameButton.Content = username;
        }

        // ── Navigation handlers ───────────────────────────────────────

        /// <summary>Shows the owner window (if any) and closes this window.</summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Owner?.Show();
            this.Close();
        }

        /// <summary>Routes nav-bar button clicks to the appropriate admin view.</summary>
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == OverviewButton)
            {
                var overviewUI = new AdminOverviewUI(_username);
                this.Hide();
                overviewUI.Show();
            }
            else if (sender == EmployeesButton)
            {
                var employeesView = new AdminEmployeesView(_username);
                this.Hide();
                employeesView.Show();
            }
            else if (sender == AttendanceButton)
            {
                // Already on this view — no-op
            }
            else if (sender == ToDoButton)
            {
                // Navigate back to AdminDashboard and open Task Management tab
                var dashboard = new AdminDashboard(_username);
                dashboard.Show();
                this.Close();
                dashboard.OpenToDoTab();
            }
            else if (sender == ReportsButton)
            {
                // Navigate back to AdminDashboard and open Reports tab
                var dashboard = new AdminDashboard(_username);
                dashboard.Show();
                this.Close();
                dashboard.OpenReportsTab();
            }
            else if (sender == LeavesButton)
            {
                // Navigate back to AdminDashboard and open Leaves tab
                var dashboard = new AdminDashboard(_username);
                dashboard.Show();
                this.Close();
                dashboard.OpenLeavesTab();
            }
            else if (sender == SettingsButton)
            {
                Debug.WriteLine("[AttendanceDashboard] Settings navigation not yet implemented.");
            }
        }

        // ── Popup toggle handlers ─────────────────────────────────────

        /// <summary>Toggles the FilterPopup visibility.</summary>
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.Visibility = FilterPopup.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Close the other popup if open
            if (FilterPopup.Visibility == Visibility.Visible)
                SortPopup.Visibility = Visibility.Collapsed;
        }

        /// <summary>Toggles the SortPopup visibility.</summary>
        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            SortPopup.Visibility = SortPopup.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Close the other popup if open
            if (SortPopup.Visibility == Visibility.Visible)
                FilterPopup.Visibility = Visibility.Collapsed;
        }

        // ── Filter handlers ───────────────────────────────────────────

        /// <summary>
        /// Reads filter controls and calls <see cref="AttendanceViewModel.ApplyFilter"/>.
        /// </summary>
        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            string employee = FilterEmployeeText.Text;
            string status   = (FilterStatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            string dateFrom = FilterFromDate.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            string dateTo   = FilterToDate.SelectedDate?.ToString("yyyy-MM-dd")   ?? string.Empty;

            _viewModel.ApplyFilter(employee, status, dateFrom, dateTo);
            FilterPopup.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Clears all filter controls and calls <see cref="AttendanceViewModel.ResetFilter"/>.
        /// </summary>
        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterEmployeeText.Text      = string.Empty;
            FilterStatusCombo.SelectedIndex = 0;
            FilterFromDate.SelectedDate  = null;
            FilterToDate.SelectedDate    = null;

            _viewModel.ResetFilter();
            FilterPopup.Visibility = Visibility.Collapsed;
        }

        // ── Sort handler ──────────────────────────────────────────────

        /// <summary>
        /// Reads sort controls and calls <see cref="AttendanceViewModel.ApplySort"/>.
        /// </summary>
        private void ApplySort_Click(object sender, RoutedEventArgs e)
        {
            string column    = (SortColumnCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Date";
            bool   ascending = SortAscending.IsChecked == true;

            _viewModel.ApplySort(column, ascending);
            SortPopup.Visibility = Visibility.Collapsed;
        }

        // ── Control group stub handlers ───────────────────────────────

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(nameof(SearchButton_Click));
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(nameof(NotificationButton_Click));
        }

        private void UserNameButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(nameof(UserNameButton_Click));
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(nameof(AvatarButton_Click));
        }
    }
}
