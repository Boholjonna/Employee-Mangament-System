using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SOFTDEV.Views;

namespace SOFTDEV
{
    /// <summary>
    /// Employee Dashboard window — code-behind.
    /// </summary>
    public partial class EmployeeDashboard : Window
    {
        public string LoggedInEmployeeName { get; private set; }

        // ── Constructor ───────────────────────────────────────────────
        public EmployeeDashboard(string employeeName)
        {
            InitializeComponent();

            LoggedInEmployeeName = employeeName;

            // Show Dashboard view by default
            NavigateToSection("Dashboard");
        }

        // ── Navigation Method ─────────────────────────────────────────
        private void NavigateToSection(string sectionName)
        {
            // Clear current content
            MainContentControl.Content = null;

            // Load the appropriate view based on section name
            switch (sectionName)
            {
                case "Dashboard":
                    MainContentControl.Content = new EmployeeDashboardView();
                    HighlightButton(DashboardButton);
                    break;

                case "Tasks":
                    MainContentControl.Content = new EmployeeTasksView(LoggedInEmployeeName);
                    HighlightButton(TasksButton);
                    break;

                case "Attendance":
                    MainContentControl.Content = new AttendanceView(LoggedInEmployeeName);
                    HighlightButton(AttendanceButton);
                    break;

                case "Performance":
                    MainContentControl.Content = new PerformanceView();
                    HighlightButton(PerformanceButton);
                    break;

                case "Reports":
                    MainContentControl.Content = new ReportsView();
                    HighlightButton(ReportsButton);
                    break;

                case "Settings":
                    MainContentControl.Content = new EmployeeSettingsView();
                    HighlightButton(SettingsButton);
                    break;

                default:
                    MainContentControl.Content = new EmployeeDashboardView();
                    HighlightButton(DashboardButton);
                    break;
            }
        }

        // ── Button Highlighting ───────────────────────────────────────
        private void HighlightButton(Button activeButton)
        {
            // Reset all buttons to default style
            DashboardButton.Background = new SolidColorBrush(Color.FromRgb(123, 97, 255));
            TasksButton.Background = new SolidColorBrush(Color.FromRgb(123, 97, 255));
            AttendanceButton.Background = new SolidColorBrush(Color.FromRgb(123, 97, 255));
            PerformanceButton.Background = new SolidColorBrush(Color.FromRgb(123, 97, 255));
            ReportsButton.Background = new SolidColorBrush(Color.FromRgb(123, 97, 255));
            SettingsButton.Background = new SolidColorBrush(Color.FromRgb(123, 97, 255));

            // Highlight the active button
            activeButton.Background = new SolidColorBrush(Color.FromRgb(106, 82, 224)); // Darker purple
        }

        // ── Control Group handlers ────────────────────────────────────
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(SearchButton_Click));
            MessageBox.Show("Search functionality coming soon!", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(NotificationButton_Click));
            MessageBox.Show("You have 4 new notifications!", "Notifications", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UserNameButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(UserNameButton_Click));
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(AvatarButton_Click));
            MessageBox.Show("Profile settings coming soon!", "Profile", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Navigation Bar handler ────────────────────────────────────
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string section = button.Content.ToString() ?? "Dashboard";
                System.Diagnostics.Debug.WriteLine($"Navigation to: {section}");
                NavigateToSection(section);
            }
        }

        // ── Task Management handlers ──────────────────────────────────
        private void TaskFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Handled in individual views
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Handled in individual views
        }

        private void UpdateTask_Click(object sender, RoutedEventArgs e)
        {
            // Handled in individual views
        }

        // ── Quick Actions handlers ────────────────────────────────────
        private void ClockInOut_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(ClockInOut_Click));
            DateTime now = DateTime.Now;
            MessageBox.Show($"Clock In/Out recorded at {now:HH:mm:ss}", "Attendance", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RequestLeave_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(RequestLeave_Click));
            MessageBox.Show("Leave request form coming soon!", "Request Leave", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewPerformance_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(ViewPerformance_Click));
            NavigateToSection("Performance");
        }

        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(DownloadReport_Click));
            NavigateToSection("Reports");
        }
    }
}
