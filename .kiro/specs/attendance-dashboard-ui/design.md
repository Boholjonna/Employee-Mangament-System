# Design Document

## Overview

The Attendance Dashboard UI is a new WPF `Window` (`AttendanceDashboard`) added to the SOFTDEV project. It follows the same structural and visual conventions as `AdminEmployeesView` and `AdminOverviewUI`: a maximized window with a dark-purple theme, a top navigation bar, and a large rounded card as the main content container. The view is fully data-bound to an `AttendanceViewModel` and loads live records from the MySQL `attendance` table via `DatabaseHelper`.

---

## Architecture

### Component Overview

```
AttendanceDashboard (Window)
├── AttendanceDashboard.xaml          — XAML layout
├── AttendanceDashboard.xaml.cs       — Code-behind (navigation, popup toggle)
├── AttendanceViewModel.cs            — INotifyPropertyChanged ViewModel
├── AttendanceRecord_Model.cs         — Model class (maps to attendance table row)
└── DatabaseHelper.cs (extended)      — New GetAllAttendanceRecords() method
```

### Data Flow

```
DatabaseHelper.GetAllAttendanceRecords()
        │
        ▼
AttendanceViewModel (constructor)
  ├── _allRecords  ← full unfiltered list (private backing store)
  └── AttendanceRecords (ObservableCollection) ← bound to ItemsControl
        │
        ▼
AttendanceDashboard.xaml
  └── ItemsControl (DataGrid_ItemsControl)
        └── DataTemplate → tile row per AttendanceRecord_Model
```

### Filter / Sort Flow

```
Admin clicks FILTER / SORT button
        │
        ▼
Code-behind toggles FilterPopup / SortPopup Visibility
        │
Admin sets criteria and clicks Apply
        │
        ▼
AttendanceViewModel.ApplyFilter() / ApplySort()
  ├── Filters/sorts _allRecords in-memory
  └── Replaces AttendanceRecords contents (Clear + AddRange)
        │
        ▼
UI updates automatically via ObservableCollection change notifications
```

---

## File Structure

| File | Location | Purpose |
|---|---|---|
| `AttendanceDashboard.xaml` | `SOFTDEV/` | Window XAML layout |
| `AttendanceDashboard.xaml.cs` | `SOFTDEV/` | Code-behind |
| `AttendanceViewModel.cs` | `SOFTDEV/` | ViewModel |
| `AttendanceRecord_Model.cs` | `SOFTDEV/Models/` | Data model |
| `DatabaseHelper.cs` (extended) | `SOFTDEV/` | Add `GetAllAttendanceRecords()` |

---

## Data Models

### AttendanceRecord_Model

Located in `SOFTDEV/Models/AttendanceRecord_Model.cs`, alongside the existing `EmployeeModels.cs`.

```csharp
namespace SOFTDEV
{
    /// <summary>
    /// Represents one row from the attendance table.
    /// StatusColor is a hex string resolved from the Status value.
    /// </summary>
    public class AttendanceRecord_Model
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Date         { get; set; } = string.Empty;
        public string TimeIn       { get; set; } = string.Empty;
        public string TimeOut      { get; set; } = string.Empty;
        public string TotalHours   { get; set; } = string.Empty;
        public string Status       { get; set; } = string.Empty;

        /// <summary>
        /// Hex color code for the StatusBadge background.
        /// Resolved by the ViewModel based on Status value.
        /// Present → #2ecc71, Late → #f39c12, Absent → #e74c3c, On Leave → #3498db
        /// </summary>
        public string StatusColor  { get; set; } = "#7b61ff";
    }
}
```

### Status Color Mapping

| Status | Hex Color | Appearance |
|---|---|---|
| Present | `#2ecc71` | Green |
| Late | `#f39c12` | Amber |
| Absent | `#e74c3c` | Red |
| On Leave | `#3498db` | Blue |
| (default) | `#7b61ff` | Purple |

---

## ViewModel Design

### AttendanceViewModel

```csharp
public class AttendanceViewModel : INotifyPropertyChanged
{
    // Backing store — never modified after initial load
    private List<AttendanceRecord_Model> _allRecords = new();

    // Active filter state
    private string _filterEmployee = string.Empty;
    private string _filterStatus   = "All";
    private string _filterDateFrom = string.Empty;
    private string _filterDateTo   = string.Empty;

    // Active sort state
    private string _sortColumn    = "Date";
    private bool   _sortAscending = true;

    // Bound properties
    public ObservableCollection<AttendanceRecord_Model> AttendanceRecords { get; } = new();
    public string AdminGreeting { get; private set; } = "Hello Admin! 👋";

    // Constructor
    public AttendanceViewModel(string adminName)
    {
        AdminGreeting = string.IsNullOrWhiteSpace(adminName)
            ? "Hello Admin! 👋"
            : $"Hello {adminName}! 👋";
        LoadRecords();
    }

    // Loads from DB, populates _allRecords and AttendanceRecords
    private void LoadRecords() { ... }

    // Applies current filter criteria to _allRecords → AttendanceRecords
    public void ApplyFilter(string employee, string status, string dateFrom, string dateTo) { ... }

    // Applies sort to current AttendanceRecords contents
    public void ApplySort(string column, bool ascending) { ... }

    // Resets filter → restores full _allRecords into AttendanceRecords
    public void ResetFilter() { ... }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

**Key design decisions:**
- `_allRecords` is the immutable source of truth loaded once at startup. Filter/sort operations never mutate it — they only replace the contents of `AttendanceRecords`.
- `AttendanceRecords` is cleared and repopulated (not replaced) so the `ItemsControl` binding stays live.
- `AdminGreeting` is set once in the constructor; no setter needed.

---

## DatabaseHelper Extension

Add the following method to the existing `DatabaseHelper.cs`:

```csharp
/// <summary>
/// Returns all rows from the attendance table joined to the employee table
/// for the employee name. Returns an empty list on any MySqlException.
/// </summary>
public static List<AttendanceRecord_Model> GetAllAttendanceRecords()
{
    var list = new List<AttendanceRecord_Model>();
    try
    {
        using var conn = GetConnection();

        const string sql =
            "SELECT e.name, a.date, a.time_in, a.time_out, a.total_hours, a.status " +
            "FROM attendance a " +
            "LEFT JOIN employee e ON a.employee_id = e.id " +
            "ORDER BY a.date DESC, e.name ASC";

        using var cmd    = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string status = reader.IsDBNull(5) ? "" : reader.GetString(5);
            list.Add(new AttendanceRecord_Model
            {
                EmployeeName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                Date         = reader.IsDBNull(1) ? "" : reader.GetString(1),
                TimeIn       = reader.IsDBNull(2) ? "" : reader.GetString(2),
                TimeOut      = reader.IsDBNull(3) ? "" : reader.GetString(3),
                TotalHours   = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Status       = status,
                StatusColor  = ResolveStatusColor(status),
            });
        }
    }
    catch (MySqlException ex)
    {
        System.Diagnostics.Debug.WriteLine($"[DB] GetAllAttendanceRecords error: {ex.Message}");
    }
    return list;
}

private static string ResolveStatusColor(string status) => status.ToLower() switch
{
    "present"  => "#2ecc71",
    "late"     => "#f39c12",
    "absent"   => "#e74c3c",
    "on leave" => "#3498db",
    _          => "#7b61ff",
};
```

**Note:** The SQL uses a `LEFT JOIN` so attendance rows with no matching employee still appear (employee name will be empty string). Parameterized queries are used for all filter methods added to the ViewModel.

---

## XAML Layout Design

### Window Structure

```
Window (AttendanceDashboard)
│  Background = DarkBackgroundBrush (#0a0a0a)
│  WindowState = Maximized, MinWidth=1200, MinHeight=700
│
└── Grid (2 rows: Auto, *)
    │
    ├── Row 0: StackPanel (header section)
    │   ├── DockPanel (top bar)
    │   │   ├── Left: Back Button + "COMPANY NAME/LOGO" TextBlock
    │   │   └── Right: Search, Notification, Username, Avatar buttons
    │   └── UniformGrid Rows=1 (nav bar — 7 buttons)
    │       Overview | Employees | Attendance* | Task Management | Reports | Leaves | Settings
    │       (* = active: Opacity=1.0, Background=PurpleAccentBrush)
    │
    └── Row 1: Grid (Margin="20,0,20,20")
        │
        ├── Greeting TextBlock (above card, outside Border)
        │   "Hello [AdminName]! 👋"  Foreground=White, FontSize=22
        │
        └── Border (main card)
            CornerRadius=30, Background=#1a1a2e, Padding=24
            │
            └── Grid (2 rows: Auto, *)
                │
                ├── Row 0: Content Header
                │   ├── Left: "ATTENDANCE" TextBlock (Bold, White, FontSize=20)
                │   └── Right: FILTER button + SORT button (PillButtonStyle)
                │
                └── Row 1: Data Area
                    │
                    ├── Header Row Border (Background=#2a2a3e, CornerRadius=8)
                    │   └── Grid (6 columns)
                    │       EMPLOYEE | DATE | TIME IN | TIME OUT | TOTAL HOURS | STATUS
                    │       (each: Bold, Center, Foreground=PurpleAccentBrush)
                    │
                    └── ScrollViewer (VerticalScrollBarVisibility=Auto)
                        ScrollBar styled with CustomScrollbarStyle
                        │
                        └── ItemsControl (bound to AttendanceRecords)
                            DataTemplate → Grid (6 columns, same proportions as header)
                            Each cell: Border CornerRadius=8, Background=#2a2a3e, Margin=4,2
                            STATUS cell: Border CornerRadius=12, Background={Binding StatusColor}
                            Empty state: TextBlock "No attendance records found." Foreground=#aaaaaa
```

### Column Proportions

The same `ColumnDefinition` set is used in both the header row and the `DataTemplate` row to guarantee alignment:

| Column | Width | Content |
|---|---|---|
| EMPLOYEE | `2*` | Employee name |
| DATE | `1.5*` | Date string |
| TIME IN | `1*` | Time-in string |
| TIME OUT | `1*` | Time-out string |
| TOTAL HOURS | `1*` | Total hours string |
| STATUS | `1.2*` | StatusBadge pill |

### Filter Popup Layout

The `FilterPopup` is a `Border` with `Visibility="Collapsed"` positioned absolutely (using `Canvas` or `Grid` overlay) over the data area. It is toggled by the FILTER button click handler in code-behind.

```
FilterPopup Border
  Background=#1a1a2e, CornerRadius=16, Padding=20
  Width=320, HorizontalAlignment=Right
  │
  └── StackPanel
      ├── "Filter Records" TextBlock (header)
      ├── "From Date" Label + DatePicker (x:Name="FilterFromDate")
      ├── "To Date" Label + DatePicker (x:Name="FilterToDate")
      ├── "Employee" Label + TextBox (x:Name="FilterEmployeeText", RoundedTextBoxStyle)
      ├── "Status" Label + ComboBox (x:Name="FilterStatusCombo")
      │   Items: All, Present, Late, Absent, On Leave
      └── StackPanel (Horizontal)
          ├── "Apply" Button (PillButtonStyle, Click=ApplyFilter_Click)
          └── "Reset" Button (OutlinePillButtonStyle, Click=ResetFilter_Click)
```

### Sort Popup Layout

```
SortPopup Border
  Background=#1a1a2e, CornerRadius=16, Padding=20
  Width=260, HorizontalAlignment=Right
  │
  └── StackPanel
      ├── "Sort By" TextBlock (header)
      ├── ComboBox (x:Name="SortColumnCombo")
      │   Items: Employee Name, Date, Time In, Time Out, Total Hours, Status
      ├── "Direction" TextBlock
      ├── StackPanel (Horizontal)
      │   ├── RadioButton "Ascending" (x:Name="SortAscending", IsChecked=True)
      │   └── RadioButton "Descending" (x:Name="SortDescending")
      └── "Apply Sort" Button (PillButtonStyle, Click=ApplySort_Click)
```

---

## Code-Behind Design

### AttendanceDashboard.xaml.cs

```csharp
public partial class AttendanceDashboard : Window
{
    private readonly string _username;
    private readonly AttendanceViewModel _viewModel;

    public AttendanceDashboard(string username)
    {
        InitializeComponent();
        _username  = username;
        _viewModel = new AttendanceViewModel(username);
        DataContext = _viewModel;
    }

    // Navigation — same pattern as AdminEmployeesView.xaml.cs
    private void NavButton_Click(object sender, RoutedEventArgs e) { ... }
    private void BackButton_Click(object sender, RoutedEventArgs e) { ... }

    // Popup toggles
    private void FilterButton_Click(object sender, RoutedEventArgs e)
        => FilterPopup.Visibility = FilterPopup.Visibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;

    private void SortButton_Click(object sender, RoutedEventArgs e)
        => SortPopup.Visibility = SortPopup.Visibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;

    // Filter apply / reset
    private void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ApplyFilter(
            FilterEmployeeText.Text,
            (FilterStatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All",
            FilterFromDate.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            FilterToDate.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty);
        FilterPopup.Visibility = Visibility.Collapsed;
    }

    private void ResetFilter_Click(object sender, RoutedEventArgs e)
    {
        FilterEmployeeText.Text = string.Empty;
        FilterStatusCombo.SelectedIndex = 0;
        FilterFromDate.SelectedDate = null;
        FilterToDate.SelectedDate   = null;
        _viewModel.ResetFilter();
        FilterPopup.Visibility = Visibility.Collapsed;
    }

    // Sort apply
    private void ApplySort_Click(object sender, RoutedEventArgs e)
    {
        string column    = (SortColumnCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Date";
        bool   ascending = SortAscending.IsChecked == true;
        _viewModel.ApplySort(column, ascending);
        SortPopup.Visibility = Visibility.Collapsed;
    }

    // Header control group stubs
    private void SearchButton_Click(object sender, RoutedEventArgs e) { ... }
    private void NotificationButton_Click(object sender, RoutedEventArgs e) { ... }
    private void UserNameButton_Click(object sender, RoutedEventArgs e) { ... }
    private void AvatarButton_Click(object sender, RoutedEventArgs e) { ... }
}
```

### Navigation Integration

Each existing admin view's `NavButton_Click` handler is extended to handle the Attendance button:

```csharp
// In AdminDashboard.xaml.cs, AdminEmployeesView.xaml.cs, AdminOverviewUI.xaml.cs
if (sender == AttendanceButton)
{
    var attendanceDashboard = new AttendanceDashboard(_username);
    this.Hide();
    attendanceDashboard.Show();
}
```

The `AttendanceDashboard` constructor accepts `string username` — consistent with how `AdminEmployeesView` and `AdminOverviewUI` are constructed.

---

## Correctness Properties

The following properties define the formal correctness of the Attendance Dashboard and will be validated through property-based testing:

### P1 — Filter Completeness
For any filter criteria `(employee, status, dateFrom, dateTo)`, every record in `AttendanceRecords` after `ApplyFilter()` must satisfy all non-empty criteria simultaneously. No record that fails any active criterion may appear in the result.

```
∀ r ∈ AttendanceRecords after ApplyFilter(e, s, df, dt):
  (e == "" ∨ r.EmployeeName.Contains(e, OrdinalIgnoreCase)) ∧
  (s == "All" ∨ r.Status == s) ∧
  (df == "" ∨ r.Date >= df) ∧
  (dt == "" ∨ r.Date <= dt)
```

### P2 — Filter Soundness
No record from `_allRecords` that satisfies all active filter criteria is omitted from `AttendanceRecords` after `ApplyFilter()`.

```
∀ r ∈ _allRecords that satisfies all criteria → r ∈ AttendanceRecords
```

### P3 — Reset Idempotency
After `ResetFilter()`, `AttendanceRecords` contains exactly the same records as `_allRecords` (same count, same content).

```
ResetFilter() → AttendanceRecords.Count == _allRecords.Count
```

### P4 — Sort Order Preservation
After `ApplySort(column, ascending)`, consecutive pairs in `AttendanceRecords` satisfy the sort predicate. The sort is stable with respect to the secondary natural order.

```
∀ i ∈ [0, AttendanceRecords.Count-2]:
  ascending  → Compare(AttendanceRecords[i][column], AttendanceRecords[i+1][column]) ≤ 0
  descending → Compare(AttendanceRecords[i][column], AttendanceRecords[i+1][column]) ≥ 0
```

### P5 — StatusColor Consistency
For every record in `AttendanceRecords`, `StatusColor` is the canonical hex value for that `Status` string. No record has a `StatusColor` that contradicts its `Status`.

```
∀ r ∈ AttendanceRecords:
  r.StatusColor == ResolveStatusColor(r.Status)
```

### P6 — Empty State Visibility
The empty-state `TextBlock` is visible if and only if `AttendanceRecords.Count == 0`.

```
EmptyStateTextBlock.Visibility == Visible ↔ AttendanceRecords.Count == 0
```

---

## Styling Reference

All styles are consumed from the existing `App.xaml` resources. No new global styles are added.

| Resource Key | Type | Usage |
|---|---|---|
| `DarkBackgroundBrush` | `SolidColorBrush` (#0a0a0a) | Window background |
| `PurpleAccentBrush` | `SolidColorBrush` (#7b61ff) | Active nav button, header text, scrollbar thumb |
| `CardBackgroundBrush` | `SolidColorBrush` (#15151b) | (available; main card uses inline #1a1a2e) |
| `PillButtonStyle` | `Style<Button>` | All nav, FILTER, SORT, Apply buttons |
| `OutlinePillButtonStyle` | `Style<Button>` | Reset button in FilterPopup |
| `CircleButtonStyle` | `Style<Button>` | Back, Search, Notification, Avatar buttons |
| `CustomScrollbarStyle` | `Style<ScrollBar>` | Data area ScrollViewer scrollbar |
| `RoundedTextBoxStyle` | `Style<TextBox>` | Employee filter text input |

Inline color values used in the XAML (not in App.xaml):

| Value | Usage |
|---|---|
| `#1a1a2e` | Main card background |
| `#2a2a3e` | Header row background, data cell background |
| `#aaaaaa` | Empty-state text, secondary labels |
