using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV.Views
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            var recentReports = new List<ReportItem>
            {
                new ReportItem { Icon = "📊", Name = "Task Summary - May 2026", Details = "Generated on May 3, 2026 • PDF • 2.4 MB" },
                new ReportItem { Icon = "📅", Name = "Monthly Attendance - April 2026", Details = "Generated on May 1, 2026 • Excel • 1.8 MB" },
                new ReportItem { Icon = "⭐", Name = "Performance Report - Q1 2026", Details = "Generated on Apr 28, 2026 • PDF • 3.1 MB" },
                new ReportItem { Icon = "📈", Name = "Complete Report - March 2026", Details = "Generated on Apr 15, 2026 • PDF • 4.5 MB" }
            };

            RecentReportsControl.ItemsSource = recentReports;
        }

        private void PreviewReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Report preview will open in a new window.", "Preview", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var reportType = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var format = (ExportFormatComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            MessageBox.Show($"Generating {reportType} in {format} format...\n\nReport will be downloaded shortly!", 
                "Generate Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Downloading report...", "Download", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void QuickReport_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            MessageBox.Show($"Generating {button?.Content}...", "Quick Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class ReportItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
