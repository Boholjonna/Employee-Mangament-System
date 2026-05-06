# Requirements Document

## Introduction

This feature adds an `AdminToDoTab` WPF UserControl to the Admin Dashboard. The control provides a task management interface styled with the application's existing modern dark-purple aesthetic. It allows an admin to add new tasks (with title, description, assignee, and due date), view the task list, and filter or sort tasks. The control is designed for MVVM integration via Command bindings and is embedded in the existing `AdminDashboard` navigation flow.

---

## Glossary

- **AdminToDoTab**: The WPF UserControl that renders the To Do tab content within the Admin Dashboard.
- **Task_Form**: The input area within AdminToDoTab used to create a new task.
- **Task_List**: The scrollable list area within AdminToDoTab that displays existing tasks.
- **NavBar**: The horizontal row of navigation buttons at the top of the Admin Dashboard window.
- **ViewModel**: The data-context class that backs AdminToDoTab via WPF data binding and ICommand.
- **PillTextBox**: A TextBox styled with a dark-purple background (`#4a447d`) and a CornerRadius of 25, giving a pill/capsule shape.
- **PillComboBox**: A ComboBox styled identically to PillTextBox but with a downward-arrow indicator on the right.
- **ActionButton**: A large rounded button with background `#8b7ed6`, bold black text, and a high CornerRadius for pill appearance.
- **UtilityButton**: A small, oval, semi-transparent purple button used for Filter and Sort actions.
- **CustomScrollBar**: A ScrollBar with hidden increment/decrement buttons and a purple-themed thumb, matching the existing `CustomScrollbarStyle` in App.xaml.

---

## Requirements

### Requirement 1: Navigation Bar — To Do Button Highlight

**User Story:** As an admin, I want the "To Do" navigation button to appear visually selected when the To Do tab is active, so that I can immediately see which section I am in.

#### Acceptance Criteria

1. WHEN the AdminToDoTab is the active view, THE NavBar SHALL render the "To Do" button with background color `#5e4eb7`.
2. WHEN the AdminToDoTab is the active view, THE NavBar SHALL render all other navigation buttons with background color `#a294f9`.
3. THE NavBar SHALL preserve the existing pill shape (CornerRadius 20) and font weight for all navigation buttons regardless of selection state.

---

### Requirement 2: Greeting Text

**User Story:** As an admin, I want to see a personalized greeting in the top-left of the To Do tab, so that the interface feels contextual and welcoming.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL display a TextBlock in the top-left area containing the text "Hello Admin Name! 👋".
2. THE AdminToDoTab SHALL render the greeting TextBlock in light purple (`#a294f9`) at a font size of 14 or smaller, visually subordinate to the section header.
3. WHERE the admin's display name is available from the ViewModel, THE AdminToDoTab SHALL substitute the literal "Admin Name" with the bound admin name value.

---

### Requirement 3: Section Header

**User Story:** As an admin, I want a clear section header for the To Do area, so that I can orient myself within the dashboard.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL display a TextBlock with the text "To Do" in bold white font as the primary section header.
2. THE AdminToDoTab SHALL display a secondary label "ADD TASK:" in a larger serif-style font below the primary header.
3. THE AdminToDoTab SHALL render both header elements before the Task_Form input fields.

---

### Requirement 4: Task Title Input Field

**User Story:** As an admin, I want a clearly labeled, pill-shaped text input for the task title, so that I can enter a task name with a visually consistent and accessible control.

#### Acceptance Criteria

1. THE Task_Form SHALL contain a white TextBlock label reading "Task Title: (required)" positioned directly above the title input.
2. THE Task_Form SHALL contain a PillTextBox bound to the ViewModel's `TaskTitle` property for task title entry.
3. THE Task_Form SHALL apply the PillTextBox style: background `#4a447d`, white foreground, CornerRadius 25, and sufficient horizontal padding.
4. IF the TaskTitle field is empty when Save is invoked, THEN THE ViewModel SHALL set a validation error state that the Task_Form can reflect.

---

### Requirement 5: Task Description Input Field

**User Story:** As an admin, I want a clearly labeled, pill-shaped text input for the task description, so that I can provide additional context for a task.

#### Acceptance Criteria

1. THE Task_Form SHALL contain a white TextBlock label reading "Description: (mandatory)" positioned directly above the description input.
2. THE Task_Form SHALL contain a PillTextBox bound to the ViewModel's `TaskDescription` property for description entry.
3. THE Task_Form SHALL apply the same PillTextBox style as the title field (background `#4a447d`, white foreground, CornerRadius 25).
4. THE Task_Form SHALL allow multi-line text entry in the description PillTextBox with a minimum height of 60 device-independent pixels.

---

### Requirement 6: Assignee Dropdown

**User Story:** As an admin, I want a pill-shaped dropdown to select which employee a task is assigned to, so that task ownership is clearly recorded.

#### Acceptance Criteria

1. THE Task_Form SHALL contain a PillComboBox bound to the ViewModel's `AssignedTo` property for assignee selection.
2. THE Task_Form SHALL display placeholder text "Assigned to:" inside the PillComboBox when no selection has been made.
3. THE Task_Form SHALL render a downward-arrow indicator on the right side of the PillComboBox.
4. THE Task_Form SHALL style the PillComboBox with background `#4a447d`, white foreground, and CornerRadius 25, matching the PillTextBox appearance.
5. THE Task_Form SHALL lay out the Assignee PillComboBox and the Due Date PillComboBox side-by-side in the same row.

---

### Requirement 7: Due Date Picker

**User Story:** As an admin, I want a pill-shaped date picker to set a task's due date, so that deadlines are captured alongside task details.

#### Acceptance Criteria

1. THE Task_Form SHALL contain a PillComboBox or DatePicker control bound to the ViewModel's `DueDate` property for due date selection.
2. THE Task_Form SHALL display placeholder text "Due Date:" inside the control when no date has been selected.
3. THE Task_Form SHALL render a downward-arrow indicator on the right side of the due date control.
4. THE Task_Form SHALL style the due date control with background `#4a447d`, white foreground, and CornerRadius 25, matching the PillComboBox appearance.
5. THE Task_Form SHALL lay out the Due Date control and the Assignee PillComboBox side-by-side in the same row.

---

### Requirement 8: Save and Cancel Action Buttons

**User Story:** As an admin, I want large, clearly styled Save and Cancel buttons at the bottom of the form, so that I can submit or discard a new task with a single click.

#### Acceptance Criteria

1. THE Task_Form SHALL contain two ActionButtons labeled "Save" and "Cancel" centered horizontally at the bottom of the form.
2. THE Task_Form SHALL bind the Save ActionButton to the ViewModel's `SaveTaskCommand` ICommand.
3. THE Task_Form SHALL bind the Cancel ActionButton to the ViewModel's `CancelTaskCommand` ICommand.
4. THE Task_Form SHALL style both ActionButtons with background `#8b7ed6`, bold black foreground, and a CornerRadius of at least 20 for pill appearance.
5. WHEN `SaveTaskCommand.CanExecute` returns false, THE Task_Form SHALL render the Save ActionButton in a visually disabled state.

---

### Requirement 9: Filter and Sort Utility Buttons

**User Story:** As an admin, I want Filter and Sort buttons in the top-right of the content area, so that I can quickly narrow or reorder the task list.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL display a UtilityButton labeled "FILTER ▾" in the top-right area of the content region.
2. THE AdminToDoTab SHALL display a UtilityButton labeled "SORT ▾" adjacent to the Filter button in the top-right area.
3. THE AdminToDoTab SHALL style both UtilityButtons as small, oval, semi-transparent purple controls (e.g., background `#554a9e` at 70% opacity or equivalent).
4. THE AdminToDoTab SHALL bind the Filter UtilityButton to the ViewModel's `FilterCommand` ICommand.
5. THE AdminToDoTab SHALL bind the Sort UtilityButton to the ViewModel's `SortCommand` ICommand.

---

### Requirement 10: Task List Display

**User Story:** As an admin, I want to see a scrollable list of existing tasks below the form, so that I can review all outstanding work at a glance.

#### Acceptance Criteria

1. THE Task_List SHALL display tasks bound to the ViewModel's `Tasks` ObservableCollection.
2. THE Task_List SHALL be wrapped in a ScrollViewer with vertical scrolling enabled.
3. THE Task_List ScrollViewer SHALL apply the CustomScrollBar style: hidden increment/decrement buttons and a purple-themed thumb matching the existing `CustomScrollbarStyle` in App.xaml.
4. WHEN the `Tasks` collection is empty, THE Task_List SHALL display a placeholder message such as "No tasks yet." in muted text.

---

### Requirement 11: Layout and Responsiveness

**User Story:** As an admin, I want the To Do tab to use a structured, responsive layout, so that the control adapts gracefully to different window sizes.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL use a root Grid with defined RowDefinitions and ColumnDefinitions to structure the greeting, utility buttons, form, and task list regions.
2. THE AdminToDoTab SHALL use StackPanel elements within the form region to stack labels and inputs vertically.
3. THE AdminToDoTab SHALL use a Grid or StackPanel with `Orientation="Horizontal"` to place the Assignee and Due Date controls side-by-side.
4. THE AdminToDoTab SHALL use a Grid or StackPanel with `Orientation="Horizontal"` to place the Save and Cancel buttons side-by-side and centered.
5. WHILE the AdminToDoTab width is less than 600 device-independent pixels, THE AdminToDoTab SHALL remain usable without horizontal scrolling by allowing input fields to stretch to available width.

---

### Requirement 12: Accessibility and Labeling

**User Story:** As an admin, I want all input fields to be properly labeled, so that assistive technologies can identify each control.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL set `AutomationProperties.LabeledBy` or `AutomationProperties.Name` on each input control (title TextBox, description TextBox, assignee ComboBox, due date control) referencing its corresponding label TextBlock.
2. THE AdminToDoTab SHALL set `AutomationProperties.Name` on the Save and Cancel ActionButtons with values "Save Task" and "Cancel Task" respectively.
3. THE AdminToDoTab SHALL set `AutomationProperties.Name` on the Filter and Sort UtilityButtons with values "Filter Tasks" and "Sort Tasks" respectively.

---

### Requirement 13: Placeholder Text Support

**User Story:** As an admin, I want placeholder text in the Assignee and Due Date controls, so that I understand what each field expects before I interact with it.

#### Acceptance Criteria

1. THE Task_Form SHALL display the text "Assigned to:" as placeholder content inside the Assignee PillComboBox when no item is selected.
2. THE Task_Form SHALL display the text "Due Date:" as placeholder content inside the Due Date control when no date is selected.
3. WHEN a value is selected or entered, THE Task_Form SHALL hide the placeholder text and show the selected value.

---

### Requirement 14: ViewModel Integration

**User Story:** As a developer, I want the AdminToDoTab to be structured for clean ViewModel integration, so that business logic remains separate from the view.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL expose a dependency on a ViewModel via its `DataContext`, with no business logic in the code-behind beyond UI initialization.
2. THE AdminToDoTab SHALL bind all input fields to ViewModel properties using WPF `{Binding}` expressions with `Mode=TwoWay` where appropriate.
3. THE AdminToDoTab SHALL bind all button actions to ViewModel ICommand properties using `Command="{Binding ...}"` syntax.
4. THE AdminToDoTab code-behind SHALL contain only the `InitializeComponent()` call and any strictly UI-only event handlers (e.g., placeholder text toggle).

---

### Requirement 15: Visual Style Consistency

**User Story:** As a developer, I want the AdminToDoTab to match the existing dark-purple aesthetic of the Admin Dashboard, so that the new tab integrates seamlessly with the rest of the application.

#### Acceptance Criteria

1. THE AdminToDoTab SHALL use the application-level brush resources (`DarkBackgroundBrush`, `CardBackgroundBrush`, `PurpleAccentBrush`) defined in App.xaml wherever applicable.
2. THE AdminToDoTab SHALL use `#4a447d` as the background for all PillTextBox and PillComboBox controls.
3. THE AdminToDoTab SHALL use `#8b7ed6` as the background for ActionButtons.
4. THE AdminToDoTab SHALL use `#5e4eb7` as the background for the selected "To Do" NavBar button.
5. THE AdminToDoTab SHALL use `#a294f9` as the background for unselected NavBar buttons when the To Do tab is active.
6. THE AdminToDoTab SHALL encode UTF-8 characters (including emoji such as 👋) directly in XAML without escape sequences.
