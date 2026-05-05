# Implementation Plan: AdminEmployeesView

## Overview

Implement the `AdminEmployeesView` WPF Window that opens when the admin clicks "Employees" in `AdminDashboard`. The window displays the full employee roster from the MySQL `employee` table using the existing `EmployeeEntry` record and `DatabaseHelper.GetAllEmployees()`. It reuses the application's dark theme, purple accent, custom scrollbar, and nav-bar pattern established by `AdminOverviewUI`.

## Tasks

- [x] 1. Create `AdminEmployeesView.xaml` — WPF Window XAML layout
  - [x] 1.1 Declare the `Window` root element with `x:Class="SOFTDEV.AdminEmployeesView"`, `Background="{StaticResource DarkBackgroundBrush}"`, `WindowStartupLocation="CenterScreen"`, `WindowState="Maximized"`, `ResizeMode="CanResize"`, `MinWidth="1200"`, `MinHeight="700"`, `Title="Admin Employees"`
    - _Requirements: 3.1, 3.4_
  - [x] 1.2 Add the top-level `Grid` with two `RowDefinitions`: `Height="Auto"` (header/nav) and `Height="*"` (main content)
    - _Requirements: 3.2_
  - [x] 1.3 Build Row 0 header: `StackPanel` containing a `DockPanel` (company logo left, control-group `StackPanel` right) with `SearchButton` (🔍, `CircleButtonStyle`), `NotificationButton` (🔔, `CircleButtonStyle`), `UserNameButton` (`PillButtonStyle`, `Width="80"`, `Height="30"`), and `AvatarButton` (👤, `CircleButtonStyle`)
    - _Requirements: 8.1_
  - [x] 1.4 Build the `UniformGrid` nav bar (`Rows="1"`, 7 buttons): `OverviewButton`, `EmployeesButton` (active: `Background="{StaticResource PurpleAccentBrush}"`, `Opacity="1.0"`), `AttendanceButton`, `ToDoButton`, `ReportsButton`, `LeavesButton`, `SettingsButton` — inactive buttons at `Opacity="0.4"` with `MouseEnter`/`MouseLeave` `DoubleAnimation` hover triggers (identical pattern to `AdminOverviewUI.xaml`)
    - _Requirements: 2.1, 2.3_
  - [x] 1.5 Build Row 1 main content: outer `ScrollViewer` with `CustomScrollbarStyle` scoped via `ScrollViewer.Resources`, containing a `Grid` with a `Border` (`CornerRadius="25"`, `Background="#1e1e2d"`, `Padding="24"`)
    - _Requirements: 3.3_
  - [x] 1.6 Inside the card `Border`, add a two-row inner `Grid`: Row 0 (card header) with a two-column layout — `TextBlock` "My Employees" (White, Bold, `FontSize="20"`) in Col 0, and a `StackPanel` with `FilterButton` ("FILTER ↓", `PillButtonStyle`) and `SortButton` ("SORT ↓", `PillButtonStyle`) in Col 1
    - _Requirements: 4.1, 4.2, 4.3, 4.4_
  - [x] 1.7 In Row 1 of the inner grid, add a `ScrollViewer` (`VerticalScrollBarVisibility="Auto"`, `HorizontalScrollBarVisibility="Disabled"`) with `CustomScrollbarStyle` scoped, containing `ItemsControl x:Name="EmployeeListControl"` with a `DataTemplate` for `EmployeeEntry`: horizontal `StackPanel` with a 44×44 `Border` (`CornerRadius="22"`, `Background="{StaticResource PurpleAccentBrush}"`) containing "👤" `TextBlock`, plus a vertical `StackPanel` with `{Binding Name}` (White, Bold, `FontSize="15"`) and `{Binding Position}` (`#aaaaaa`, `FontSize="12"`)
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 2. Create `AdminEmployeesView.xaml.cs` — code-behind
  - [x] 2.1 Declare the `AdminEmployeesView` partial class with private fields `_username` (string) and `_ownerDashboard` (Window?), and the public property `public List<EmployeeEntry> Employees { get; set; }`
    - _Requirements: 6.4, 7.1, 7.3_
  - [x] 2.2 Implement the constructor `public AdminEmployeesView(string username, Window? ownerDashboard = null)`: call `InitializeComponent()`, assign `_username` and `_ownerDashboard`, set `UserNameButton.Content = username`, call `LoadEmployees()`
    - _Requirements: 1.2, 8.1_
  - [x] 2.3 Implement `LoadEmployees()`: call `DatabaseHelper.GetAllEmployees()` inside a `try/catch (Exception ex)` block; on exception log via `Debug.WriteLine` and set `Employees` to an empty list; if `Employees.Count == 0` after the try/catch, assign the fallback list (Alice Santos / Software Engineer, Bob Reyes / Project Manager, Carol Lim / QA Analyst); assign `EmployeeListControl.ItemsSource = null` then `EmployeeListControl.ItemsSource = Employees`
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 7.2, 7.3_
  - [x] 2.4 Implement `BackButton_Click`: call `_ownerDashboard?.Show()` then `this.Close()`
    - _Requirements: 1.3_
  - [x] 2.5 Implement placeholder click handlers for `FilterButton_Click`, `SortButton_Click`, `SearchButton_Click`, `NotificationButton_Click`, `UserNameButton_Click`, and `AvatarButton_Click` — each logs the handler name via `System.Diagnostics.Debug.WriteLine` and does nothing else
    - _Requirements: 4.5, 8.2_

- [x] 3. Modify `AdminDashboard.xaml.cs` — add `EmployeesButton` nav branch
  - [x] 3.1 In `NavButton_Click`, add an `else if (sender == EmployeesButton)` branch that instantiates `new AdminEmployeesView(_username, this)`, calls `this.Hide()`, then calls `employeesView.Show()` — following the exact same pattern as the existing `OverviewButton` branch
    - _Requirements: 1.1_

- [x] 4. Write example-based tests in `SOFTDEV.Tests/AdminEmployeesViewTests.cs`
  - [x] 4.1 Add test: constructor sets `UserNameButton.Content` to the provided username string — construct `AdminEmployeesView("Alice")` on STA, assert `(window.FindName("UserNameButton") as Button).Content == "Alice"`
    - _Requirements: 1.2_
  - [x] 4.2 Add test: `BackButton_Click` calls `Show()` on the owner window and closes the view — use a mock/stub `Window` owner, invoke the back button click, assert owner is visible and view is closed
    - _Requirements: 1.3_
  - [x] 4.3 Add test: `FilterButton_Click` does not throw — invoke via `RaiseEvent` or reflection, assert no exception
    - _Requirements: 4.5_
  - [x] 4.4 Add test: `SortButton_Click` does not throw — same pattern as 4.3
    - _Requirements: 4.5_
  - [x] 4.5 Add test: control group button handlers (`SearchButton_Click`, `NotificationButton_Click`, `UserNameButton_Click`, `AvatarButton_Click`) do not throw
    - _Requirements: 8.2_
  - [x] 4.6 Add test: `EmployeesButton` in the nav bar has `Background` equal to `PurpleAccentBrush` and `Opacity == 1.0`
    - _Requirements: 2.1, 2.3_
  - [x] 4.7 Add test: `Employees` property has at least 3 entries after construction (fallback guarantee when DB is unavailable in test environment)
    - _Requirements: 6.2, 6.3_
  - [x] 4.8 Add test: `EmployeeListControl.Items.Count` equals `Employees.Count` after construction
    - _Requirements: 5.1, 6.1_

- [x] 5. Write property-based tests in `SOFTDEV.Tests/AdminEmployeesViewPropertyTests.cs`
  - [x] 5.1 Write property test for Property 1 — Username Display Invariant: use `[Property(MaxTest = 100)]` with `FsCheck.Xunit`; for any non-null, non-empty username string, construct `AdminEmployeesView(username, null)` on STA and assert `(window.FindName("UserNameButton") as Button).Content == username`
    - **Property 1: Username Display Invariant**
    - **Validates: Requirements 1.2**
  - [x] 5.2 Write property test for Property 2 — ItemsControl Count Matches Employee List: use `[Property(MaxTest = 100)]`; generate a random `List<EmployeeEntry>` (1–20 entries with random Name/Position strings), assign to `window.Employees`, refresh `EmployeeListControl.ItemsSource`, and assert `EmployeeListControl.Items.Count == window.Employees.Count`
    - **Property 2: ItemsControl Count Matches Employee List**
    - **Validates: Requirements 5.1, 6.1**
  - [x] 5.3 Write property test for Property 3 — Fallback Guarantee on DB Failure: use `[Property(MaxTest = 100)]`; for any empty-list outcome (and for exception-throwing stubs via reflection or subclassing), call `LoadEmployees()` and assert `window.Employees.Count >= 3` and no exception propagates; also verify the empty-list case directly
    - **Property 3: Fallback Guarantee on DB Failure**
    - **Validates: Requirements 6.2, 6.3**

- [x] 6. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- All WPF tests must call `StaHelper.RunOnSta(() => { WpfAppBootstrap.EnsureInitialized(); ... })` — reuse the helpers already defined in `AdminDashboardTests.cs`
- Property tests use `[Property(MaxTest = 100)]` from `FsCheck.Xunit` (already in `SOFTDEV.Tests.csproj`)
- `UserNameButton` uses `Button.Content` (not `TextBlock.Text`) to display the username — consistent with `AdminDashboard`'s `UserNameButton` pattern
- The `Employees` public property enables Property 2 and Property 3 tests without a live database connection
- The fallback list must contain at least 3 entries (Alice Santos, Bob Reyes, Carol Lim) to satisfy the `Employees.Count >= 3` invariant in Property 3
- No new model types, no new SQL queries, no new application-level styles — reuse everything from the existing codebase
