using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SOFTDEV
{
    /// <summary>
    /// Code-behind for AdminEmployeesView.xaml.
    /// Displays the full employee roster from the MySQL <c>employee</c> table.
    /// Falls back to a hardcoded placeholder list when the database is unavailable.
    /// </summary>
    public partial class AdminEmployeesView : Window
    {
        // ── Private fields ────────────────────────────────────────────
        private string          _username;
        private Window?         _ownerDashboard;
        private Button?         _selectedButton = null;
        private EmployeeDetail? _selectedDetail = null;

        // ── Public properties ─────────────────────────────────────────

        /// <summary>
        /// Gets or sets the list of employees currently displayed in the view.
        /// Exposed as a public property to enable testability without a live database.
        /// </summary>
        public List<EmployeeEntry> Employees { get; set; } = new();

        // ── Constructor ───────────────────────────────────────────────

        /// <summary>
        /// Initialises the view, sets the username label, and loads the employee list.
        /// </summary>
        /// <param name="username">The logged-in admin's username, shown in the header.</param>
        /// <param name="ownerDashboard">
        /// Optional reference to the parent <see cref="AdminDashboard"/> window.
        /// When provided, the dashboard is restored when this view is closed.
        /// </param>
        public AdminEmployeesView(string username, Window? ownerDashboard = null)
        {
            InitializeComponent();

            _username       = username;
            _ownerDashboard = ownerDashboard;

            UserNameButton.Content = username;

            LoadEmployees();
        }

        // ── Data loading ──────────────────────────────────────────────

        /// <summary>
        /// Fetches all employees from the database and binds them to
        /// <see cref="EmployeeListControl"/>. Falls back to a hardcoded placeholder
        /// list of at least three entries when the database returns an empty list
        /// or throws an exception.
        /// </summary>
        private void LoadEmployees()
        {
            try
            {
                Employees = DatabaseHelper.GetAllEmployees();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminEmployeesView] LoadEmployees error: {ex.Message}");
                Employees = new List<EmployeeEntry>();
            }

            if (Employees.Count == 0)
            {
                Employees = new List<EmployeeEntry>
                {
                    new EmployeeEntry("Alice Santos", "Software Engineer"),
                    new EmployeeEntry("Bob Reyes",    "Project Manager"),
                    new EmployeeEntry("Carol Lim",    "QA Analyst"),
                };
            }

            EmployeeListControl.ItemsSource = null;
            EmployeeListControl.ItemsSource = Employees;
        }

        // ── Navigation handlers ───────────────────────────────────────

        /// <summary>Restores the owner dashboard and closes this view.</summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _ownerDashboard?.Show();
            this.Close();
        }

        /// <summary>Routes nav-bar button clicks.</summary>
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == OverviewButton)
            {
                var overviewUI = new AdminOverviewUI(_username, this);
                this.Hide();
                overviewUI.Show();
            }
            else if (sender == AttendanceButton)
            {
                var attendanceDashboard = new AttendanceDashboard(_username);
                this.Hide();
                attendanceDashboard.Show();
            }
            Debug.WriteLine(nameof(NavButton_Click));
        }

        // ── Filter / Sort placeholder handlers ───────────────────────

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(nameof(FilterButton_Click));
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(nameof(SortButton_Click));
        }

        // ── Control group placeholder handlers ────────────────────────

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

        // ── Employee row click handler ────────────────────────────────

        /// <summary>
        /// Handles a click on an employee row button in the list.
        /// Highlights the selected row, loads the employee's full details from the
        /// database, and shows or hides the <see cref="DetailPanel"/> accordingly.
        /// Falls back to the list entry data when the database is unavailable.
        /// </summary>
        private void EmployeeRow_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            string? name = button.Tag as string;

            // Reset previous selection highlight
            if (_selectedButton != null)
                _selectedButton.Background = Brushes.Transparent;

            // Apply highlight to newly selected button
            button.Background = (Brush)FindResource("PurpleAccentBrush");
            _selectedButton = button;

            // Load employee details from the database
            EmployeeDetail? result = null;
            try
            {
                result = DatabaseHelper.GetEmployeeDetails(name ?? string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminEmployeesView] GetEmployeeDetails threw: {ex.Message}");
            }

            // If DB lookup failed, build a minimal detail from the list entry so the
            // panel always shows something rather than silently collapsing.
            if (result == null)
            {
                Debug.WriteLine($"[AdminEmployeesView] Falling back to list data for: {name}");
                var entry = Employees.Find(emp => emp.Name == name);
                result = new EmployeeDetail
                {
                    Name     = entry?.Name     ?? name ?? string.Empty,
                    Position = entry?.Position ?? string.Empty,
                };
            }

            DetailPanel.DataContext = result;
            DetailPanel.Visibility = Visibility.Visible;
            _selectedDetail = result;
        }
    }
}
