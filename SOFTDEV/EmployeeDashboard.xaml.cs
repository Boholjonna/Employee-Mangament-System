using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        public void NavigateToSection(string sectionName)
        {
            // Clear current content
            MainContentControl.Content = null;

            // Load the appropriate view based on section name
            switch (sectionName)
            {
                case "Dashboard":
                    MainContentControl.Content = new EmployeeDashboardView(LoggedInEmployeeName);
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
                    MainContentControl.Content = new EmployeeDashboardView(LoggedInEmployeeName);
                    HighlightButton(DashboardButton);
                    break;
            }
        }

        // ── Button Highlighting ───────────────────────────────────────
        private void HighlightButton(Button activeButton)
        {
            Color defaultColor = Color.FromRgb(123, 97, 255);     // Purple
            Color activeColor = Color.FromRgb(200, 100, 255);     // Violet-Pink
            TimeSpan duration = TimeSpan.FromMilliseconds(300);

            // Reset all buttons with animation
            foreach (Button btn in new[] { DashboardButton, TasksButton, AttendanceButton, PerformanceButton, ReportsButton, SettingsButton })
            {
                AnimateButtonColor(btn, defaultColor, duration);
            }

            // Highlight the active button with animation
            AnimateButtonColor(activeButton, activeColor, duration);
        }

        private void AnimateButtonColor(Button button, Color targetColor, TimeSpan duration)
        {
            // Create a new brush that is NOT frozen, so we can animate it
            SolidColorBrush animatableBrush = new SolidColorBrush(button.Background is SolidColorBrush sb ? sb.Color : Color.FromRgb(123, 97, 255));
            button.Background = animatableBrush;

            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = targetColor,
                Duration = new Duration(duration),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animatableBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
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
