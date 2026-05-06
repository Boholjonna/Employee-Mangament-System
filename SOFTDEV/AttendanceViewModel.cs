using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SOFTDEV
{
    /// <summary>
    /// ViewModel for the AttendanceDashboard window.
    /// Implements <see cref="INotifyPropertyChanged"/> for WPF data binding.
    /// </summary>
    public class AttendanceViewModel : INotifyPropertyChanged
    {
        // ── Backing store — never modified after initial load ─────────────────
        private List<AttendanceRecord_Model> _allRecords = new();

        // ── Active filter state ───────────────────────────────────────────────
        private string _filterEmployee = string.Empty;
        private string _filterStatus   = "All";
        private string _filterDateFrom = string.Empty;
        private string _filterDateTo   = string.Empty;

        // ── Active sort state ─────────────────────────────────────────────────
        private string _sortColumn    = "Date";
        private bool   _sortAscending = true;

        // ── Bound properties ──────────────────────────────────────────────────

        /// <summary>
        /// The observable collection of attendance records bound to the UI ItemsControl.
        /// Contents are replaced (Clear + Add) on filter/sort; the reference never changes.
        /// </summary>
        public ObservableCollection<AttendanceRecord_Model> AttendanceRecords { get; } = new();

        /// <summary>
        /// Greeting message displayed at the top of the dashboard.
        /// Set once in the constructor from the logged-in admin's name.
        /// </summary>
        public string AdminGreeting { get; private set; } = "Hello Admin! 👋";

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Initialises the ViewModel, sets the admin greeting, and loads attendance records.
        /// </summary>
        /// <param name="adminName">The display name of the currently logged-in admin.</param>
        public AttendanceViewModel(string adminName)
        {
            AdminGreeting = string.IsNullOrWhiteSpace(adminName)
                ? "Hello Admin! 👋"
                : $"Hello {adminName}! 👋";

            LoadRecords();
        }

        /// <summary>
        /// Test-only constructor that seeds <see cref="_allRecords"/> directly,
        /// bypassing the database call in <see cref="LoadRecords"/>.
        /// Accessible to the test project via <c>InternalsVisibleTo</c>.
        /// </summary>
        /// <param name="records">The pre-seeded list of attendance records.</param>
        internal AttendanceViewModel(List<AttendanceRecord_Model> records)
        {
            _allRecords = records;
            foreach (var r in _allRecords)
                AttendanceRecords.Add(r);
        }

        // ── Data loading ──────────────────────────────────────────────────────

        /// <summary>
        /// Loads all attendance records from the database into <see cref="_allRecords"/>
        /// and populates <see cref="AttendanceRecords"/> for UI binding.
        /// On any exception, <see cref="AttendanceRecords"/> is left empty and the error is logged.
        /// </summary>
        private void LoadRecords()
        {
            try
            {
                var records = DatabaseHelper.GetAllAttendanceRecords();

                _allRecords = records;

                AttendanceRecords.Clear();
                foreach (var record in _allRecords)
                    AttendanceRecords.Add(record);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AttendanceViewModel] LoadRecords error: {ex.Message}");
                AttendanceRecords.Clear();
            }
        }

        // ── Filter ────────────────────────────────────────────────────────────

        /// <summary>
        /// Filters <see cref="_allRecords"/> in-memory and replaces the contents of
        /// <see cref="AttendanceRecords"/> with the matching subset.
        /// </summary>
        /// <param name="employee">Substring match on <see cref="AttendanceRecord_Model.EmployeeName"/> (case-insensitive). Empty = no filter.</param>
        /// <param name="status">Exact match on <see cref="AttendanceRecord_Model.Status"/>. "All" = no filter.</param>
        /// <param name="dateFrom">Inclusive lower bound on <see cref="AttendanceRecord_Model.Date"/> (yyyy-MM-dd). Empty = no lower bound.</param>
        /// <param name="dateTo">Inclusive upper bound on <see cref="AttendanceRecord_Model.Date"/> (yyyy-MM-dd). Empty = no upper bound.</param>
        public void ApplyFilter(string employee, string status, string dateFrom, string dateTo)
        {
            // Persist active filter state
            _filterEmployee = employee;
            _filterStatus   = status;
            _filterDateFrom = dateFrom;
            _filterDateTo   = dateTo;

            var filtered = _allRecords.Where(r =>
            {
                // Employee name contains (OrdinalIgnoreCase) when non-empty
                if (!string.IsNullOrEmpty(employee) &&
                    r.EmployeeName.IndexOf(employee, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;

                // Status equals when not "All"
                if (status != "All" && r.Status != status)
                    return false;

                // Date >= dateFrom when non-empty
                if (!string.IsNullOrEmpty(dateFrom) &&
                    string.Compare(r.Date, dateFrom, StringComparison.Ordinal) < 0)
                    return false;

                // Date <= dateTo when non-empty
                if (!string.IsNullOrEmpty(dateTo) &&
                    string.Compare(r.Date, dateTo, StringComparison.Ordinal) > 0)
                    return false;

                return true;
            });

            AttendanceRecords.Clear();
            foreach (var record in filtered)
                AttendanceRecords.Add(record);
        }

        // ── Sort ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Sorts the current contents of <see cref="AttendanceRecords"/> by the specified column and direction.
        /// </summary>
        /// <param name="column">Column name to sort by (e.g., "Date", "Employee Name", "Status").</param>
        /// <param name="ascending"><see langword="true"/> for ascending order; <see langword="false"/> for descending.</param>
        public void ApplySort(string column, bool ascending)
        {
            _sortColumn    = column;
            _sortAscending = ascending;

            Func<AttendanceRecord_Model, string> keySelector = column switch
            {
                "Employee Name" => r => r.EmployeeName,
                "Time In"       => r => r.TimeIn,
                "Time Out"      => r => r.TimeOut,
                "Total Hours"   => r => r.TotalHours,
                "Status"        => r => r.Status,
                _               => r => r.Date,   // "Date" and any unknown column
            };

            var sorted = ascending
                ? AttendanceRecords.OrderBy(keySelector, StringComparer.OrdinalIgnoreCase).ToList()
                : AttendanceRecords.OrderByDescending(keySelector, StringComparer.OrdinalIgnoreCase).ToList();

            AttendanceRecords.Clear();
            foreach (var record in sorted)
                AttendanceRecords.Add(record);
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resets all active filters and restores <see cref="AttendanceRecords"/> to the full
        /// unfiltered contents of <see cref="_allRecords"/>.
        /// </summary>
        public void ResetFilter()
        {
            _filterEmployee = string.Empty;
            _filterStatus   = "All";
            _filterDateFrom = string.Empty;
            _filterDateTo   = string.Empty;

            AttendanceRecords.Clear();
            foreach (var record in _allRecords)
                AttendanceRecords.Add(record);
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
