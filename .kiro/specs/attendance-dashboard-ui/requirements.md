# Requirements Document

## Introduction

The Attendance Dashboard UI is a new admin-facing view in the existing WPF C# application (SOFTDEV project). It provides administrators with a full-screen, data-bound attendance management interface that matches the application's established dark-purple design language. The view displays all employee attendance records in a custom tile-based grid, supports filtering and sorting, and integrates with the existing MySQL database via `DatabaseHelper`. It is navigated to from the shared navigation bar present on all admin views (AdminDashboard, AdminEmployeesView, AdminOverviewUI).

## Glossary

- **AttendanceDashboard**: The new WPF `Window` (or `UserControl`) that renders the attendance management interface.
- **AttendanceRecord**: A single row of attendance data containing employee name, date, time-in, time-out, total hours, and status.
- **AttendanceViewModel**: The C# class that exposes observable collections and commands for data binding to the AttendanceDashboard.
- **AttendanceRecord_Model**: The C# model class representing one attendance record (maps to the `attendance` database table).
- **StatusBadge**: A pill-shaped UI element inside each data row that displays the attendance status (e.g., Present, Late, Absent, On Leave) with a corresponding color.
- **NavBar**: The `UniformGrid` navigation bar shared across all admin views, containing buttons for Overview, Employees, Attendance, Task Management, Reports, Leaves, and Settings.
- **FilterPopup**: A dropdown or popup panel that allows the admin to filter attendance records by date range, employee, or status.
- **SortPopup**: A dropdown or popup panel that allows the admin to sort attendance records by any column.
- **DataGrid_ItemsControl**: The `ItemsControl` with a custom `DataTemplate` used to render attendance rows as modular tiles instead of a standard `DataGrid`.
- **PillButtonStyle**: The existing `Style` defined in `App.xaml` applied to all navigation and action buttons.
- **CustomScrollbarStyle**: The existing `Style` defined in `App.xaml` applied to all `ScrollBar` elements.
- **DarkBackgroundBrush**: The existing `SolidColorBrush` resource (`#0a0a0a`) used as the window background.
- **CardBackgroundBrush**: The existing `SolidColorBrush` resource (`#15151b`) used as card/container backgrounds.
- **PurpleAccentBrush**: The existing `SolidColorBrush` resource (`#7b61ff`) used for active states, headers, and accents.

---

## Requirements

### Requirement 1: Window Layout and Visual Theme

**User Story:** As an admin, I want the Attendance Dashboard to match the visual style of the other admin views, so that the application feels consistent and professional.

#### Acceptance Criteria

1. THE AttendanceDashboard SHALL use `Background="{StaticResource DarkBackgroundBrush}"` as the window background color.
2. THE AttendanceDashboard SHALL contain a main content `Border` with `CornerRadius="30"` and `Background="#1a1a2e"` as the primary card container.
3. THE AttendanceDashboard SHALL set `WindowState="Maximized"`, `MinWidth="1200"`, `MinHeight="700"`, and `WindowStartupLocation="CenterScreen"` to match existing admin windows.
4. THE AttendanceDashboard SHALL use `FontFamily="Segoe UI"` as the default font, consistent with other admin views.
5. THE AttendanceDashboard SHALL apply `{StaticResource CustomScrollbarStyle}` to all `ScrollBar` elements within the view.

---

### Requirement 2: Navigation Header

**User Story:** As an admin, I want a consistent navigation bar at the top of the Attendance Dashboard, so that I can switch between admin sections without returning to the main dashboard.

#### Acceptance Criteria

1. THE AttendanceDashboard SHALL render a `DockPanel` top navigation bar containing a company logo/name `TextBlock` on the left and Search, Notification, Username, and Avatar buttons on the right, matching the layout in `AdminEmployeesView.xaml`.
2. THE AttendanceDashboard SHALL render a `UniformGrid` with `Rows="1"` containing seven navigation buttons: Overview, Employees, Attendance, Task Management, Reports, Leaves, and Settings.
3. THE AttendanceDashboard SHALL apply `{StaticResource PillButtonStyle}` to all seven navigation buttons.
4. WHEN the AttendanceDashboard is displayed, THE AttendanceDashboard SHALL render the "Attendance" navigation button with `Background="{StaticResource PurpleAccentBrush}"` and `Opacity="1.0"` to indicate the active tab.
5. WHEN any inactive navigation button receives a `MouseEnter` event, THE AttendanceDashboard SHALL animate that button's `Opacity` from `0.4` to `1.0` over `0:0:0.15` using a `DoubleAnimation`.
6. WHEN any inactive navigation button receives a `MouseLeave` event, THE AttendanceDashboard SHALL animate that button's `Opacity` from `1.0` to `0.4` over `0:0:0.15` using a `DoubleAnimation`.
7. WHEN a navigation button is clicked, THE AttendanceDashboard SHALL open the corresponding admin view and close the current window, consistent with navigation behavior in `AdminEmployeesView.xaml.cs`.

---

### Requirement 3: Content Header

**User Story:** As an admin, I want a clear header inside the main content area showing the section title and action buttons, so that I can quickly identify the current view and access filtering/sorting controls.

#### Acceptance Criteria

1. THE AttendanceDashboard SHALL display a greeting `TextBlock` (e.g., `"Hello [AdminName]! 👋"`) above the main container, bound to the logged-in admin's name, using `Foreground="White"` and `FontSize="22"`.
2. THE AttendanceDashboard SHALL display a `TextBlock` with the text `"ATTENDANCE"` in `FontWeight="Bold"` and `Foreground="White"` on the left side of the content header row inside the main container.
3. THE AttendanceDashboard SHALL display a `"FILTER"` button and a `"SORT"` button on the right side of the content header row, both styled with `{StaticResource PillButtonStyle}`.
4. WHEN the admin name is not available, THE AttendanceDashboard SHALL display `"Hello Admin! 👋"` as the default greeting text.

---

### Requirement 4: Attendance Data Grid — Header Row

**User Story:** As an admin, I want a clearly labeled header row above the attendance data, so that I can identify what each column represents.

#### Acceptance Criteria

1. THE AttendanceDashboard SHALL render a header row `Grid` with six columns: EMPLOYEE, DATE, TIME IN, TIME OUT, TOTAL HOURS, and STATUS.
2. THE AttendanceDashboard SHALL apply a darker purple background (`#2a2a3e`) to the header row `Border`.
3. THE AttendanceDashboard SHALL render each column header label as a `TextBlock` with `FontWeight="Bold"`, `HorizontalAlignment="Center"`, and `Foreground="{StaticResource PurpleAccentBrush}"`.
4. THE AttendanceDashboard SHALL use the same six-column `ColumnDefinition` proportions in both the header row and each data row `DataTemplate` to ensure visual alignment.

---

### Requirement 5: Attendance Data Grid — Data Rows

**User Story:** As an admin, I want each attendance record displayed as a modular tile row with rounded cell blocks, so that the data is easy to read and visually distinct from a standard grid.

#### Acceptance Criteria

1. THE AttendanceDashboard SHALL use an `ItemsControl` bound to `AttendanceViewModel.AttendanceRecords` (an `ObservableCollection<AttendanceRecord_Model>`) to render data rows.
2. THE AttendanceDashboard SHALL define a `DataTemplate` for each row that uses a `Grid` with six columns matching the header layout.
3. THE AttendanceDashboard SHALL render each cell value inside a `Border` with `CornerRadius="8"`, `Background="#2a2a3e"`, and `Margin="4,2"` to achieve the modular tile appearance.
4. THE AttendanceDashboard SHALL bind the six cell values to `{Binding EmployeeName}`, `{Binding Date}`, `{Binding TimeIn}`, `{Binding TimeOut}`, `{Binding TotalHours}`, and `{Binding Status}` respectively.
5. THE AttendanceDashboard SHALL render the STATUS cell as a `StatusBadge`: a `Border` with `CornerRadius="12"`, `Padding="10,4"`, and `Background` bound to `{Binding StatusColor}`, containing a `TextBlock` bound to `{Binding Status}`.
6. WHILE the `AttendanceViewModel.AttendanceRecords` collection is empty, THE AttendanceDashboard SHALL display a centered `TextBlock` with the text `"No attendance records found."` and `Foreground="#aaaaaa"`.

---

### Requirement 6: Scrollable Data Area

**User Story:** As an admin, I want the attendance record list to be scrollable, so that I can view all records even when the list exceeds the visible area.

#### Acceptance Criteria

1. THE AttendanceDashboard SHALL wrap the `DataGrid_ItemsControl` in a `ScrollViewer` with `VerticalScrollBarVisibility="Auto"` and `HorizontalScrollBarVisibility="Disabled"`.
2. THE AttendanceDashboard SHALL apply `{StaticResource CustomScrollbarStyle}` to the `ScrollBar` inside the data area `ScrollViewer` so the scrollbar matches the dark theme.
3. THE AttendanceDashboard SHALL position the vertical scrollbar on the right side of the data area.

---

### Requirement 7: Filter Functionality

**User Story:** As an admin, I want to filter attendance records by date range, employee, or status, so that I can quickly find relevant records.

#### Acceptance Criteria

1. WHEN the admin clicks the `"FILTER"` button, THE AttendanceDashboard SHALL display a `FilterPopup` panel containing controls for: date range (start date, end date), employee name (text input or dropdown), and status (dropdown: All, Present, Late, Absent, On Leave).
2. WHEN the admin confirms a filter selection, THE AttendanceViewModel SHALL update `AttendanceRecords` to contain only records matching all active filter criteria.
3. WHEN all filter fields are cleared or reset, THE AttendanceViewModel SHALL restore `AttendanceRecords` to the full unfiltered dataset.
4. IF a filter produces zero matching records, THEN THE AttendanceDashboard SHALL display the empty-state `TextBlock` `"No attendance records found."`.

---

### Requirement 8: Sort Functionality

**User Story:** As an admin, I want to sort attendance records by any column, so that I can organize the data in a meaningful order.

#### Acceptance Criteria

1. WHEN the admin clicks the `"SORT"` button, THE AttendanceDashboard SHALL display a `SortPopup` panel with options to sort by: Employee Name, Date, Time In, Time Out, Total Hours, or Status; and direction: Ascending or Descending.
2. WHEN the admin selects a sort option, THE AttendanceViewModel SHALL reorder `AttendanceRecords` according to the selected column and direction.
3. WHEN a sort is applied, THE AttendanceDashboard SHALL visually indicate the active sort column and direction in the `SortPopup`.

---

### Requirement 9: Data Model and ViewModel

**User Story:** As a developer, I want a well-defined data model and ViewModel for the attendance dashboard, so that the UI is fully data-bound and testable.

#### Acceptance Criteria

1. THE AttendanceRecord_Model SHALL expose the following properties: `EmployeeName` (string), `Date` (string), `TimeIn` (string), `TimeOut` (string), `TotalHours` (string), `Status` (string), and `StatusColor` (string — hex color code).
2. THE AttendanceViewModel SHALL expose an `ObservableCollection<AttendanceRecord_Model>` named `AttendanceRecords` for binding to the `DataGrid_ItemsControl`.
3. THE AttendanceViewModel SHALL expose a `string` property `AdminGreeting` for binding to the greeting `TextBlock`.
4. THE AttendanceViewModel SHALL implement `INotifyPropertyChanged` so that all bound properties update the UI when changed.
5. WHEN `AttendanceViewModel` is initialized, THE AttendanceViewModel SHALL load attendance records from the database via `DatabaseHelper` and populate `AttendanceRecords`.
6. IF the database is unavailable during initialization, THEN THE AttendanceViewModel SHALL set `AttendanceRecords` to an empty collection and log the error via `System.Diagnostics.Debug.WriteLine`.

---

### Requirement 10: Database Integration

**User Story:** As a developer, I want the attendance dashboard to load real attendance data from the MySQL database, so that admins see accurate, live records.

#### Acceptance Criteria

1. THE DatabaseHelper SHALL expose a method `GetAllAttendanceRecords()` that returns a `List<AttendanceRecord_Model>` by querying the `attendance` table.
2. WHEN `GetAllAttendanceRecords()` is called, THE DatabaseHelper SHALL select the columns: employee name (or employee id joined to the employee table), date, time_in, time_out, total_hours, and status.
3. IF a `MySqlException` occurs in `GetAllAttendanceRecords()`, THEN THE DatabaseHelper SHALL log the error via `System.Diagnostics.Debug.WriteLine` and return an empty `List<AttendanceRecord_Model>`.
4. THE DatabaseHelper SHALL use parameterized queries in all new SQL methods to prevent SQL injection.

---

### Requirement 11: Navigation Integration

**User Story:** As an admin, I want the Attendance Dashboard to be reachable from the "Attendance" button on any other admin view's navigation bar, so that navigation is consistent across the application.

#### Acceptance Criteria

1. WHEN the "Attendance" navigation button is clicked on `AdminDashboard`, THE AdminDashboard SHALL open `AttendanceDashboard` and close itself, passing the current admin username.
2. WHEN the "Attendance" navigation button is clicked on `AdminEmployeesView`, THE AdminEmployeesView SHALL open `AttendanceDashboard` and close itself, passing the current admin username.
3. WHEN the "Attendance" navigation button is clicked on `AdminOverviewUI`, THE AdminOverviewUI SHALL open `AttendanceDashboard` and close itself, passing the current admin username.
4. THE AttendanceDashboard SHALL accept the admin username as a constructor parameter and use it to populate the `AdminGreeting` property.
