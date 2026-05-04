using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SOFTDEV.Views
{
    public partial class TasksView : UserControl
    {
        private List<TaskItemDetailed> _allTasks = new();
        private List<TaskItemDetailed> _filteredTasks = new();

        public TasksView()
        {
            InitializeComponent();
            InitializeSampleData();
            ApplyFilters();
            DrawCharts();
        }

        private void InitializeSampleData()
        {
            _allTasks = new List<TaskItemDetailed>
            {
                new TaskItemDetailed
                {
                    Title = "Complete Q2 Financial Report",
                    Description = "Finalize quarterly financial analysis and prepare presentation for stakeholders",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    Priority = "High",
                    PriorityColor = "#f44336",
                    DueDate = "Due: May 28, 2026",
                    Progress = "Progress: 65%",
                    IsCompleted = false
                },
                new TaskItemDetailed
                {
                    Title = "Review Code Pull Request #234",
                    Description = "Review and approve authentication module changes",
                    Status = "Not Started",
                    StatusColor = "#f44336",
                    Priority = "High",
                    PriorityColor = "#f44336",
                    DueDate = "Due: May 25, 2026",
                    Progress = "Progress: 0%",
                    IsCompleted = false
                },
                new TaskItemDetailed
                {
                    Title = "Update API Documentation",
                    Description = "Document new endpoints for v2.0 release",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    Priority = "Medium",
                    PriorityColor = "#ff9800",
                    DueDate = "Due: May 30, 2026",
                    Progress = "Progress: 40%",
                    IsCompleted = false
                },
                new TaskItemDetailed
                {
                    Title = "Team Meeting Preparation",
                    Description = "Prepare slides and agenda for weekly sync",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    Priority = "Low",
                    PriorityColor = "#2196f3",
                    DueDate = "Completed: May 20, 2026",
                    Progress = "Progress: 100%",
                    IsCompleted = true
                },
                new TaskItemDetailed
                {
                    Title = "Client Presentation Deck",
                    Description = "Create presentation showcasing project progress",
                    Status = "Not Started",
                    StatusColor = "#f44336",
                    Priority = "High",
                    PriorityColor = "#f44336",
                    DueDate = "Due: June 5, 2026",
                    Progress = "Progress: 0%",
                    IsCompleted = false
                },
                new TaskItemDetailed
                {
                    Title = "Database Performance Optimization",
                    Description = "Analyze and optimize slow queries",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    Priority = "Medium",
                    PriorityColor = "#ff9800",
                    DueDate = "Due: June 1, 2026",
                    Progress = "Progress: 30%",
                    IsCompleted = false
                },
                new TaskItemDetailed
                {
                    Title = "Security Audit Report",
                    Description = "Complete security assessment and document findings",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    Priority = "High",
                    PriorityColor = "#f44336",
                    DueDate = "Completed: May 15, 2026",
                    Progress = "Progress: 100%",
                    IsCompleted = true
                },
                new TaskItemDetailed
                {
                    Title = "User Training Materials",
                    Description = "Develop training guides for new features",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    Priority = "Low",
                    PriorityColor = "#2196f3",
                    DueDate = "Due: June 10, 2026",
                    Progress = "Progress: 20%",
                    IsCompleted = false
                }
            };
        }

        private void ApplyFilters()
        {
            _filteredTasks = _allTasks.ToList();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchTerm = SearchTextBox.Text.ToLower();
                _filteredTasks = _filteredTasks.Where(t =>
                    t.Title.ToLower().Contains(searchTerm) ||
                    t.Description.ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Apply status filter
            if (StatusFilterComboBox.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "All Status")
            {
                string status = statusItem.Content.ToString();
                _filteredTasks = _filteredTasks.Where(t => t.Status == status).ToList();
            }

            // Apply priority filter
            if (PriorityFilterComboBox.SelectedItem is ComboBoxItem priorityItem && priorityItem.Content.ToString() != "All Priority")
            {
                string priority = priorityItem.Content.ToString();
                _filteredTasks = _filteredTasks.Where(t => t.Priority == priority).ToList();
            }

            // Update statistics
            UpdateStatistics();

            // Update task list
            TaskListControl.ItemsSource = _filteredTasks;
        }

        private void UpdateStatistics()
        {
            TotalTasksText.Text = _allTasks.Count.ToString();
            InProgressTasksText.Text = _allTasks.Count(t => t.Status == "In Progress").ToString();
            CompletedTasksText.Text = _allTasks.Count(t => t.Status == "Completed").ToString();
            OverdueTasksText.Text = "2"; // Placeholder

            int completed = _allTasks.Count(t => t.Status == "Completed");
            int total = _allTasks.Count;
            int completedPercent = total > 0 ? (completed * 100 / total) : 0;
            int pendingPercent = 100 - completedPercent;

            CompletedPercentText.Text = $"{completedPercent}%";
            PendingPercentText.Text = $"{pendingPercent}%";
        }

        private void DrawCharts()
        {
            DrawPieChart();
            DrawTrendChart();
        }

        private void DrawPieChart()
        {
            CompletionPieChart.Children.Clear();

            int completed = _allTasks.Count(t => t.Status == "Completed");
            int total = _allTasks.Count;
            double completedPercent = total > 0 ? (double)completed / total : 0;

            double centerX = 100;
            double centerY = 100;
            double radius = 80;

            // Draw completed arc
            if (completedPercent > 0)
            {
                Path completedPath = CreateArcPath(centerX, centerY, radius, 0, completedPercent * 360, Color.FromRgb(76, 175, 80));
                CompletionPieChart.Children.Add(completedPath);
            }

            // Draw pending arc
            if (completedPercent < 1)
            {
                Path pendingPath = CreateArcPath(centerX, centerY, radius, completedPercent * 360, 360, Color.FromRgb(255, 152, 0));
                CompletionPieChart.Children.Add(pendingPath);
            }

            // Draw center circle (donut effect)
            Ellipse centerCircle = new Ellipse
            {
                Width = radius * 1.2,
                Height = radius * 1.2,
                Fill = new SolidColorBrush(Color.FromRgb(21, 21, 27))
            };
            Canvas.SetLeft(centerCircle, centerX - radius * 0.6);
            Canvas.SetTop(centerCircle, centerY - radius * 0.6);
            CompletionPieChart.Children.Add(centerCircle);

            // Draw percentage text
            TextBlock percentText = new TextBlock
            {
                Text = $"{(int)(completedPercent * 100)}%",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 24,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(percentText, centerX - 25);
            Canvas.SetTop(percentText, centerY - 12);
            CompletionPieChart.Children.Add(percentText);
        }

        private Path CreateArcPath(double centerX, double centerY, double radius, double startAngle, double endAngle, Color color)
        {
            double startRad = startAngle * Math.PI / 180;
            double endRad = endAngle * Math.PI / 180;

            Point startPoint = new Point(
                centerX + radius * Math.Cos(startRad - Math.PI / 2),
                centerY + radius * Math.Sin(startRad - Math.PI / 2)
            );

            Point endPoint = new Point(
                centerX + radius * Math.Cos(endRad - Math.PI / 2),
                centerY + radius * Math.Sin(endRad - Math.PI / 2)
            );

            bool largeArc = (endAngle - startAngle) > 180;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = new Point(centerX, centerY) };
            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0, largeArc, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));
            geometry.Figures.Add(figure);

            return new Path
            {
                Data = geometry,
                Fill = new SolidColorBrush(color)
            };
        }

        private void DrawTrendChart()
        {
            TrendLineChart.Children.Clear();

            double width = 400;
            double height = 180;
            double[] data = { 45, 58, 62, 71, 78, 85 }; // Weekly completion rates

            // Draw grid lines
            for (int i = 0; i <= 4; i++)
            {
                double y = height - (i * height / 4);
                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(42, 42, 62)),
                    StrokeThickness = 1
                };
                TrendLineChart.Children.Add(gridLine);
            }

            // Draw line chart
            double segmentWidth = width / (data.Length - 1);
            for (int i = 0; i < data.Length - 1; i++)
            {
                double x1 = i * segmentWidth;
                double y1 = height - (data[i] / 100.0 * height);
                double x2 = (i + 1) * segmentWidth;
                double y2 = height - (data[i + 1] / 100.0 * height);

                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(Color.FromRgb(123, 97, 255)),
                    StrokeThickness = 3
                };
                TrendLineChart.Children.Add(line);

                // Draw data points
                Ellipse point = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Color.FromRgb(123, 97, 255))
                };
                Canvas.SetLeft(point, x1 - 4);
                Canvas.SetTop(point, y1 - 4);
                TrendLineChart.Children.Add(point);
            }

            // Draw last point
            Ellipse lastPoint = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(123, 97, 255))
            };
            Canvas.SetLeft(lastPoint, width - 4);
            Canvas.SetTop(lastPoint, height - (data[data.Length - 1] / 100.0 * height) - 4);
            TrendLineChart.Children.Add(lastPoint);
        }

        // Event Handlers
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Sort_Changed(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement sorting logic
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add new task dialog coming soon!", "Add Task", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateProgress_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update progress dialog coming soon!", "Update Progress", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add note dialog coming soon!", "Add Note", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class TaskItemDetailed
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
