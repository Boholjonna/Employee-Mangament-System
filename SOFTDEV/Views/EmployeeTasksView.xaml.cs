using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV.Views
{
    public partial class EmployeeTasksView : UserControl
    {
        private readonly string _employeeName;
        private List<TaskItem> _allTasks = new();

        /// <summary>
        /// Creates the view and loads tasks assigned to the given employee from the database.
        /// </summary>
        /// <param name="employeeName">
        /// The logged-in employee's name, matched against the <c>assignedto</c> column in the task table.
        /// </param>
        public EmployeeTasksView(string employeeName)
        {
            _employeeName = employeeName;

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
            _allTasks = DatabaseHelper.GetTasksByEmployee(_employeeName);

            if (TaskListControl != null)
                TaskListControl.ItemsSource = _allTasks;

            UpdateEmptyState();
            UpdateStatCards();
        }

        /// <summary>Shows or hides the "No task available" message based on task count.</summary>
        private void UpdateEmptyState()
        {
            if (NoTasksMessage != null)
                NoTasksMessage.Visibility = _allTasks.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        /// <summary>Updates the stat card numbers to reflect the actual DB data.</summary>
        private void UpdateStatCards()
        {
            int total      = _allTasks.Count;
            int inProgress = _allTasks.FindAll(t => t.Status == "In Progress").Count;
            int completed  = _allTasks.FindAll(t => t.Status == "Completed").Count;
            int notStarted = _allTasks.FindAll(t => t.Status == "Not Started").Count;

            if (TotalTasksCount  != null) TotalTasksCount.Text  = total.ToString();
            if (InProgressCount  != null) InProgressCount.Text  = inProgress.ToString();
            if (CompletedCount   != null) CompletedCount.Text   = completed.ToString();
            if (NotStartedCount  != null) NotStartedCount.Text  = notStarted.ToString();
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
                        "Completed"   => _allTasks.FindAll(t => t.Status == "Completed"),
                        "Not Started" => _allTasks.FindAll(t => t.Status == "Not Started"),
                        _             => _allTasks
                    };

                    if (TaskListControl != null)
                        TaskListControl.ItemsSource = filteredTasks;

                    // Show empty state if filtered list is empty
                    if (NoTasksMessage != null)
                        NoTasksMessage.Visibility = filteredTasks.Count == 0
                            ? Visibility.Visible
                            : Visibility.Collapsed;
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
