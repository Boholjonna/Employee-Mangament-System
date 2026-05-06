# Implementation Plan: AdminToDoTab

## Overview

Implement the Admin To Do tab as a WPF UserControl following the MVVM pattern. The work proceeds in dependency order: data model → infrastructure (RelayCommand) → ViewModel → View → dashboard integration. Property-based tests (FsCheck) validate the four correctness properties defined in the design.

## Tasks

- [x] 1. Add AdminTaskItem model to EmployeeModels.cs
  - Open `SOFTDEV/Models/EmployeeModels.cs` and append the `AdminTaskItem` class inside the `SOFTDEV` namespace, after the existing model classes.
  - Properties: `Id` (Guid, default `Guid.NewGuid()`), `Title` (string), `Description` (string), `AssignedTo` (string), `DueDate` (string?), `CreatedAt` (DateTime, default `DateTime.Now`), `Status` (string, default `"Pending"`).
  - Do **not** modify or remove any existing model class.
  - _Requirements: 10.1_

- [x] 2. Create RelayCommand helper
  - Create new file `SOFTDEV/ViewModels/RelayCommand.cs` (create the `ViewModels` folder if it does not exist).
  - Declare `namespace SOFTDEV.ViewModels`.
  - Implement `public class RelayCommand : ICommand` with:
    - Private fields `_execute` (`Action<object?>`) and `_canExecute` (`Func<object?, bool>?`).
    - Constructor `(Action<object?> execute, Func<object?, bool>? canExecute = null)`.
    - `CanExecute` delegates to `_canExecute` (returns `true` when null).
    - `Execute` delegates to `_execute`.
    - `public event EventHandler? CanExecuteChanged` and `public void RaiseCanExecuteChanged()`.
  - _Requirements: 8.2, 8.3, 9.4, 9.5_

- [x] 3. Create AdminToDoViewModel
  - Create new file `SOFTDEV/ViewModels/AdminToDoViewModel.cs`.
  - Declare `namespace SOFTDEV.ViewModels` with required `using` statements (`System`, `System.Collections.ObjectModel`, `System.ComponentModel`, `System.Runtime.CompilerServices`, `SOFTDEV`).
  - Implement `public class AdminToDoViewModel : INotifyPropertyChanged`:
    - Read-only `public string AdminName { get; }` set in constructor.
    - Backing fields and full property bodies (with `OnPropertyChanged()`) for `TaskTitle` (string), `TaskDescription` (string), `AssignedTo` (string), `DueDate` (string?). `TaskTitle` setter must also call `SaveTaskCommand.RaiseCanExecuteChanged()`.
    - `public ObservableCollection<AdminTaskItem> Tasks { get; } = new();`
    - Four `public RelayCommand` properties: `SaveTaskCommand`, `CancelTaskCommand`, `FilterCommand`, `SortCommand`.
    - Constructor `(string adminName)` wires all four commands; `SaveTaskCommand.CanExecute` predicate is `!string.IsNullOrWhiteSpace(TaskTitle)`.
    - `ExecuteSave()`: adds a new `AdminTaskItem` (Title trimmed, Description trimmed, AssignedTo, DueDate, CreatedAt = `DateTime.Now`, Status = `"Pending"`) then calls `ExecuteCancel()`.
    - `ExecuteCancel()`: resets `TaskTitle`, `TaskDescription`, `AssignedTo` to `string.Empty` and `DueDate` to `null`.
    - Standard `INotifyPropertyChanged` implementation with `[CallerMemberName]`.
  - _Requirements: 2.3, 4.4, 8.2, 8.3, 8.5, 9.4, 9.5, 10.1, 14.1, 14.2, 14.3_

  - [x] 3.1 Write property test — Property 1: AdminName reflects constructor input
    - **Property 1: AdminName binding reflects constructor input**
    - In the test project, add `AdminToDoViewModelTests.cs` (or equivalent). Using FsCheck `[Property]`, generate arbitrary `NonNull<string>` values, construct `new AdminToDoViewModel(name.Get)`, and assert `vm.AdminName == name.Get`.
    - **Validates: Requirements 2.3**

  - [x] 3.2 Write property test — Property 2: CanExecute false for null/empty/whitespace TaskTitle
    - **Property 2: SaveTaskCommand.CanExecute is false for empty or whitespace TaskTitle**
    - Generate arbitrary strings; skip (return `true`) when `!string.IsNullOrWhiteSpace(title)`. For null/empty/whitespace inputs, set `vm.TaskTitle = title ?? string.Empty` and assert `!vm.SaveTaskCommand.CanExecute(null)`.
    - **Validates: Requirements 4.4, 8.5**

  - [x] 3.3 Write property test — Property 3: Save adds exactly one item with correct trimmed Title
    - **Property 3: Saving a valid task adds exactly one item to the Tasks collection**
    - Generate `NonWhiteSpaceString` values. Set `vm.TaskTitle = title.Get`, record `countBefore = vm.Tasks.Count`, call `vm.SaveTaskCommand.Execute(null)`, assert `vm.Tasks.Count == countBefore + 1` and `vm.Tasks.Last().Title == title.Get.Trim()`.
    - **Validates: Requirements 10.1**

  - [x] 3.4 Write property test — Property 4: Cancel resets all form fields
    - **Property 4: CancelTaskCommand resets all form fields**
    - Generate arbitrary `string` values for title, desc, assignee, and `string?` for due. Set all four ViewModel properties, call `vm.CancelTaskCommand.Execute(null)`, assert `TaskTitle == string.Empty`, `TaskDescription == string.Empty`, `AssignedTo == string.Empty`, `DueDate == null`.
    - **Validates: Requirements 8.3, 14.2**

- [x] 4. Create AdminToDoTab code-behind
  - Create `SOFTDEV/Views/AdminToDoTab.xaml.cs`.
  - Declare `namespace SOFTDEV.Views`.
  - Class `public partial class AdminToDoTab : UserControl` with a single constructor that calls `InitializeComponent()`.
  - No business logic; no event handlers beyond `InitializeComponent`.
  - _Requirements: 14.1, 14.4_

- [x] 5. Create AdminToDoTab.xaml — local styles
  - Create `SOFTDEV/Views/AdminToDoTab.xaml` as a `UserControl` with:
    - `x:Class="SOFTDEV.Views.AdminToDoTab"`
    - Namespaces: default WPF, `x`, `vm="clr-namespace:SOFTDEV.ViewModels"`.
    - `Background="{StaticResource DarkBackgroundBrush}"`.
  - Inside `<UserControl.Resources>`, define all four local styles:
    - **PillTextBoxStyle** (`TargetType="TextBox"`): custom `ControlTemplate` with `Border` background `#4a447d`, `CornerRadius="25"`, `Padding="20,10"`, white `Foreground`, white `CaretBrush`, `FontSize="14"`, `PART_ContentHost` `ScrollViewer`.
    - **PillComboBoxStyle** (`TargetType="ComboBox"`): custom `ControlTemplate` with `Border` background `#4a447d`, `CornerRadius="25"`, `Padding="20,10"`, inner two-column `Grid` (content + `▾` arrow), `ContentPresenter` named `ContentSite`, `TextBlock` named `Placeholder` bound via `{TemplateBinding Tag}` in `#aaaaaa`, `Popup` (`PART_Popup`) with `Border` background `#4a447d` `CornerRadius="12"`, `DataTrigger` on `SelectedItem == {x:Null}` to show `Placeholder` / hide `ContentSite`.
    - **ActionButtonStyle** (`TargetType="Button"`): `ControlTemplate` with `Border` named `Bd`, background `#8b7ed6`, `CornerRadius="25"`, `Padding="32,12"`, bold black foreground, `IsMouseOver` trigger → `#7b6ec8`, `IsEnabled=False` trigger → background `#3a3560` opacity `0.5`.
    - **UtilityButtonStyle** (`TargetType="Button"`): `ControlTemplate` with `Border` named `Bd`, background `#554a9e`, `CornerRadius="20"`, `Padding="14,6"`, white foreground, `FontSize="12"`, `Opacity="0.7"`, `IsMouseOver` trigger → `Opacity="1.0"`.
  - _Requirements: 4.3, 6.4, 7.4, 8.4, 9.3, 15.2, 15.3_

- [x] 6. Create AdminToDoTab.xaml — root layout and header row
  - Inside the `UserControl`, add a root `Grid` with four `RowDefinition`s: `Auto`, `Auto`, `Auto`, `*`.
  - **Row 0** — header row (`Grid.Row="0"`): two-column `Grid` (col 0 `*`, col 1 `Auto`).
    - Col 0: `TextBlock` bound to `{Binding AdminName, StringFormat='Hello {0}! 👋'}`, `Foreground="#a294f9"`, `FontSize="13"`, `VerticalAlignment="Center"`, `Margin="20,12,0,8"`. Set `AutomationProperties.Name="Greeting"`.
    - Col 1: horizontal `StackPanel` right-aligned, `Margin="0,12,20,8"`:
      - `Button` content `"FILTER ▾"`, `Style="{StaticResource UtilityButtonStyle}"`, `Command="{Binding FilterCommand}"`, `AutomationProperties.Name="Filter Tasks"`, `Margin="0,0,8,0"`.
      - `Button` content `"SORT ▾"`, `Style="{StaticResource UtilityButtonStyle}"`, `Command="{Binding SortCommand}"`, `AutomationProperties.Name="Sort Tasks"`.
  - _Requirements: 1.1–1.3, 2.1–2.3, 9.1–9.5, 11.1, 12.3_

- [x] 7. Create AdminToDoTab.xaml — section headers row
  - **Row 1** (`Grid.Row="1"`): vertical `StackPanel`, `Margin="20,0,20,8"`.
    - `TextBlock` text `"To Do"`, `Foreground="White"`, `FontWeight="Bold"`, `FontSize="20"`.
    - `TextBlock` text `"ADD TASK:"`, `Foreground="White"`, `FontSize="16"`, `FontFamily="Georgia"` (or `"Times New Roman"`), `Margin="0,4,0,0"`.
  - _Requirements: 3.1, 3.2, 3.3_

- [x] 8. Create AdminToDoTab.xaml — Task_Form
  - **Row 2** (`Grid.Row="2"`): vertical `StackPanel` named `Task_Form`, `Margin="20,0,20,12"`.
  - **Title field**:
    - `TextBlock` text `"Task Title: (required)"`, `Foreground="White"`, `FontSize="13"`, `Margin="0,0,0,4"`, `x:Name="TitleLabel"`.
    - `TextBox` `Style="{StaticResource PillTextBoxStyle}"`, `Text="{Binding TaskTitle, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"`, `Margin="0,0,0,12"`, `AutomationProperties.LabeledBy="{Binding ElementName=TitleLabel}"`.
  - **Description field**:
    - `TextBlock` text `"Description: (mandatory)"`, `Foreground="White"`, `FontSize="13"`, `Margin="0,0,0,4"`, `x:Name="DescLabel"`.
    - `TextBox` `Style="{StaticResource PillTextBoxStyle}"`, `Text="{Binding TaskDescription, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"`, `AcceptsReturn="True"`, `TextWrapping="Wrap"`, `MinHeight="60"`, `Margin="0,0,0,12"`, `AutomationProperties.LabeledBy="{Binding ElementName=DescLabel}"`.
  - **Assignee + DueDate row**: two-column `Grid` (`*`, `*`), `Margin="0,0,0,12"`:
    - Col 0: `ComboBox` `Style="{StaticResource PillComboBoxStyle}"`, `Tag="Assigned to:"`, `SelectedValue="{Binding AssignedTo, Mode=TwoWay}"`, `Margin="0,0,8,0"`, `x:Name="AssigneeCombo"`, `AutomationProperties.Name="Assigned To"`.
    - Col 1: `ComboBox` `Style="{StaticResource PillComboBoxStyle}"`, `Tag="Due Date:"`, `SelectedValue="{Binding DueDate, Mode=TwoWay}"`, `x:Name="DueDateCombo"`, `AutomationProperties.Name="Due Date"`.
  - **Save + Cancel row**: horizontal `StackPanel`, `HorizontalAlignment="Center"`, `Margin="0,4,0,0"`:
    - `Button` content `"Save"`, `Style="{StaticResource ActionButtonStyle}"`, `Command="{Binding SaveTaskCommand}"`, `Margin="0,0,12,0"`, `AutomationProperties.Name="Save Task"`.
    - `Button` content `"Cancel"`, `Style="{StaticResource ActionButtonStyle}"`, `Command="{Binding CancelTaskCommand}"`, `AutomationProperties.Name="Cancel Task"`.
  - _Requirements: 4.1–4.3, 5.1–5.4, 6.1–6.5, 7.1–7.5, 8.1–8.5, 11.2–11.4, 12.1, 12.2, 13.1–13.3_

- [x] 9. Create AdminToDoTab.xaml — Task_List with empty state
  - **Row 3** (`Grid.Row="3"`): `ScrollViewer`, `VerticalScrollBarVisibility="Auto"`, `Margin="20,0,20,20"`.
    - Inside `ScrollViewer.Resources`: `<Style TargetType="ScrollBar" BasedOn="{StaticResource CustomScrollbarStyle}" />`.
    - Inside the `ScrollViewer`: a root `Grid` containing:
      - `ItemsControl` `ItemsSource="{Binding Tasks}"` with a `DataTemplate` that renders each `AdminTaskItem` as a `Border` (`Background="#1a1a2e"`, `CornerRadius="8"`, `Padding="16"`, `Margin="0,0,0,8"`) containing a `StackPanel` with:
        - `TextBlock` bound to `{Binding Title}`, `Foreground="White"`, `FontWeight="Bold"`, `FontSize="14"`.
        - `TextBlock` bound to `{Binding Description}`, `Foreground="#aaaaaa"`, `FontSize="12"`, `TextWrapping="Wrap"`.
        - Horizontal `StackPanel` with `TextBlock` bound to `{Binding AssignedTo}` and `TextBlock` bound to `{Binding DueDate}`, both `Foreground="#888888"`, `FontSize="11"`.
        - `TextBlock` bound to `{Binding Status}`, `Foreground="{StaticResource PurpleAccentBrush}"`, `FontSize="11"`.
      - `TextBlock` text `"No tasks yet."`, `Foreground="#888888"`, `FontSize="14"`, `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`, `Margin="0,24,0,0"`. Visibility controlled by a `Style` `DataTrigger` on `{Binding Tasks.Count}` — `Visible` when `0`, `Collapsed` otherwise.
  - _Requirements: 10.1–10.4, 11.1, 15.1_

- [x] 10. Checkpoint — verify build compiles cleanly
  - Ensure all new files are included in the project (`.csproj` may need `<Compile>` / `<Page>` entries if not auto-discovered).
  - Build the solution and confirm zero errors before proceeding to dashboard integration.
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Wire AdminToDoTab into AdminDashboard.xaml.cs
  - Open `SOFTDEV/AdminDashboard.xaml.cs`.
  - Add `using SOFTDEV.Views;` and `using SOFTDEV.ViewModels;` at the top (if not already present).
  - Inside `NavButton_Click`, add an `else if (sender == ToDoButton)` branch **before** the final `Debug.WriteLine` call:
    ```csharp
    else if (sender == ToDoButton)
    {
        MainContentGrid.Children.Clear();
        MainContentGrid.ColumnDefinitions.Clear();

        ToDoButton.Background      = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e4eb7"));
        OverviewButton.Background  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));
        EmployeesButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));
        AttendanceButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));
        ReportsButton.Background   = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));
        LeavesButton.Background    = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));
        SettingsButton.Background  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));

        var vm = new AdminToDoViewModel(_username);
        var todoTab = new AdminToDoTab { DataContext = vm };
        Grid.SetColumnSpan(todoTab, 3);
        MainContentGrid.Children.Add(todoTab);
    }
    ```
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 12. Final checkpoint — full build and smoke test
  - Build the solution; confirm zero errors and zero relevant warnings.
  - Run any existing unit/property tests and confirm they pass.
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP.
- Property tests (3.1–3.4) require FsCheck and FsCheck.Xunit NuGet packages in the test project.
- Each task references specific requirements for traceability.
- The `ViewModels` folder is new — create it alongside the existing `Models` and `Views` folders.
- `AdminToDoTab.xaml` must be added as a `<Page>` item in `SOFTDEV.csproj` if the project does not auto-include XAML files.
- The `CustomScrollbarStyle` referenced in Task 9 is already defined in `App.xaml` and available application-wide.
