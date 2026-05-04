using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV.Views
{
    public partial class EmployeeTasksView : UserControl
    {
        private List<TaskItem> _allTasks = new();

        public EmployeeTasksView()
        {
            try
            {
                InitializeComponent();
                Loaded += EmployeeTasksView_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Tasks view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EmployeeTasksView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasks()
        {
            _allTasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Complete Q2 Report",
                    Description = "Finalize quarterly performance report with all metrics and analysis",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    DueDate = "Due: May 28, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Review Code Changes",
                    Description = "Review pull request #234 for the new authentication module",
                    Status = "Not Started",
                    StatusColor = "#f44336",
                    DueDate = "Due: May 25, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Team Meeting Preparation",
                    Description = "Prepare slides and agenda for weekly team sync meeting",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    DueDate = "Completed: May 20, 2026",
                    IsCompleted = true
                },
                new TaskItem
                {
                    Title = "Update Documentation",
                    Description = "Update API documentation for v2.0 release with new endpoints",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    DueDate = "Due: May 30, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Client Presentation",
                    Description = "Present project progress and roadmap to client stakeholders",
                    Status = "Not Started",
                    StatusColor = "#f44336",
                    DueDate = "Due: June 5, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Database Optimization",
                    Description = "Optimize slow queries and add proper indexes",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    DueDate = "Due: May 27, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Security Audit",
                    Description = "Conduct security audit of authentication system",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    DueDate = "Completed: May 18, 2026",
                    IsCompleted = true
                },
                new TaskItem
                {
                    Title = "Unit Tests",
                    Description = "Write unit tests for new payment module",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    DueDate = "Due: May 29, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Performance Testing",
                    Description = "Run load tests on production environment",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    DueDate = "Completed: May 19, 2026",
                    IsCompleted = true
                },
                new TaskItem
                {
                    Title = "Bug Fixes",
                    Description = "Fix reported bugs in user dashboard",
                    Status = "In Progress",
                    StatusColor = "#ff9800",
                    DueDate = "Due: May 26, 2026",
                    IsCompleted = false
                },
                new TaskItem
                {
                    Title = "Code Review Training",
                    Description = "Complete code review best practices training",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    DueDate = "Completed: May 15, 2026",
                    IsCompleted = true
                },
                new TaskItem
                {
                    Title = "Sprint Planning",
                    Description = "Participate in sprint planning for next iteration",
                    Status = "Completed",
                    StatusColor = "#4caf50",
                    DueDate = "Completed: May 21, 2026",
                    IsCompleted = true
                }
            };

            if (TaskListControl != null)
            {
                TaskListControl.ItemsSource = _allTasks;
            }
        }

        private void TaskFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (TaskFilterComboBox?.SelectedItem is ComboBoxItem selected)
                {
                    string filter = selected.Content?.ToString() ?? "All Tasks";
                    
                    List<TaskItem> filteredTasks = filter switch
                    {
                        "In Progress" => _allTasks.FindAll(t => t.Status == "In Progress"),
                        "Completed" => _allTasks.FindAll(t => t.Status == "Completed"),
                        "Not Started" => _allTasks.FindAll(t => t.Status == "Not Started"),
                        _ => _allTasks
                    };

                    if (TaskListControl != null)
                    {
                        TaskListControl.ItemsSource = filteredTasks;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add new task dialog coming soon!", "Add Task", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateTask_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update task dialog coming soon!", "Update Task", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
