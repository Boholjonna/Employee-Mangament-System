using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SOFTDEV;

namespace SOFTDEV.ViewModels
{
    /// <summary>
    /// ViewModel for the AdminToDoTab. Holds all form state, the task collection,
    /// and the four commands exposed to the View via data binding.
    /// </summary>
    public class AdminToDoViewModel : INotifyPropertyChanged
    {
        // ── Greeting ──────────────────────────────────────────────────────
        /// <summary>The admin's display name, set once in the constructor.</summary>
        public string AdminName { get; }

        // ── Employee list for AssignedTo ComboBox ─────────────────────────
        /// <summary>Employee names loaded from the database for the Assigned To dropdown.</summary>
        public ObservableCollection<string> EmployeeNames { get; } = new();

        // ── Form field backing fields ─────────────────────────────────────
        private string  _taskTitle       = string.Empty;
        private string  _taskDescription = string.Empty;
        private string  _assignedTo      = string.Empty;
        private DateTime? _dueDate;

        // ── Form field properties (TwoWay bindings) ───────────────────────
        public string TaskTitle
        {
            get => _taskTitle;
            set
            {
                _taskTitle = value;
                OnPropertyChanged();
                SaveTaskCommand.RaiseCanExecuteChanged();
            }
        }

        public string TaskDescription
        {
            get => _taskDescription;
            set
            {
                _taskDescription = value;
                OnPropertyChanged();
            }
        }

        public string AssignedTo
        {
            get => _assignedTo;
            set
            {
                _assignedTo = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        // ── Task collection ───────────────────────────────────────────────
        /// <summary>The live collection of tasks displayed in the task list.</summary>
        public ObservableCollection<AdminTaskItem> Tasks { get; } = new();

        // ── Commands ──────────────────────────────────────────────────────
        public RelayCommand SaveTaskCommand   { get; }
        public RelayCommand CancelTaskCommand { get; }
        public RelayCommand FilterCommand     { get; }
        public RelayCommand SortCommand       { get; }

        // ── Constructor ───────────────────────────────────────────────────
        public AdminToDoViewModel(string adminName)
        {
            AdminName = adminName;

            // Load employee names from DB for the AssignedTo dropdown
            LoadEmployeeNames();

            SaveTaskCommand = new RelayCommand(
                execute:    _ => ExecuteSave(),
                canExecute: _ => !string.IsNullOrWhiteSpace(TaskTitle)
            );

            CancelTaskCommand = new RelayCommand(_ => ExecuteCancel());
            FilterCommand     = new RelayCommand(_ => { /* stub — future implementation */ });
            SortCommand       = new RelayCommand(_ => { /* stub — future implementation */ });
        }

        // ── Load employees ────────────────────────────────────────────────
        private void LoadEmployeeNames()
        {
            try
            {
                var employees = DatabaseHelper.GetAllEmployees();
                EmployeeNames.Clear();
                foreach (var emp in employees)
                    EmployeeNames.Add(emp.Name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ViewModel] LoadEmployeeNames error: {ex.Message}");
            }
        }

        // ── Command implementations ───────────────────────────────────────
        private void ExecuteSave()
        {
            string title       = TaskTitle.Trim();
            string description = TaskDescription.Trim();
            string assignedTo  = AssignedTo;
            // Format date as yyyy-MM-dd for the varchar column, or empty string if not picked
            string dueDate     = DueDate.HasValue
                ? DueDate.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            // Persist to database
            bool saved = DatabaseHelper.SaveTask(title, description, assignedTo, dueDate);
            if (!saved)
            {
                MessageBox.Show(
                    "Failed to save task to the database. Please check your connection.",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Add to in-memory list for immediate UI feedback
            Tasks.Add(new AdminTaskItem
            {
                Title       = title,
                Description = description,
                AssignedTo  = assignedTo,
                DueDate     = dueDate,
                CreatedAt   = DateTime.Now,
                Status      = "Pending"
            });

            ExecuteCancel(); // clear form after save
        }

        private void ExecuteCancel()
        {
            TaskTitle       = string.Empty;
            TaskDescription = string.Empty;
            AssignedTo      = string.Empty;
            DueDate         = null;
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
