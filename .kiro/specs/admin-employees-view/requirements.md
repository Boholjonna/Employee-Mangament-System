# Requirements Document

## Introduction

The Admin Employees View is a dedicated WPF Window (`AdminEmployeesView`) that opens when the admin clicks the "Employees" navigation button in `AdminDashboard`. It displays the full employee roster pulled from the MySQL `employee` table, presenting each employee with a circular avatar, bold name, and position subtitle. The view reuses the application's established dark theme, purple accent, and custom scrollbar style. Filter and Sort action buttons are included as visual placeholders for future functionality. The navigation bar remains visible and the "Employees" button shows an active (solid purple) state while this view is open.

---

## Glossary

- **AdminDashboard**: The main WPF Window (`AdminDashboard.xaml` / `AdminDashboard.xaml.cs`) that hosts the navigation bar and delegates to child views.
- **AdminEmployeesView**: The new WPF Window (`AdminEmployeesView.xaml` / `AdminEmployeesView.xaml.cs`) introduced by this feature.
- **EmployeeEntry**: The existing C# `record` with `Name` (string) and `Position` (string) properties, representing one row from the `employee` table.
- **DatabaseHelper**: The existing static class that provides `GetAllEmployees()` returning `List<EmployeeEntry>`.
- **EmployeeListControl**: The `ItemsControl` inside `AdminEmployeesView` that renders the employee rows.
- **NavButton_Click**: The shared click handler in `AdminDashboard.xaml.cs` that routes navigation button events.
- **PillButtonStyle**: Application-level WPF style (`App.xaml`) for rounded pill-shaped buttons.
- **CircleButtonStyle**: Application-level WPF style for circular icon buttons.
- **CustomScrollbarStyle**: Application-level WPF `ScrollBar` style with a thin purple thumb on a dark track.
- **CardBackgroundBrush**: Application-level `SolidColorBrush` (`#15151b`) used for card surfaces.
- **DarkBackgroundBrush**: Application-level `SolidColorBrush` (`#0a0a0a`) used for window backgrounds.
- **PurpleAccentBrush**: Application-level `SolidColorBrush` (`#7b61ff`) used for highlights and active states.
- **ActiveNavStyle**: A visual state where a navigation button displays a solid purple background (`PurpleAccentBrush`) with white foreground to indicate the currently active view.
- **FilterButton**: A pill-shaped button labelled "FILTER ↓" in the `AdminEmployeesView` header; a placeholder for future filter functionality.
- **SortButton**: A pill-shaped button labelled "SORT ↓" in the `AdminEmployeesView` header; a placeholder for future sort functionality.
- **AvatarCircle**: A circular `Border` (CornerRadius ≥ 18) containing a default "👤" emoji, representing an employee avatar when no image path is available.

---

## Requirements

### Requirement 1: Navigation Integration

**User Story:** As an admin, I want clicking the "Employees" navigation button to open the Employees view, so that I can access the employee roster from the dashboard.

#### Acceptance Criteria

1. WHEN the admin clicks `EmployeesButton` in `AdminDashboard`, THE `NavButton_Click` handler SHALL instantiate `AdminEmployeesView`, hide `AdminDashboard`, and show `AdminEmployeesView`.
2. WHEN `AdminEmployeesView` is instantiated, THE `AdminEmployeesView` constructor SHALL accept the admin's `_username` string and a reference to the parent `AdminDashboard` window, following the same signature pattern as `AdminOverviewUI(string username, Window? ownerDashboard)`.
3. WHEN the admin closes or navigates back from `AdminEmployeesView`, THE `AdminEmployeesView` SHALL call `Show()` on the parent `AdminDashboard` reference and close itself, restoring the dashboard.

---

### Requirement 2: Active Navigation Tab State

**User Story:** As an admin, I want the "Employees" button to appear highlighted while the Employees view is open, so that I can see which section is currently active.

#### Acceptance Criteria

1. WHILE `AdminEmployeesView` is the active window, THE `EmployeesButton` in `AdminDashboard` SHALL display a solid `PurpleAccentBrush` background with white foreground text (ActiveNavStyle).
2. WHEN `AdminEmployeesView` is closed and `AdminDashboard` is restored, THE `EmployeesButton` SHALL revert to its default `PillButtonStyle` appearance (no persistent active highlight).
3. THE `AdminEmployeesView` header navigation bar SHALL display the same seven navigation buttons (`OverviewButton`, `EmployeesButton`, `AttendanceButton`, `ToDoButton`, `ReportsButton`, `LeavesButton`, `SettingsButton`) as `AdminDashboard`, with `EmployeesButton` rendered in ActiveNavStyle.

---

### Requirement 3: Window Layout and Dark Theme

**User Story:** As an admin, I want the Employees view to match the application's dark theme and layout conventions, so that the UI feels consistent with the rest of the dashboard.

#### Acceptance Criteria

1. THE `AdminEmployeesView` window SHALL set its `Background` to `DarkBackgroundBrush` (`#0a0a0a`).
2. THE `AdminEmployeesView` SHALL contain a top-level `Grid` with two `RowDefinitions`: `Height="Auto"` for the header/navigation row and `Height="*"` for the main content area.
3. THE main content area SHALL contain a `Border` with `CornerRadius="25"`, `Background` set to `#1e1e2d`, and sufficient `Padding` (minimum 24) to serve as the primary card container.
4. THE `AdminEmployeesView` SHALL set `WindowStartupLocation="CenterScreen"`, `WindowState="Maximized"`, and `ResizeMode="CanResize"` to match `AdminDashboard` sizing behaviour.

---

### Requirement 4: View Header

**User Story:** As an admin, I want a clear header inside the Employees view card showing the section title and action buttons, so that I can identify the view and access filter/sort controls.

#### Acceptance Criteria

1. THE main card `Border` SHALL contain a header row with a `TextBlock` displaying the text "My Employees" in white, bold, at a minimum `FontSize` of 18.
2. THE header row SHALL place `FilterButton` and `SortButton` right-aligned using a `Grid` with `ColumnDefinitions` of `Width="*"` (title) and `Width="Auto"` (buttons).
3. THE `FilterButton` SHALL display the label "FILTER ↓", use `PillButtonStyle`, and have `CornerRadius="15"`.
4. THE `SortButton` SHALL display the label "SORT ↓", use `PillButtonStyle`, and have `CornerRadius="15"`.
5. WHEN the admin clicks `FilterButton` or `SortButton`, THE `AdminEmployeesView` SHALL log the button name to `System.Diagnostics.Debug` without throwing an exception, as these are placeholder actions.

---

### Requirement 5: Employee List Display

**User Story:** As an admin, I want to see all employees listed with their name and position, so that I can review the full roster at a glance.

#### Acceptance Criteria

1. THE `EmployeeListControl` (`ItemsControl`) SHALL render one row per `EmployeeEntry` in its `ItemsSource` collection.
2. EACH employee row `DataTemplate` SHALL contain an `AvatarCircle` (`Border` with `CornerRadius="18"`, `Width="36"`, `Height="36"`, `Background="{StaticResource PurpleAccentBrush}"`), a bold white `TextBlock` bound to `{Binding Name}`, and a smaller (`FontSize` ≤ 13) grey (`#aaaaaa`) `TextBlock` bound to `{Binding Position}`.
3. THE `AvatarCircle` SHALL display the "👤" emoji as its content when no employee image path is available.
4. THE `EmployeeListControl` SHALL be wrapped in a `ScrollViewer` with `VerticalScrollBarVisibility="Auto"` and `HorizontalScrollBarVisibility="Disabled"`.
5. THE `ScrollViewer` SHALL apply `CustomScrollbarStyle` to its inner `ScrollBar` via a scoped `Style` resource, producing a thin purple vertical scrollbar.

---

### Requirement 6: Data Loading from Database

**User Story:** As an admin, I want the employee list to be populated from the MySQL database on view load, so that I always see current employee data.

#### Acceptance Criteria

1. WHEN `AdminEmployeesView` is constructed, THE `AdminEmployeesView` SHALL call `DatabaseHelper.GetAllEmployees()` and assign the returned `List<EmployeeEntry>` as the `ItemsSource` of `EmployeeListControl`.
2. IF `DatabaseHelper.GetAllEmployees()` returns an empty list, THEN THE `AdminEmployeesView` SHALL populate `EmployeeListControl` with a predefined fallback list of at least three `EmployeeEntry` placeholder records so the UI is never blank.
3. IF `DatabaseHelper.GetAllEmployees()` throws an exception, THEN THE `AdminEmployeesView` SHALL catch the exception, log it via `System.Diagnostics.Debug.WriteLine`, and display the fallback placeholder list without crashing.
4. THE `AdminEmployeesView` SHALL expose a `public List<EmployeeEntry> Employees` property that holds the currently displayed collection, enabling testability without a live database connection.

---

### Requirement 7: C# Data Model and Collection

**User Story:** As a developer, I want the Employees view to use the existing `EmployeeEntry` record and `DatabaseHelper` without introducing new model types, so that the codebase stays consistent.

#### Acceptance Criteria

1. THE `AdminEmployeesView` code-behind SHALL use the existing `EmployeeEntry` record (`Name`, `Position`) as the item type for the employee collection — no new model class SHALL be introduced for this feature.
2. THE `AdminEmployeesView` code-behind SHALL use `DatabaseHelper.GetAllEmployees()` as the sole database access method for loading employee data — no new SQL queries SHALL be written in the view code-behind.
3. THE `AdminEmployeesView` SHALL store the employee collection in an `ObservableCollection<EmployeeEntry>` or `List<EmployeeEntry>` field and bind it to `EmployeeListControl.ItemsSource` by assignment (not via `DataContext` binding), consistent with the pattern used in `AdminDashboard.LoadEmployees()`.

---

### Requirement 8: Control Group Header (Search, Bell, User, Avatar)

**User Story:** As an admin, I want the top control group (search, notification, username, avatar buttons) to appear in `AdminEmployeesView`, so that the header chrome is consistent with `AdminDashboard`.

#### Acceptance Criteria

1. THE `AdminEmployeesView` header section SHALL include a right-aligned `StackPanel` containing `SearchButton` (🔍, `CircleButtonStyle`), `NotificationButton` (🔔, `CircleButtonStyle`), `UserNameButton` (displays `_username`, `PillButtonStyle`, `Width="80"`, `Height="30"`), and `AvatarButton` (👤, `CircleButtonStyle`), matching the layout in `AdminDashboard`.
2. WHEN the admin clicks any control group button in `AdminEmployeesView`, THE `AdminEmployeesView` SHALL log the button name to `System.Diagnostics.Debug` without throwing an exception, as these are placeholder actions in this view.
