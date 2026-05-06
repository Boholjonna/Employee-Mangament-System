# Design Document

## Employee Detail Panel

---

## Overview

This feature extends `AdminEmployeesView` to support a split-panel layout where clicking an employee row in the left list reveals a full-profile detail panel on the right. The detail panel displays all fields from the `employee` table (`name`, `position`, `salary`, `payroll`, `datehired`, `contactno`, `address`, `emergencycontact`) in a purple-themed card consistent with the existing `AdminDashboard` visual language.

The implementation touches four files:

| File | Change |
|---|---|
| `SOFTDEV/Models/EmployeeModels.cs` | Add `EmployeeDetail` model class |
| `SOFTDEV/DatabaseHelper.cs` | Add `GetEmployeeDetails(string name)` query method |
| `SOFTDEV/AdminEmployeesView.xaml` | Restructure layout to two-column Grid; add `DetailPanel`; make rows clickable |
| `SOFTDEV/AdminEmployeesView.xaml.cs` | Add selection state management and detail-loading logic |

No new files, NuGet packages, or external dependencies are introduced.

---

## Architecture

The feature follows the existing code-behind pattern used throughout the project — there is no MVVM framework or data-binding infrastructure beyond standard WPF `{Binding}`. All state is managed directly in `AdminEmployeesView.xaml.cs`.

```
┌─────────────────────────────────────────────────────────────────┐
│  AdminEmployeesView (Window)                                    │
│                                                                 │
│  ┌──────────────────────┐   ┌──────────────────────────────┐   │
│  │  EmployeeListPanel   │   │  DetailPanel (right col)     │   │
│  │  (fixed-width col)   │   │  Visibility: Collapsed/Visible│  │
│  │                      │   │                              │   │
│  │  ItemsControl        │   │  AvatarPlaceholder (120×120) │   │
│  │  ┌────────────────┐  │   │  EmployeeDetail fields       │   │
│  │  │ Button wrapper │──┼───┼─► code-behind click handler  │   │
│  │  │  (row item)    │  │   │     └─► GetEmployeeDetails() │   │
│  │  └────────────────┘  │   │         └─► bind to panel    │   │
│  └──────────────────────┘   └──────────────────────────────┘   │
│                                                                 │
│  Code-behind state:                                             │
│    _selectedButton : Button?   (currently highlighted row)     │
│    _selectedDetail : EmployeeDetail?  (currently shown data)   │
└─────────────────────────────────────────────────────────────────┘
                          │
                          ▼
              DatabaseHelper.GetEmployeeDetails(name)
                          │
                          ▼
              MySQL: userrole.employee table
```

**Key design decisions:**

- **Fixed left column width (300 px)**: Keeps the list readable at all window sizes without the detail panel squeezing it. The right column takes the remaining space (`*`).
- **Code-behind over MVVM**: Consistent with the rest of the project. Introducing a ViewModel would be out of scope and inconsistent.
- **`Button` wrapper in `DataTemplate`**: The simplest WPF mechanism for making an `ItemsControl` row clickable without switching to `ListBox`. The `Button` uses a transparent/custom template so it looks like a plain row, with background changed on selection.
- **`DataContext` assignment for detail binding**: The `DetailPanel` `Border`'s `DataContext` is set to the `EmployeeDetail` instance in code-behind, keeping XAML bindings simple and avoiding a separate `SelectedEmployee` property.

---

## Components and Interfaces

### 1. `EmployeeDetail` (new model class)

Location: `SOFTDEV/Models/EmployeeModels.cs`

```csharp
public class EmployeeDetail
{
    public string  Name             { get; set; } = string.Empty;
    public string  Position         { get; set; } = string.Empty;
    public decimal Salary           { get; set; }
    public decimal Payroll          { get; set; }
    public string  DateHired        { get; set; } = string.Empty;
    public string  ContactNo        { get; set; } = string.Empty;
    public string  Address          { get; set; } = string.Empty;
    public string  EmergencyContact { get; set; } = string.Empty;
}
```

All `string` properties default to `string.Empty` to prevent null-reference binding errors in XAML.

---

### 2. `DatabaseHelper.GetEmployeeDetails` (new query method)

Location: `SOFTDEV/DatabaseHelper.cs`

```csharp
/// <summary>
/// Returns all profile fields for the employee with the given name,
/// or null if no match is found or a database error occurs.
/// </summary>
public static EmployeeDetail? GetEmployeeDetails(string name)
```

- Uses a parameterised `@name` parameter to prevent SQL injection.
- Queries columns: `name`, `position`, `salary`, `payroll`, `datehired`, `contactno`, `address`, `emergencycontact`.
- Returns `null` on no match or `MySqlException`; logs errors via `Debug.WriteLine`.
- `datehired` is read as a string (via `GetString`) to avoid locale-dependent `DateTime` formatting issues in the UI.

---

### 3. `AdminEmployeesView.xaml` layout changes

The existing single-column `Grid` inside the primary `Border` card is replaced with a two-column `Grid`:

```
Column 0: Width="300"   → EmployeeListPanel (existing card content)
Column 1: Width="*"     → DetailPanel (new, initially Collapsed)
```

The `ItemsControl` `DataTemplate` wraps each row in a `Button` with a transparent style. A `Tag` binding carries the employee `Name` to the click handler.

The `DetailPanel` is a `Border` (`x:Name="DetailPanel"`) with `Visibility="Collapsed"`, `Background="#1e1e2d"`, `CornerRadius="25"`, and `Margin="16,0,0,0"`. Its internal layout uses a `StackPanel` with:
- A section title `TextBlock` ("Employee Details")
- A horizontal `StackPanel` containing the `AvatarPlaceholder` and a `Grid` of label/value pairs

---

### 4. `AdminEmployeesView.xaml.cs` code-behind changes

New private fields:

```csharp
private Button?         _selectedButton = null;
private EmployeeDetail? _selectedDetail = null;
```

New method `EmployeeRow_Click(object sender, RoutedEventArgs e)`:
1. Extracts the employee `Name` from the clicked `Button.Tag`.
2. Resets `_selectedButton` background to transparent; sets new button background to `PurpleAccentBrush`.
3. Calls `DatabaseHelper.GetEmployeeDetails(name)`.
4. If result is `null`: logs via `Debug.WriteLine`, keeps `DetailPanel.Visibility = Collapsed`.
5. If result is non-null: sets `DetailPanel.DataContext = result`, sets `DetailPanel.Visibility = Visible`.

---

## Data Models

### `EmployeeDetail` property mapping

| Property | DB Column | Type | Default |
|---|---|---|---|
| `Name` | `name` | `string` | `string.Empty` |
| `Position` | `position` | `string` | `string.Empty` |
| `Salary` | `salary` | `decimal` | `0m` |
| `Payroll` | `payroll` | `decimal` | `0m` |
| `DateHired` | `datehired` | `string` | `string.Empty` |
| `ContactNo` | `contactno` | `string` | `string.Empty` |
| `Address` | `address` | `string` | `string.Empty` |
| `EmergencyContact` | `emergencycontact` | `string` | `string.Empty` |

### XAML binding expressions (DetailPanel)

| Field label | Binding expression |
|---|---|
| Name | `{Binding Name}` |
| Position | `{Binding Position}` |
| Salary | `{Binding Salary, StringFormat=₱{0:N2}}` |
| Payroll | `{Binding Payroll, StringFormat=₱{0:N2}}` |
| Date Hired | `{Binding DateHired}` |
| Contact No. | `{Binding ContactNo}` |
| Address | `{Binding Address}` |
| Emergency Contact | `{Binding EmergencyContact}` |

### Layout diagram

```
DetailPanel (Border, #1e1e2d, CornerRadius=25, Margin=16,0,0,0)
└── StackPanel (Vertical, Padding=24)
    ├── TextBlock "Employee Details" (White, Bold, FontSize=18, Margin=0,0,0,16)
    └── StackPanel (Horizontal)
        ├── Border (AvatarPlaceholder, 120×120, CornerRadius=60, PurpleAccentBrush)
        │   └── TextBlock "👤" (centered)
        └── Grid (Margin=24,0,0,0)
            ├── ColumnDefinition Width="Auto"  (labels)
            └── ColumnDefinition Width="*"     (values)
            Rows: Name, Position, Salary, Payroll, DateHired,
                  ContactNo, Address, EmergencyContact
            Each row:
              Col 0: TextBlock (label, #aaaaaa, FontSize=12)
              Col 1: TextBlock (value, White, FontSize=14, Margin=12,0,0,0)
```

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Row selection makes the detail panel visible with correct data

*For any* employee in the employee list, when that employee's row is clicked and `GetEmployeeDetails` returns a non-null result, the `DetailPanel` shall have `Visibility == Visible` and its `DataContext` shall be an `EmployeeDetail` whose `Name` matches the clicked employee's name.

**Validates: Requirements 1.3, 2.1**

---

### Property 2: Highlight exclusivity on re-selection

*For any* two distinct employees A and B in the employee list, after clicking A's row and then clicking B's row, only B's row button shall have its background set to `PurpleAccentBrush`; A's row button shall have its background reset to transparent.

**Validates: Requirements 2.3, 2.4**

---

### Property 3: EmployeeDetail default string properties are non-null

*For any* `EmployeeDetail` instance created with the default (parameterless) constructor, all `string`-typed properties (`Name`, `Position`, `DateHired`, `ContactNo`, `Address`, `EmergencyContact`) shall be non-null (equal to `string.Empty`).

**Validates: Requirements 3.3**

---

### Property 4: Null database result keeps the detail panel collapsed

*For any* employee name for which `GetEmployeeDetails` returns `null`, the `DetailPanel` shall remain `Visibility == Collapsed` and no null-reference exception shall be thrown.

**Validates: Requirements 6.1, 6.3**

---

## Error Handling

| Scenario | Handling |
|---|---|
| `GetEmployeeDetails` returns `null` (no DB match) | `DetailPanel` stays `Collapsed`; `Debug.WriteLine` logs the employee name |
| `MySqlException` inside `GetEmployeeDetails` | Caught inside `DatabaseHelper`; logs via `Debug.WriteLine`; returns `null` |
| `null` propagated to `EmployeeRow_Click` | Null-check before setting `DataContext`; panel stays `Collapsed` |
| Unhandled exception reaching the UI thread | Prevented by the null-check guard; no exception dialog shown to user |

The code-behind never sets `DetailPanel.DataContext = null` — it only assigns a valid `EmployeeDetail` instance or leaves the previous `DataContext` in place while keeping the panel `Collapsed`. This prevents WPF binding errors from a null `DataContext`.

---

## Testing Strategy

This feature is primarily a WPF UI feature. The testable logic lives in two places: the `EmployeeDetail` model (pure C# class) and the selection/visibility state management in the code-behind. The database query method (`GetEmployeeDetails`) is an integration concern.

### Unit Tests

Focus on the pure C# logic that can be exercised without a WPF dispatcher or live database:

- **`EmployeeDetail` default initialization**: Verify all string properties are `string.Empty` on a default-constructed instance (covers Property 3).
- **Null-return guard**: Using a test subclass or dependency injection shim, verify that when `GetEmployeeDetails` returns `null`, the panel stays collapsed and no exception is thrown (covers Property 4).
- **Selection state reset**: Verify that `_selectedButton` tracking correctly resets the previous button's background when a new row is clicked (covers Property 2).

### Property-Based Tests

Property-based testing is applicable to the model initialization and selection state logic. Use **CsCheck** (a .NET property-based testing library) configured for a minimum of 100 iterations per property.

Each test is tagged with the corresponding design property:

```
// Feature: employee-detail-panel, Property 3: EmployeeDetail default string properties are non-null
// Feature: employee-detail-panel, Property 1: Row selection makes the detail panel visible with correct data
// Feature: employee-detail-panel, Property 2: Highlight exclusivity on re-selection
// Feature: employee-detail-panel, Property 4: Null database result keeps the detail panel collapsed
```

**Property 1 test approach**: Generate a list of `EmployeeEntry` objects with random names/positions. For each, simulate the `EmployeeRow_Click` logic with a stub that returns a matching `EmployeeDetail`. Assert `DetailPanel.Visibility == Visible` and `DataContext.Name == employee.Name`.

**Property 2 test approach**: Generate pairs of distinct employee names. Simulate clicking A then B. Assert only B's button has the accent background.

**Property 3 test approach**: Construct `EmployeeDetail` instances (default constructor). Assert no string property is null.

**Property 4 test approach**: Generate arbitrary employee names. Simulate `EmployeeRow_Click` with a stub returning `null`. Assert `DetailPanel.Visibility == Collapsed` and no exception is thrown.

### Integration Tests

- **`GetEmployeeDetails` with a real DB record**: Insert a known employee row into a test database, call `GetEmployeeDetails`, verify all eight fields are populated correctly (covers Requirement 4.2).
- **`GetEmployeeDetails` with no match**: Query a name that does not exist, verify `null` is returned (covers Requirement 4.3).

### What is NOT tested with PBT

- XAML layout structure (column widths, `CornerRadius`, `Background` values) — verified by XAML inspection and visual review.
- Currency formatting (`StringFormat=₱{0:N2}`) — verified by a single example-based unit test.
- `MySqlException` logging — verified by a single example-based unit test with a broken connection mock.
