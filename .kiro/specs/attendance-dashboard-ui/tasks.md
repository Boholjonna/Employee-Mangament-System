# Implementation Plan: Attendance Dashboard UI

## Overview

Implement the `AttendanceDashboard` WPF Window following the `AdminEmployeesView` structural pattern. The work proceeds in five incremental steps: data layer first, then ViewModel, then XAML layout, then code-behind wiring, and finally navigation integration across the three existing admin views.

## Tasks

- [x] 1. Create the `AttendanceRecord_Model` data model
  - Create `SOFTDEV/Models/AttendanceRecord_Model.cs` in the `SOFTDEV` namespace alongside `EmployeeModels.cs`
  - Define the seven properties: `EmployeeName`, `Date`, `TimeIn`, `TimeOut`, `TotalHours`, `Status`, `StatusColor` (all `string`)
  - Add XML doc comments matching the design specification
  - _Requirements: 9.1_

- [x] 2. Extend `DatabaseHelper` with attendance query methods
  - [x] 2.1 Add `ResolveStatusColor(string status)` private static helper to `DatabaseHelper.cs`
    - Implement the switch expression mapping: `present → #2ecc71`, `late → #f39c12`, `absent → #e74c3c`, `on leave → #3498db`, default `→ #7b61ff`
    - _Requirements: 5.5, 10.1_

  - [x] 2.2 Write property test for `ResolveStatusColor`
    - **Property 5: StatusColor consistency** — for every `AttendanceRecord_Model` produced by `GetAllAttendanceRecords`, `r.StatusColor == ResolveStatusColor(r.Status)`
    - **Validates: Requirements 5.5**

  - [x] 2.3 Add `GetAllAttendanceRecords()` public static method to `DatabaseHelper.cs`
    - Execute the LEFT JOIN SQL: `SELECT e.name, a.date, a.time_in, a.time_out, a.total_hours, a.status FROM attendance a LEFT JOIN employee e ON a.employee_id = e.id ORDER BY a.date DESC, e.name ASC`
    - Map each reader row to `AttendanceRecord_Model`, calling `ResolveStatusColor` for `StatusColor`
    - Catch `MySqlException`, log via `Debug.WriteLine`, and return an empty list on failure
    - _Requirements: 10.1, 10.2, 10.3, 10.4_

- [x] 3. Implement `AttendanceViewModel`
  - [x] 3.1 Create `SOFTDEV/AttendanceViewModel.cs` implementing `INotifyPropertyChanged`
    - Declare `private List<AttendanceRecord_Model> _allRecords` as the immutable backing store
    - Declare `public ObservableCollection<AttendanceRecord_Model> AttendanceRecords` for UI binding
    - Declare `public string AdminGreeting` with a private setter
    - Implement the constructor `AttendanceViewModel(string adminName)`: set `AdminGreeting`, call `LoadRecords()`
    - Implement `LoadRecords()`: call `DatabaseHelper.GetAllAttendanceRecords()`, populate `_allRecords` and `AttendanceRecords`; on exception set `AttendanceRecords` to empty and log
    - _Requirements: 9.2, 9.3, 9.4, 9.5, 9.6, 3.1, 3.4_

  - [x] 3.2 Implement `ApplyFilter` on `AttendanceViewModel`
    - Signature: `public void ApplyFilter(string employee, string status, string dateFrom, string dateTo)`
    - Filter `_allRecords` in-memory: employee name contains (OrdinalIgnoreCase) when non-empty; status equals when not "All"; date >= dateFrom when non-empty; date <= dateTo when non-empty
    - Clear and repopulate `AttendanceRecords` (do not replace the collection reference)
    - _Requirements: 7.2, 7.3, 7.4_

  - [x] 3.3 Write property test for `ApplyFilter` — Filter Completeness (P1)
    - **Property 1: Filter completeness** — every record in `AttendanceRecords` after `ApplyFilter` satisfies all active criteria
    - **Validates: Requirements 7.2**

  - [x] 3.4 Write property test for `ApplyFilter` — Filter Soundness (P2)
    - **Property 2: Filter soundness** — no record from `_allRecords` that satisfies all active criteria is absent from `AttendanceRecords`
    - **Validates: Requirements 7.2, 7.3**

  - [x] 3.5 Implement `ResetFilter` on `AttendanceViewModel`
    - Clear and repopulate `AttendanceRecords` from `_allRecords` (full restore)
    - _Requirements: 7.3_

  - [x] 3.6 Write property test for `ResetFilter` — Reset Idempotency (P3)
    - **Property 3: Reset idempotency** — after `ResetFilter()`, `AttendanceRecords.Count == _allRecords.Count` and contents match
    - **Validates: Requirements 7.3**

  - [x] 3.7 Implement `ApplySort` on `AttendanceViewModel`
    - Signature: `public void ApplySort(string column, bool ascending)`
    - Sort the current `AttendanceRecords` contents in-place by the selected column (Employee Name, Date, Time In, Time Out, Total Hours, Status) and direction
    - Clear and repopulate `AttendanceRecords` with the sorted result
    - _Requirements: 8.2_

  - [x] 3.8 Write property test for `ApplySort` — Sort Order (P4)
    - **Property 4: Sort order** — for every consecutive pair `(i, i+1)` in `AttendanceRecords` after `ApplySort`, the comparison satisfies the sort predicate (≤ 0 ascending, ≥ 0 descending)
    - **Validates: Requirements 8.2**

- [x] 4. Checkpoint — ViewModel complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Create `AttendanceDashboard.xaml` layout
  - [x] 5.1 Create `SOFTDEV/AttendanceDashboard.xaml` as a WPF `Window` with `x:Class="SOFTDEV.AttendanceDashboard"`
    - Set `Background="{StaticResource DarkBackgroundBrush}"`, `WindowState="Maximized"`, `MinWidth="1200"`, `MinHeight="700"`, `WindowStartupLocation="CenterScreen"`, `FontFamily="Segoe UI"`
    - _Requirements: 1.1, 1.2, 1.3, 1.4_

  - [x] 5.2 Add the header section (DockPanel top bar + UniformGrid nav bar)
    - DockPanel: left side has Back button (`CircleButtonStyle`, `x:Name="BackButton"`) + "COMPANY NAME/LOGO" TextBlock; right side has Search, Notification, Username, Avatar buttons
    - UniformGrid `Rows="1"`: seven nav buttons (Overview, Employees, Attendance, Task Management, Reports, Leaves, Settings) all styled `PillButtonStyle`
    - Attendance button: `Background="{StaticResource PurpleAccentBrush}"`, `Opacity="1.0"` (active tab)
    - All inactive buttons: `Opacity="0.4"` with `MouseEnter`/`MouseLeave` `DoubleAnimation` triggers (To=1.0 / To=0.4, Duration=0:0:0.15) matching `AdminEmployeesView.xaml` pattern
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

  - [x] 5.3 Add the greeting TextBlock and main content card
    - Greeting `TextBlock` bound to `{Binding AdminGreeting}`, `Foreground="White"`, `FontSize="22"`, placed above the card
    - Main `Border`: `CornerRadius="30"`, `Background="#1a1a2e"`, `Padding="24"`
    - Content header row inside the card: left `TextBlock` "ATTENDANCE" (`FontWeight="Bold"`, `Foreground="White"`, `FontSize="20"`); right `StackPanel` with FILTER button (`x:Name="FilterButton"`) and SORT button (`x:Name="SortButton"`) both `PillButtonStyle`
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [x] 5.4 Add the column header row
    - `Border` with `Background="#2a2a3e"`, `CornerRadius="8"` containing a `Grid` with six `ColumnDefinition`s: `2*`, `1.5*`, `1*`, `1*`, `1*`, `1.2*`
    - Six `TextBlock` headers: EMPLOYEE, DATE, TIME IN, TIME OUT, TOTAL HOURS, STATUS — each `FontWeight="Bold"`, `HorizontalAlignment="Center"`, `Foreground="{StaticResource PurpleAccentBrush}"`
    - _Requirements: 4.1, 4.2, 4.3, 4.4_

  - [x] 5.5 Add the scrollable data area with `ItemsControl` and empty state
    - `ScrollViewer` with `VerticalScrollBarVisibility="Auto"`, `HorizontalScrollBarVisibility="Disabled"`, scrollbar styled with `CustomScrollbarStyle`
    - `ItemsControl` (`x:Name="DataGrid_ItemsControl"`) bound to `{Binding AttendanceRecords}`
    - `DataTemplate`: `Grid` with the same six `ColumnDefinition` proportions (`2*`, `1.5*`, `1*`, `1*`, `1*`, `1.2*`); each of the first five cells is a `Border` (`CornerRadius="8"`, `Background="#2a2a3e"`, `Margin="4,2"`) containing a centered `TextBlock` bound to the respective property
    - STATUS cell: `Border` `CornerRadius="12"`, `Padding="10,4"`, `Background="{Binding StatusColor}"` containing `TextBlock` bound to `{Binding Status}`
    - Empty-state `TextBlock` "No attendance records found." `Foreground="#aaaaaa"`, centered, visible only when `AttendanceRecords.Count == 0` (use a `DataTrigger` or `BooleanToVisibilityConverter`)
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 6.1, 6.2, 6.3_

  - [x] 5.6 Add the FilterPopup overlay
    - `Border` (`x:Name="FilterPopup"`, `Visibility="Collapsed"`) positioned as a `Grid` overlay (absolute, `HorizontalAlignment="Right"`, `VerticalAlignment="Top"`)
    - `Background="#1a1a2e"`, `CornerRadius="16"`, `Padding="20"`, `Width="320"`
    - Contents: "Filter Records" header TextBlock; "From Date" label + `DatePicker` (`x:Name="FilterFromDate"`); "To Date" label + `DatePicker` (`x:Name="FilterToDate"`); "Employee" label + `TextBox` (`x:Name="FilterEmployeeText"`, `RoundedTextBoxStyle`); "Status" label + `ComboBox` (`x:Name="FilterStatusCombo"`) with items: All, Present, Late, Absent, On Leave; horizontal `StackPanel` with "Apply" button (`Click="ApplyFilter_Click"`, `PillButtonStyle`) and "Reset" button (`Click="ResetFilter_Click"`, `OutlinePillButtonStyle`)
    - _Requirements: 7.1_

  - [x] 5.7 Add the SortPopup overlay
    - `Border` (`x:Name="SortPopup"`, `Visibility="Collapsed"`) positioned as a `Grid` overlay (`HorizontalAlignment="Right"`, `VerticalAlignment="Top"`)
    - `Background="#1a1a2e"`, `CornerRadius="16"`, `Padding="20"`, `Width="260"`
    - Contents: "Sort By" header TextBlock; `ComboBox` (`x:Name="SortColumnCombo"`) with items: Employee Name, Date, Time In, Time Out, Total Hours, Status; "Direction" TextBlock; horizontal `StackPanel` with `RadioButton` "Ascending" (`x:Name="SortAscending"`, `IsChecked="True"`) and `RadioButton` "Descending" (`x:Name="SortDescending"`); "Apply Sort" button (`Click="ApplySort_Click"`, `PillButtonStyle`)
    - _Requirements: 8.1, 8.3_

- [ ] 6. Create `AttendanceDashboard.xaml.cs` code-behind
  - [x] 6.1 Create `SOFTDEV/AttendanceDashboard.xaml.cs` with the `AttendanceDashboard` partial class
    - Constructor `AttendanceDashboard(string username)`: call `InitializeComponent()`, instantiate `AttendanceViewModel(username)`, set `DataContext`
    - Implement `BackButton_Click`: show owner window (if any) and close this window
    - Implement stub handlers for `SearchButton_Click`, `NotificationButton_Click`, `UserNameButton_Click`, `AvatarButton_Click` (log via `Debug.WriteLine`)
    - _Requirements: 11.4, 2.7_

  - [x] 6.2 Implement popup toggle handlers
    - `FilterButton_Click`: toggle `FilterPopup.Visibility` between `Visible` and `Collapsed`
    - `SortButton_Click`: toggle `SortPopup.Visibility` between `Visible` and `Collapsed`
    - _Requirements: 7.1, 8.1_

  - [x] 6.3 Implement `ApplyFilter_Click` and `ResetFilter_Click`
    - `ApplyFilter_Click`: read `FilterEmployeeText.Text`, `FilterStatusCombo` selected item content, `FilterFromDate.SelectedDate` (formatted `yyyy-MM-dd`), `FilterToDate.SelectedDate`; call `_viewModel.ApplyFilter(...)`; collapse `FilterPopup`
    - `ResetFilter_Click`: clear all filter controls; call `_viewModel.ResetFilter()`; collapse `FilterPopup`
    - _Requirements: 7.2, 7.3_

  - [x] 6.4 Implement `ApplySort_Click`
    - Read `SortColumnCombo` selected item content and `SortAscending.IsChecked`; call `_viewModel.ApplySort(column, ascending)`; collapse `SortPopup`
    - _Requirements: 8.2_

  - [x] 6.5 Implement `NavButton_Click` for all seven nav buttons
    - Match the `AdminEmployeesView` pattern: open the target window with `_username`, call `this.Hide()`, call `targetWindow.Show()`
    - Attendance button click is a no-op (already on this view)
    - _Requirements: 2.7_

  - [x] 6.6 Write property test for empty-state visibility (P6)
    - **Property 6: Empty state** — `EmptyStateTextBlock.Visibility == Visible` iff `AttendanceRecords.Count == 0`; verify by loading the ViewModel with zero records and with non-zero records
    - **Validates: Requirements 5.6**

- [x] 7. Checkpoint — AttendanceDashboard complete
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Wire Attendance navigation in existing admin views
  - [x] 8.1 Update `AdminDashboard.xaml.cs` — `NavButton_Click`
    - Add `else if (sender == AttendanceButton)` branch: instantiate `new AttendanceDashboard(_username)`, call `this.Hide()`, call `attendanceDashboard.Show()`
    - _Requirements: 11.1_

  - [x] 8.2 Update `AdminEmployeesView.xaml.cs` — `NavButton_Click`
    - Add `else if (sender == AttendanceButton)` branch: instantiate `new AttendanceDashboard(_username)`, call `this.Hide()`, call `attendanceDashboard.Show()`
    - _Requirements: 11.2_

  - [ ] 8.3 Update `AdminOverviewUI.xaml.cs` — add `NavButton_Click` handler (or extend existing)
    - Add `AttendanceButton` click handling: instantiate `new AttendanceDashboard(_username)`, call `this.Hide()`, call `attendanceDashboard.Show()`
    - _Requirements: 11.3_

- [ ] 9. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- `_allRecords` is never mutated after `LoadRecords()` — all filter/sort operations only replace the contents of `AttendanceRecords`
- The same six `ColumnDefinition` proportions (`2*` / `1.5*` / `1*` / `1*` / `1*` / `1.2*`) must appear in both the header row Grid and the DataTemplate Grid to guarantee column alignment
- Property tests (P1–P6) can be written using any .NET property-based testing library (e.g., FsCheck, CsCheck) and do not require a live database — seed `_allRecords` directly via reflection or a test constructor overload
- Navigation follows the `Hide()/Show()` pattern established in `AdminEmployeesView.xaml.cs`; `AttendanceDashboard` does not need an `_ownerDashboard` reference unless back-navigation to a specific window is required
