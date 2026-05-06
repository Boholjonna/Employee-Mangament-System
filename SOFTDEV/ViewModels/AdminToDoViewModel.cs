using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        // ── Form field backing fields ─────────────────────────────────────
        private string  _taskTitle       = string.Empty;
        private string  _taskDescription = string.Empty;
        private string  _assignedTo      = string.Empty;
        private string? _dueDate;

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

        public string? DueDate
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

            SaveTaskCommand = new RelayCommand(
                execute:    _ => ExecuteSave(),
                canExecute: _ => !string.IsNullOrWhiteSpace(TaskTitle)
            );

            CancelTaskCommand = new RelayCommand(_ => ExecuteCancel());
            FilterCommand     = new RelayCommand(_ => { /* stub — future implementation */ });
            SortCommand       = new RelayCommand(_ => { /* stub — future implementation */ });
        }

        // ── Command implementations ───────────────────────────────────────
        private void ExecuteSave()
        {
            Tasks.Add(new AdminTaskItem
            {
                Title       = TaskTitle.Trim(),
                Description = TaskDescription.Trim(),
                AssignedTo  = AssignedTo,
                DueDate     = DueDate,
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
