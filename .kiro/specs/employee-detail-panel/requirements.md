# Requirements Document

## Introduction

This feature adds an Employee Detail Panel to the `AdminEmployeesView` in the WPF C# application. When an admin clicks any row in the employee list, the main content area splits into two columns: the left column retains the scrollable employee list, and the right column reveals a detail panel showing the selected employee's full profile. The panel displays a large circular avatar placeholder, and all employee fields stored in the `employee` table of the `userrole` MySQL database. The visual design follows the existing purple theme used throughout the AdminDashboard.

---

## Glossary

- **AdminEmployeesView**: The WPF `Window` (`AdminEmployeesView.xaml`) that displays the employee roster for admin users.
- **EmployeeListPanel**: The left-column panel inside `AdminEmployeesView` that contains the scrollable list of employees.
- **DetailPanel**: The right-column panel inside `AdminEmployeesView` that displays the full profile of the currently selected employee.
- **EmployeeDetail**: The C# model class that carries all displayable fields for a single employee: `Name`, `Position`, `Salary`, `Payroll`, `DateHired`, `ContactNo`, `Address`, and `EmergencyContact`.
- **DatabaseHelper**: The static C# class (`DatabaseHelper.cs`) that manages all MySQL queries against the `userrole` database.
- **EmployeeListControl**: The `ItemsControl` named `EmployeeListControl` in `AdminEmployeesView.xaml` that renders the employee rows.
- **PurpleAccentBrush**: The `#7B61FF` accent brush resource defined in `App.xaml` and used throughout the application's purple theme.
- **DarkBackgroundBrush**: The dark background brush resource defined in `App.xaml`.
- **AvatarPlaceholder**: The 120×120 circular `Border` element in the `DetailPanel` that displays the `👤` icon as a profile picture placeholder.

---

## Requirements

### Requirement 1: Split Layout

**User Story:** As an admin, I want the employee list and the employee detail panel to appear side by side, so that I can browse the list while viewing a selected employee's information without navigating away.

#### Acceptance Criteria

1. THE `AdminEmployeesView` SHALL divide the main content area into two columns using a `Grid` with a fixed-width left column for the `EmployeeListPanel` and a star-width right column for the `DetailPanel`.
2. WHEN no employee has been selected, THE `DetailPanel` SHALL have its `Visibility` set to `Collapsed`.
3. WHEN an employee is selected, THE `DetailPanel` SHALL have its `Visibility` set to `Visible`.
4. THE `EmployeeListPanel` SHALL remain fully functional and scrollable regardless of whether the `DetailPanel` is visible or collapsed.

---

### Requirement 2: Clickable Employee Rows

**User Story:** As an admin, I want to click any employee row in the list to select that employee, so that the detail panel updates to show the selected employee's information.

#### Acceptance Criteria

1. WHEN the admin clicks an employee row in `EmployeeListControl`, THE `AdminEmployeesView` SHALL load and display that employee's full details in the `DetailPanel`.
2. THE `EmployeeListControl` `DataTemplate` SHALL wrap each employee row in a `Button` (or equivalent clickable container) that raises a click event handled in the code-behind.
3. WHEN a row is clicked, THE `AdminEmployeesView` SHALL visually highlight the selected row using the `PurpleAccentBrush` to distinguish it from unselected rows.
4. WHEN a different row is clicked, THE `AdminEmployeesView` SHALL remove the highlight from the previously selected row and apply it to the newly selected row.

---

### Requirement 3: Employee Detail Data Model

**User Story:** As a developer, I want a dedicated model class for full employee details, so that all employee fields can be bound to the detail panel UI cleanly.

#### Acceptance Criteria

1. THE `EmployeeDetail` class SHALL expose the following properties: `Name` (`string`), `Position` (`string`), `Salary` (`decimal`), `Payroll` (`decimal`), `DateHired` (`string`), `ContactNo` (`string`), `Address` (`string`), and `EmergencyContact` (`string`).
2. THE `EmployeeDetail` class SHALL reside in `SOFTDEV/Models/EmployeeModels.cs` alongside the existing `EmployeeFinancialInfo` class.
3. THE `EmployeeDetail` class SHALL initialise all `string` properties to `string.Empty` by default to prevent null-reference binding errors in XAML.

---

### Requirement 4: Database Query for Full Employee Details

**User Story:** As a developer, I want a `DatabaseHelper` method that retrieves all fields for a specific employee, so that the detail panel can display complete and accurate data from the database.

#### Acceptance Criteria

1. THE `DatabaseHelper` SHALL expose a method `GetEmployeeDetails(string name)` that returns a nullable `EmployeeDetail` (i.e., `EmployeeDetail?`).
2. WHEN a matching employee name is found in the `employee` table, THE `DatabaseHelper` SHALL return an `EmployeeDetail` populated with `name`, `position`, `salary`, `payroll`, `datehired`, `contactno`, `address`, and `emergencycontact` column values.
3. WHEN no matching employee name is found, THE `DatabaseHelper` SHALL return `null`.
4. IF a `MySqlException` is thrown during the query, THEN THE `DatabaseHelper` SHALL log the error via `System.Diagnostics.Debug.WriteLine` and return `null`.
5. THE `GetEmployeeDetails` method SHALL use a parameterised query with `@name` to prevent SQL injection.

---

### Requirement 5: Detail Panel Layout and Visual Design

**User Story:** As an admin, I want the employee detail panel to display a large circular avatar and all employee fields in a clean, purple-themed layout, so that I can read the selected employee's information at a glance.

#### Acceptance Criteria

1. THE `DetailPanel` SHALL display an `AvatarPlaceholder` that is a `Border` with `Width="120"`, `Height="120"`, `CornerRadius="60"`, and `Background` set to `PurpleAccentBrush`, containing a `TextBlock` with the `👤` character.
2. THE `DetailPanel` SHALL display the following labelled fields to the right of the `AvatarPlaceholder`: Name, Position, Salary, Payroll, Date Hired, Contact No., Address, and Emergency Contact.
3. EACH field in the `DetailPanel` SHALL consist of a label `TextBlock` in a muted colour (`#aaaaaa`) and a value `TextBlock` in white, bound to the corresponding property of `EmployeeDetail`.
4. THE `DetailPanel` SHALL use `Background="#1e1e2d"` and `CornerRadius="25"` consistent with the existing card style in `AdminEmployeesView`.
5. THE `DetailPanel` SHALL display `Salary` and `Payroll` values formatted as currency (e.g., `{Binding Salary, StringFormat=₱{0:N2}}`).
6. WHEN the `DetailPanel` is visible, THE `AdminEmployeesView` SHALL display a section title (e.g., "Employee Details") in white bold text at the top of the `DetailPanel`.

---

### Requirement 6: Fallback Behaviour When Database Is Unavailable

**User Story:** As an admin, I want the application to handle database errors gracefully when loading employee details, so that the UI does not crash or display unhandled exceptions.

#### Acceptance Criteria

1. IF `DatabaseHelper.GetEmployeeDetails` returns `null`, THEN THE `AdminEmployeesView` SHALL keep the `DetailPanel` collapsed and not attempt to bind a null object.
2. IF `DatabaseHelper.GetEmployeeDetails` returns `null` after a row click, THEN THE `AdminEmployeesView` SHALL log a diagnostic message via `System.Diagnostics.Debug.WriteLine` identifying the employee name that could not be loaded.
3. THE `AdminEmployeesView` SHALL NOT display an unhandled exception dialog to the user when a database error occurs during detail loading.
