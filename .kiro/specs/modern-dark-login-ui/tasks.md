# Implementation Plan: Modern Dark Login UI

## Overview

Implement the modern dark-themed login screen for the SOFTDEV WPF (.NET 10) application. The work is split into three sequential phases: (1) define all global styles in `App.xaml`, (2) build the two-column XAML layout in `MainWindow.xaml`, and (3) implement the code-behind logic in `MainWindow.xaml.cs`. Each phase builds directly on the previous one, ending with all components wired together and the application compiling cleanly.

## Tasks

- [x] 1. Define global control styles in App.xaml
  - [x] 1.1 Add `RoundedTextBoxStyle` to `App.xaml` Application.Resources
    - Create a `Style` targeting `TextBox` with `x:Key="RoundedTextBoxStyle"`
    - Set `Foreground="White"`, `CaretBrush="White"`, `FontSize="14"` via `Setter`s
    - Add a custom `ControlTemplate` containing a `Border` with `Background="#2a2a3e"`, `CornerRadius="8"`, `Padding="12,10"`
    - Place a `ScrollViewer` named `PART_ContentHost` with `Focusable="False"`, `HorizontalScrollBarVisibility="Hidden"`, `VerticalScrollBarVisibility="Hidden"` inside the `Border`
    - _Requirements: 6.1, 6.2, 14.1, 14.2_

  - [x] 1.2 Add `RoundedPasswordBoxStyle` to `App.xaml` Application.Resources
    - Create a `Style` targeting `PasswordBox` with `x:Key="RoundedPasswordBoxStyle"`
    - Mirror the same `ControlTemplate` structure as `RoundedTextBoxStyle` (Border + `PART_ContentHost` ScrollViewer)
    - _Requirements: 6.7, 6.8, 14.1_

  - [x] 1.3 Add `PillButtonStyle` to `App.xaml` Application.Resources
    - Create a `Style` targeting `Button` with `x:Key="PillButtonStyle"`
    - Set `Background="#7b61ff"`, `Foreground="White"`, `FontSize="14"`, `FontWeight="SemiBold"`, `Cursor="Hand"` via `Setter`s
    - Add a `ControlTemplate` with a `Border` named `ButtonBorder`, `CornerRadius="20"`, `Padding="0,12"`, and a `ContentPresenter` centered inside
    - Add a `Trigger` for `IsMouseOver=True` that sets `ButtonBorder.Background` to `#6a52e0`
    - Add a `Trigger` for `IsPressed=True` that sets `ButtonBorder.Background` to `#5a44cc`
    - _Requirements: 6.3, 6.4, 6.5, 14.1_

  - [x] 1.4 Add `OutlinePillButtonStyle` to `App.xaml` Application.Resources
    - Create a `Style` targeting `Button` with `x:Key="OutlinePillButtonStyle"`
    - Use a transparent background with a `BorderBrush="#7b61ff"` and `BorderThickness="1.5"`, `CornerRadius="20"`, `Foreground="#7b61ff"`, `FontSize="14"`, `FontWeight="SemiBold"`, `Cursor="Hand"`
    - Add hover and press triggers analogous to `PillButtonStyle`
    - _Requirements: 6.6, 14.1_

- [x] 2. Build the two-column XAML layout in MainWindow.xaml
  - [x] 2.1 Replace MainWindow.xaml with the root Window and two-column Grid
    - Set `Title="Login"`, `Height="600"`, `Width="900"`, `WindowStartupLocation="CenterScreen"`, `Background="#0a0a0a"`, `ResizeMode="CanResize"`
    - Add a root `Grid` with `Background="#0a0a0a"` and two `ColumnDefinition` entries: `Width="1*"` and `Width="1.2*"`
    - _Requirements: 1.1, 1.2, 1.3, 1.4_

  - [x] 2.2 Add the left decorative panel
    - Place a `Border` in `Grid.Column="0"` with `CornerRadius="25"`, `Margin="20"`, `Background="#1a1a2e"`
    - Nest an `Image` inside with `Source="/images/login-bg.png"`, `Stretch="UniformToFill"`, `Opacity="0.6"`
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

  - [x] 2.3 Add the right form container Border and inner Grid
    - Place a `Border` in `Grid.Column="1"` with `Background="#15151b"`, `CornerRadius="20"`, `Margin="20"`, `Padding="40,30"`
    - Inside the `Border`, add a `Grid` with five `RowDefinition` entries: Auto, Auto, `*`, Auto, Auto
    - _Requirements: 3.1, 3.4_

  - [x] 2.4 Add the header, subtitle, and language button to the form Grid
    - Add a `TextBlock` in `Grid.Row="0"` with `Text="Hi, there!"`, `Foreground="#7b61ff"`, `FontSize="28"`, `FontWeight="Bold"`
    - Add a `TextBlock` in `Grid.Row="1"` with `Text="Welcome to SOFTDEV Portal"`, `Foreground="#aaaaaa"`, `FontSize="14"`, `Margin="0,4,0,24"`
    - Add a `Button` named `LanguageButton` with `Content="EN"`, `HorizontalAlignment="Right"`, `VerticalAlignment="Top"`, `Style="{StaticResource PillButtonStyle}"`, `Width="50"`, `Height="30"`, `Click="LanguageButton_Click"`
    - Add a `ContextMenu` named `LanguageContextMenu` with `MenuItem`s for EN, FR, DE, ES (or similar), each with `Click="LanguageMenuItem_Click"`
    - _Requirements: 3.2, 3.3, 4.1, 4.2, 4.3_

  - [x] 2.5 Add the input fields StackPanel in Grid.Row="2"
    - Add a `StackPanel` in `Grid.Row="2"`
    - Add a `TextBlock` label `"Employee User Name"` (`Foreground="#7b61ff"`, `FontSize="12"`, `Margin="0,0,0,4"`) followed by a `TextBox` named `UsernameTextBox` with `Style="{StaticResource RoundedTextBoxStyle}"`, `Margin="0,0,0,16"`, `MaxLength="100"`
    - Add a `TextBlock` label `"Email ID"` followed by a `TextBox` named `EmailTextBox` with `Style="{StaticResource RoundedTextBoxStyle}"`, `Margin="0,0,0,16"`, `MaxLength="200"`
    - Add a `TextBlock` label `"Password (min 8 char)"` followed by a `PasswordBox` named `PasswordBox` with `Style="{StaticResource RoundedPasswordBoxStyle}"`, `Margin="0,0,0,8"`
    - Add a `TextBlock` named `ErrorMessageText` with `Foreground="#ff6b6b"`, `FontSize="12"`, `Visibility="Collapsed"`, `Margin="0,0,0,8"`
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 10.4, 13.1, 13.4_

  - [x] 2.6 Add the action buttons StackPanel in Grid.Row="3"
    - Add a `StackPanel` in `Grid.Row="3"` with `Margin="0,16,0,0"`
    - Add a `Button` with `Content="Log in"`, `Style="{StaticResource PillButtonStyle}"`, `Height="44"`, `Margin="0,0,0,12"`, `Click="LoginButton_Click"`
    - Add a `Button` with `Content="Sign in with Google"`, `Style="{StaticResource OutlinePillButtonStyle}"`, `Height="44"`, `Click="GoogleSignInButton_Click"`
    - _Requirements: 7.1, 7.2, 7.3_

  - [x] 2.7 Add the footer sign-up link in Grid.Row="4"
    - Add a `TextBlock` in `Grid.Row="4"` with `HorizontalAlignment="Center"`, `Margin="0,20,0,0"`
    - Inside the `TextBlock`, add a `Run` with `Text="Don't have an account? "` and `Foreground="#888888"`, followed by a `Hyperlink` with `Click="SignUpLink_Click"` containing a `Run` with `Text="Sign up"` and `Foreground="#7b61ff"`
    - _Requirements: 12.1, 12.2_

- [x] 3. Checkpoint — verify XAML compiles and renders
  - Build the project (`dotnet build`) and confirm zero errors
  - Visually verify the two-column layout, styled controls, and error text block are present
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement code-behind logic in MainWindow.xaml.cs
  - [x] 4.1 Add the `ValidationResult` readonly record struct
    - Declare `internal readonly record struct ValidationResult(bool IsValid, string Message)` in `MainWindow.xaml.cs` (or a separate file in the `SOFTDEV` namespace)
    - Add static factory methods `Ok()` and `Fail(string msg)`
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

  - [x] 4.2 Implement `IsValidEmailFormat(string email)`
    - Declare as `private static bool IsValidEmailFormat(string email)`
    - Split on `'@'`; return `false` if `parts.Length != 2`
    - Return `false` if `parts[0]` is empty
    - Return `false` if `parts[1]` does not contain `'.'`
    - Return `false` if `parts[1]` ends with `'.'`
    - Return `true` otherwise
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

  - [ ] 4.3 Write property tests for `IsValidEmailFormat`
    - **Property 6: Email format validation is a pure function** — same input always returns same result
    - **Property 7: Invalid email formats are rejected** — strings lacking `@`, empty local part, domain without `.`, domain ending with `.` all return `false`
    - **Property 8: Valid email formats are accepted** — strings of form `[non-empty]@[non-empty].[non-empty]` return `true`
    - Use xUnit + FsCheck (or CsCheck); add a test project targeting `net10.0-windows` if one does not exist
    - **Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7**

  - [x] 4.4 Implement `ValidateInputs()`
    - Declare as `private ValidationResult ValidateInputs()`
    - Check `string.IsNullOrWhiteSpace(UsernameTextBox.Text)` → return `Fail("Employee User Name is required.")`
    - Check `string.IsNullOrWhiteSpace(EmailTextBox.Text)` → return `Fail("Email ID is required.")`
    - Check `!IsValidEmailFormat(EmailTextBox.Text)` → return `Fail("Email ID is not a valid format.")`
    - Check `PasswordBox.Password.Length < 8` → return `Fail("Password must be at least 8 characters.")`
    - Return `ValidationResult.Ok()`
    - Do NOT modify any control's content
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7_

  - [ ]* 4.5 Write property tests for `ValidateInputs`
    - **Property 1: Validation totality** — for any non-null username/email/password, `ValidateInputs` never throws
    - **Property 2: Whitespace inputs are rejected** — whitespace-only username or email always returns `Fail`
    - **Property 3: Short passwords are rejected** — password length < 8 (with valid username and email) always returns `Fail`
    - **Property 4: Valid inputs always pass** — non-whitespace username, valid email, password ≥ 8 chars always returns `Ok`
    - **Property 5: Validator is side-effect-free** — control text properties are unchanged after calling `ValidateInputs`
    - **Validates: Requirements 8.1, 8.2, 8.4, 8.5, 8.7, 8.8**

  - [x] 4.6 Implement `LoginButton_Click` event handler
    - Set `ErrorMessageText.Visibility = Visibility.Collapsed` at the start of the handler
    - Call `ValidateInputs()`; if `!result.IsValid`, set `ErrorMessageText.Text = result.Message` and `ErrorMessageText.Visibility = Visibility.Visible`, then return
    - Read `UsernameTextBox.Text`, `EmailTextBox.Text`, and `PasswordBox.Password` exactly once into local variables
    - Do NOT assign `PasswordBox.Password` to any field or property
    - Add a `// TODO: Pass to AuthService` comment placeholder
    - _Requirements: 8.1–8.8, 10.1, 10.2, 10.3, 11.1, 11.2, 13.3_

  - [ ]* 4.7 Write property test for `LoginButton_Click` error visibility
    - **Property 9: Error visibility reflects validation outcome** — `ErrorMessageText.Visibility` is `Collapsed` at start; becomes `Visible` iff `ValidateInputs` returns `Fail`
    - Use WPF UI thread dispatcher or extract testable logic into a pure helper
    - **Validates: Requirements 10.1, 10.2, 10.3**

  - [x] 4.8 Implement `LanguageButton_Click` and `LanguageMenuItem_Click` handlers
    - `LanguageButton_Click`: set `LanguageContextMenu.IsOpen = true`
    - `LanguageMenuItem_Click`: cast `sender` to `MenuItem`; set `LanguageButton.Content = item.Header?.ToString() ?? "EN"`
    - _Requirements: 4.3, 4.4_

  - [x] 4.9 Implement `GoogleSignInButton_Click` and `SignUpLink_Click` placeholder handlers
    - `GoogleSignInButton_Click`: add `// TODO: OAuth2 Google sign-in flow` — must not throw
    - `SignUpLink_Click`: add `// TODO: Navigate to registration window` — must not throw
    - _Requirements: 11.3, 12.2_

- [ ] 5. Checkpoint — build and smoke-test the complete application
  - Run `dotnet build` from the `SOFTDEV` folder and confirm zero errors and zero warnings
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 6. Wire everything together and final validation
  - [ ] 6.1 Verify `App.xaml` resource keys match all `StaticResource` references in `MainWindow.xaml`
    - Confirm `RoundedTextBoxStyle`, `RoundedPasswordBoxStyle`, `PillButtonStyle`, `OutlinePillButtonStyle` are all defined and referenced correctly
    - _Requirements: 6.1, 6.3, 6.6, 6.7, 14.1_

  - [ ] 6.2 Verify `MaxLength` is set on `UsernameTextBox` and `EmailTextBox`
    - Confirm `MaxLength` attribute is present on both controls in `MainWindow.xaml`
    - _Requirements: 13.4_

  - [ ]* 6.3 Write property test for proportional column layout invariant
    - **Property 11: Proportional column layout invariant** — for any window width > 0, the right column's rendered width is greater than the left column's rendered width
    - Use WPF layout measurement APIs (`UIElement.Measure` / `UIElement.Arrange`) in a test
    - **Validates: Requirements 1.3, 1.4**

  - [ ]* 6.4 Write property test for language button label update
    - **Property 12: Language button label reflects selection** — for any language code string set as a `MenuItem` header, selecting that item updates `LanguageButton.Content` to that string
    - **Validates: Requirements 4.4**

- [ ] 7. Final checkpoint — Ensure all tests pass
  - Run the full test suite (if a test project was created) and confirm all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP
- Property tests require adding a separate xUnit + FsCheck (or CsCheck) test project; skip if no test infrastructure is desired
- Each task references specific requirements for traceability
- The `PART_ContentHost` `ScrollViewer` is a WPF contract — omitting it will cause a runtime `InvalidOperationException`
- `PasswordBox.Password` must only be read at the moment of submission and never stored in a field (Requirements 11.2, 13.3)
- The `/images/login-bg.png` asset should be added to `SOFTDEV/images/` with `Build Action = Resource`; the app renders gracefully without it
