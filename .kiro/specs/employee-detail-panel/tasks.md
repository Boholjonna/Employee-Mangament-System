# Implementation Plan: Employee Detail Panel

## Overview

Extend `AdminEmployeesView` with a split-panel layout. Clicking an employee row reveals a full-profile detail panel on the right. The work is broken into four incremental steps that mirror the four files being changed, finishing with wiring everything together.

## Tasks

- [x] 1. Add `EmployeeDetail` model class
  - Open `SOFTDEV/Models/EmployeeModels.cs` and add the `EmployeeDetail` class after the existing `EmployeeFinancialInfo` class
  - Declare all eight properties: `Name`, `Position`, `Salary`, `Payroll`, `DateHired`, `ContactNo`, `Address`, `EmergencyContact`
  - Initialise all `string` properties to `string.Empty` and `decimal` properties to `0m`
  - _Requirements: 3.1, 3.2, 3.3_

  - [ ]* 1.1 Write property test for `EmployeeDetail` default string properties
    - **Property 3: EmployeeDetail default string properties are non-null**
    - Use CsCheck to generate arbitrary `EmployeeDetail` instances (default constructor) and assert that `Name`, `Position`, `DateHired`, `ContactNo`, `Address`, and `EmergencyContact` are all non-null (equal to `string.Empty`)
    - Tag the test: `// Feature: employee-detail-panel, Property 3`
    - **Validates: Requirements 3.3**

- [x] 2. Add `GetEmployeeDetails` to `DatabaseHelper`
  - Open `SOFTDEV/DatabaseHelper.cs` and add the `GetEmployeeDetails(string name)` static method
  - Write a parameterised SQL query selecting `name`, `position`, `salary`, `payroll`, `datehired`, `contactno`, `address`, `emergencycontact` from the `employee` table where `name = @name LIMIT 1`
  - Read `datehired` as a string via `GetString` to avoid locale-dependent formatting
  - Return `null` on no match; catch `MySqlException`, log via `Debug.WriteLine`, and return `null`
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

  - [ ]* 2.1 Write unit test for null-return guard in `GetEmployeeDetails`
    - Verify that when the database returns no rows (or throws), `GetEmployeeDetails` returns `null` and does not throw
    - _Requirements: 4.3, 4.4_

- [x] 3. Restructure `AdminEmployeesView.xaml` layout
  - Open `SOFTDEV/AdminEmployeesView.xaml`
  - Replace the single-column `Grid` inside the primary `Border` card with a two-column `Grid`: `Column 0 Width="300"` (employee list) and `Column 1 Width="*"` (detail panel)
  - Move the existing card header (`Grid.Row="0"`) and `ScrollViewer`/`ItemsControl` (`Grid.Row="1"`) into `Column 0` of the new two-column grid
  - Wrap each employee row in the `DataTemplate` inside a `Button` with `Background="Transparent"`, `BorderThickness="0"`, `HorizontalContentAlignment="Stretch"`, `Tag="{Binding Name}"`, and `Click="EmployeeRow_Click"`
  - Add the `DetailPanel` `Border` in `Column 1`: `x:Name="DetailPanel"`, `Visibility="Collapsed"`, `Background="#1e1e2d"`, `CornerRadius="25"`, `Margin="16,0,0,0"`
  - Inside `DetailPanel`, add a vertical `StackPanel` with `Padding="24"` containing:
    - A `TextBlock` "Employee Details" (`Foreground="White"`, `FontWeight="Bold"`, `FontSize="18"`, `Margin="0,0,0,16"`)
    - A horizontal `StackPanel` with the `AvatarPlaceholder` (`Border` `Width="120"` `Height="120"` `CornerRadius="60"` `Background="{StaticResource PurpleAccentBrush}"` containing a centred `👤` `TextBlock`) and a two-column `Grid` (`Margin="24,0,0,0"`) with label/value row pairs for all eight fields
  - Use `{Binding Salary, StringFormat=₱{0:N2}}` and `{Binding Payroll, StringFormat=₱{0:N2}}` for currency fields; all other fields use plain `{Binding PropertyName}`
  - Label `TextBlock`s: `Foreground="#aaaaaa"`, `FontSize="12"`; value `TextBlock`s: `Foreground="White"`, `FontSize="14"`, `Margin="12,0,0,0"`
  - _Requirements: 1.1, 1.2, 2.2, 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_

- [x] 4. Add selection state and click handler to `AdminEmployeesView.xaml.cs`
  - Open `SOFTDEV/AdminEmployeesView.xaml.cs`
  - Add two private fields: `private Button? _selectedButton = null;` and `private EmployeeDetail? _selectedDetail = null;`
  - Implement `EmployeeRow_Click(object sender, RoutedEventArgs e)`:
    1. Cast `sender` to `Button` and read the employee name from `button.Tag as string`
    2. Reset `_selectedButton?.Background` to `Transparent`; set the new button's `Background` to `(Brush)FindResource("PurpleAccentBrush")`; update `_selectedButton`
    3. Call `DatabaseHelper.GetEmployeeDetails(name)`
    4. If result is `null`: log `Debug.WriteLine($"[AdminEmployeesView] Could not load details for: {name}")` and set `DetailPanel.Visibility = Collapsed`
    5. If result is non-null: set `DetailPanel.DataContext = result` and `DetailPanel.Visibility = Visible`; update `_selectedDetail`
  - _Requirements: 1.3, 1.4, 2.1, 2.3, 2.4, 6.1, 6.2, 6.3_

  - [ ]* 4.1 Write property test for highlight exclusivity on re-selection
    - **Property 2: Highlight exclusivity on re-selection**
    - Use CsCheck to generate pairs of distinct employee names; simulate clicking row A then row B using the `EmployeeRow_Click` logic (or a testable extraction of it); assert only B's button has `PurpleAccentBrush` background and A's is `Transparent`
    - Tag the test: `// Feature: employee-detail-panel, Property 2`
    - **Validates: Requirements 2.3, 2.4**

  - [ ]* 4.2 Write property test for null database result keeping panel collapsed
    - **Property 4: Null database result keeps the detail panel collapsed**
    - Use CsCheck to generate arbitrary employee names; simulate `EmployeeRow_Click` with a stub/delegate that returns `null` for `GetEmployeeDetails`; assert `DetailPanel.Visibility == Collapsed` and no exception is thrown
    - Tag the test: `// Feature: employee-detail-panel, Property 4`
    - **Validates: Requirements 6.1, 6.3**

  - [ ]* 4.3 Write property test for row selection making the detail panel visible
    - **Property 1: Row selection makes the detail panel visible with correct data**
    - Use CsCheck to generate a list of `EmployeeEntry` objects with random names/positions; for each, simulate `EmployeeRow_Click` with a stub returning a matching `EmployeeDetail`; assert `DetailPanel.Visibility == Visible` and `DetailPanel.DataContext` is an `EmployeeDetail` whose `Name` matches the clicked employee's name
    - Tag the test: `// Feature: employee-detail-panel, Property 1`
    - **Validates: Requirements 1.3, 2.1**

- [x] 5. Checkpoint — Ensure all tests pass
  - Build the solution and confirm there are no compile errors
  - Run all unit and property tests; ensure all pass
  - Visually verify the split layout, row highlighting, and detail panel in the running application
  - Ask the user if any questions arise before closing out

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP
- Each task references specific requirements for traceability
- Property tests use CsCheck; tag each test with the property number and feature name as shown above
- The `DetailPanel.DataContext` is never set to `null` — only a valid `EmployeeDetail` instance is assigned; the panel is hidden via `Visibility = Collapsed` when data is unavailable
- The `Button` wrapper in the `DataTemplate` must suppress its default chrome (border, hover highlight) so it looks like a plain row; use `BorderThickness="0"` and `Background="Transparent"` with `Padding="0"`
