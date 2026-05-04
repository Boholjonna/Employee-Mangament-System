# Requirements Document

## Introduction

This document defines the requirements for the Modern Dark Login UI feature of the SOFTDEV WPF application (.NET 10). The feature replaces the placeholder `MainWindow.xaml` with a polished, dark-themed login screen split into a decorative left panel and a functional right-side form. All visual styling is achieved through native WPF `ControlTemplate`s and `Style` resources defined in `App.xaml`, with no third-party UI libraries required. The requirements are derived from the approved design document.

---

## Glossary

- **LoginWindow**: The `MainWindow` WPF window that hosts the login UI.
- **Validator**: The `ValidateInputs()` method (and supporting `IsValidEmailFormat()`) in `MainWindow.xaml.cs` responsible for checking form field values.
- **ValidationResult**: The `readonly record struct` that carries an `IsValid` flag and an optional error `Message`.
- **PasswordBox**: The WPF `PasswordBox` control used to collect the user's password without exposing it in the visual tree.
- **RoundedTextBoxStyle**: The `Style` resource keyed `RoundedTextBoxStyle` in `App.xaml` that applies a custom `ControlTemplate` with rounded corners to `TextBox` controls.
- **RoundedPasswordBoxStyle**: The `Style` resource keyed `RoundedPasswordBoxStyle` in `App.xaml` that applies a custom `ControlTemplate` with rounded corners to `PasswordBox` controls.
- **PillButtonStyle**: The `Style` resource keyed `PillButtonStyle` in `App.xaml` that applies a fully-rounded pill shape and hover/press triggers to `Button` controls.
- **OutlinePillButtonStyle**: The `Style` resource keyed `OutlinePillButtonStyle` in `App.xaml` that applies an outlined pill shape to secondary `Button` controls.
- **LanguageButton**: The `Button` control named `LanguageButton` that displays the current locale code and opens the language `ContextMenu`.
- **ErrorMessageText**: The `TextBlock` named `ErrorMessageText` that displays inline validation error messages on the form.
- **AuthService**: A future authentication service that will receive validated credentials from the code-behind.
- **PART_ContentHost**: The required `ScrollViewer` named `PART_ContentHost` that must be present inside any custom `ControlTemplate` for `TextBox` or `PasswordBox` controls.

---

## Requirements

### Requirement 1: Two-Column Layout

**User Story:** As a user, I want to see a visually structured login screen, so that I can clearly distinguish the decorative branding area from the interactive form area.

#### Acceptance Criteria

1. THE LoginWindow SHALL render a root `Grid` with two `ColumnDefinition` entries using star-sizing widths of `1*` and `1.2*`.
2. THE LoginWindow SHALL set the root `Grid` background to `#0a0a0a`.
3. WHILE the LoginWindow is displayed, THE LoginWindow SHALL ensure the right form column is always wider than the left decorative column regardless of window width.
4. WHEN the LoginWindow is resized, THE LoginWindow SHALL maintain the proportional `1:1.2` column ratio between the decorative panel and the form container.

---

### Requirement 2: Left Decorative Panel

**User Story:** As a user, I want to see a branded decorative panel on the left side of the login screen, so that the application feels polished and professional.

#### Acceptance Criteria

1. THE LoginWindow SHALL render a `Border` in the left column with `CornerRadius="25"`, `Margin="20"`, and `Background="#1a1a2e"`.
2. THE LoginWindow SHALL display an `Image` inside the left `Border` with `Source="/images/login-bg.png"`, `Stretch="UniformToFill"`, and `Opacity="0.6"`.
3. IF the image asset `/images/login-bg.png` is not found, THEN THE LoginWindow SHALL render the left `Border` using its `Background` color only without throwing an exception.
4. THE left decorative panel SHALL NOT participate in any form logic or user input handling.

---

### Requirement 3: Right Form Container

**User Story:** As a user, I want to interact with a clearly defined form area on the right side of the screen, so that I can enter my credentials comfortably.

#### Acceptance Criteria

1. THE LoginWindow SHALL render a `Border` in the right column with `Background="#15151b"`, `CornerRadius="20"`, `Margin="20"`, and `Padding="40,30"`.
2. THE LoginWindow SHALL display the header text "Hi, there!" in `Foreground="#7b61ff"`, `FontSize="28"`, and `FontWeight="Bold"` within the form container.
3. THE LoginWindow SHALL display the subtitle text "Welcome to SOFTDEV Portal" in `Foreground="#aaaaaa"` and `FontSize="14"` below the header.
4. THE LoginWindow SHALL arrange form content in a `Grid` with five row definitions: Auto (header), Auto (subtitle), `*` (input fields), Auto (buttons), Auto (footer).

---

### Requirement 4: Language Dropdown

**User Story:** As a user, I want to select my preferred language from the login screen, so that I can use the application in my language.

#### Acceptance Criteria

1. THE LoginWindow SHALL display the `LanguageButton` aligned to the top-right of the form container, styled with `PillButtonStyle`, with `Width="50"` and `Height="30"`.
2. THE LanguageButton SHALL display the current locale code (default: "EN") as its content.
3. WHEN the user clicks the `LanguageButton`, THE LoginWindow SHALL open the associated `ContextMenu` (`LanguageContextMenu.IsOpen = true`).
4. WHEN the user selects a language `MenuItem`, THE LoginWindow SHALL update the `LanguageButton` content to the selected language code.

---

### Requirement 5: Input Fields

**User Story:** As a user, I want clearly labeled input fields for my username, email, and password, so that I know exactly what information to enter.

#### Acceptance Criteria

1. THE LoginWindow SHALL display a `TextBox` named `UsernameTextBox` with a preceding label "Employee User Name" in `Foreground="#7b61ff"` and `FontSize="12"`.
2. THE LoginWindow SHALL display a `TextBox` named `EmailTextBox` with a preceding label "Email ID" in `Foreground="#7b61ff"` and `FontSize="12"`.
3. THE LoginWindow SHALL display a `PasswordBox` named `PasswordBox` with a preceding label "Password (min 8 char)" in `Foreground="#7b61ff"` and `FontSize="12"`.
4. THE LoginWindow SHALL apply `RoundedTextBoxStyle` to `UsernameTextBox` and `EmailTextBox`.
5. THE LoginWindow SHALL apply `RoundedPasswordBoxStyle` to the `PasswordBox`.
6. THE LoginWindow SHALL stack each label-input pair vertically with a bottom margin of `16` between pairs.

---

### Requirement 6: Styled Controls (App.xaml Resources)

**User Story:** As a developer, I want all control styles defined as global resources in `App.xaml`, so that they can be reused across future windows without duplication.

#### Acceptance Criteria

1. THE LoginWindow SHALL define `RoundedTextBoxStyle` in `App.xaml` as a `Style` targeting `TextBox` with a custom `ControlTemplate` that includes a `Border` with `CornerRadius="8"`, `Background="#2a2a3e"`, and `Padding="12,10"`.
2. THE `RoundedTextBoxStyle` ControlTemplate SHALL include a `ScrollViewer` named `PART_ContentHost` as the content host inside the `Border`.
3. THE LoginWindow SHALL define `PillButtonStyle` in `App.xaml` as a `Style` targeting `Button` with `Background="#7b61ff"`, `Foreground="White"`, `FontSize="14"`, `FontWeight="SemiBold"`, and a `ControlTemplate` with `CornerRadius="20"`.
4. THE `PillButtonStyle` ControlTemplate SHALL include a `Trigger` for `IsMouseOver=True` that sets the border background to `#6a52e0`.
5. THE `PillButtonStyle` ControlTemplate SHALL include a `Trigger` for `IsPressed=True` that sets the border background to `#5a44cc`.
6. THE LoginWindow SHALL define `OutlinePillButtonStyle` in `App.xaml` as a `Style` targeting `Button` with an outlined pill shape for secondary actions.
7. THE LoginWindow SHALL define `RoundedPasswordBoxStyle` in `App.xaml` as a `Style` targeting `PasswordBox` with a custom `ControlTemplate` that includes a `ScrollViewer` named `PART_ContentHost`.
8. IF a custom `ControlTemplate` for `TextBox` or `PasswordBox` omits the `ScrollViewer` named `PART_ContentHost`, THEN THE LoginWindow SHALL throw an `InvalidOperationException` at runtime when the control is rendered.

---

### Requirement 7: Action Buttons

**User Story:** As a user, I want clearly visible action buttons to log in or sign in with Google, so that I can initiate the authentication process.

#### Acceptance Criteria

1. THE LoginWindow SHALL display a "Log in" `Button` styled with `PillButtonStyle`, `Height="44"`, that triggers `LoginButton_Click` on click.
2. THE LoginWindow SHALL display a "Sign in with Google" `Button` styled with `OutlinePillButtonStyle`, `Height="44"`, that triggers `GoogleSignInButton_Click` on click.
3. THE LoginWindow SHALL stack the "Log in" button above the "Sign in with Google" button with a bottom margin of `12` between them.

---

### Requirement 8: Input Validation

**User Story:** As a user, I want the login form to validate my input before submission, so that I receive clear feedback when I have entered incorrect or incomplete information.

#### Acceptance Criteria

1. WHEN the user clicks "Log in" with an empty or whitespace-only `UsernameTextBox`, THE Validator SHALL return `ValidationResult.Fail("Employee User Name is required.")`.
2. WHEN the user clicks "Log in" with an empty or whitespace-only `EmailTextBox`, THE Validator SHALL return `ValidationResult.Fail("Email ID is required.")`.
3. WHEN the user clicks "Log in" with an `EmailTextBox` value that does not match the pattern `[non-empty]@[non-empty].[non-empty]`, THE Validator SHALL return `ValidationResult.Fail("Email ID is not a valid format.")`.
4. WHEN the user clicks "Log in" with a `PasswordBox` value of fewer than 8 characters, THE Validator SHALL return `ValidationResult.Fail("Password must be at least 8 characters.")`.
5. WHEN all fields satisfy their constraints, THE Validator SHALL return `ValidationResult.Ok()`.
6. THE Validator SHALL evaluate fields in the order: username → email → email format → password, returning on the first failure.
7. THE Validator SHALL NOT modify the content of any input control.
8. FOR ALL combinations of username, email, and password inputs, THE Validator SHALL return either `ValidationResult.Ok()` or `ValidationResult.Fail(message)` and SHALL NOT throw an exception.

---

### Requirement 9: Email Format Validation

**User Story:** As a user, I want the system to check that my email address is properly formatted, so that I am notified of typos before attempting to log in.

#### Acceptance Criteria

1. THE Validator SHALL implement `IsValidEmailFormat(string email)` as a pure, side-effect-free function.
2. WHEN `email` does not contain exactly one `@` character, THE Validator SHALL return `false`.
3. WHEN the local part (before `@`) is empty, THE Validator SHALL return `false`.
4. WHEN the domain part (after `@`) does not contain a `.` character, THE Validator SHALL return `false`.
5. WHEN the domain part ends with `.`, THE Validator SHALL return `false`.
6. WHEN `email` satisfies all format rules, THE Validator SHALL return `true`.
7. FOR ALL non-null email strings, THE Validator SHALL return the same result for the same input on every invocation (referential transparency).

---

### Requirement 10: Error Message Display

**User Story:** As a user, I want to see a clear inline error message when my login attempt fails validation, so that I know what to correct.

#### Acceptance Criteria

1. WHEN `LoginButton_Click` is invoked, THE LoginWindow SHALL set `ErrorMessageText.Visibility` to `Collapsed` before calling `ValidateInputs()`.
2. WHEN `ValidateInputs()` returns `Fail`, THE LoginWindow SHALL set `ErrorMessageText.Text` to the failure message and set `ErrorMessageText.Visibility` to `Visible`.
3. WHEN `ValidateInputs()` returns `Ok`, THE LoginWindow SHALL keep `ErrorMessageText.Visibility` as `Collapsed`.
4. THE `ErrorMessageText` TextBlock SHALL use `Foreground="#ff6b6b"` and `FontSize="12"`.

---

### Requirement 11: Login Submission Flow

**User Story:** As a user, I want my credentials to be passed to the authentication service after successful validation, so that I can access the application.

#### Acceptance Criteria

1. WHEN `ValidateInputs()` returns `Ok`, THE LoginWindow SHALL read `UsernameTextBox.Text`, `EmailTextBox.Text`, and `PasswordBox.Password` exactly once and pass them to the authentication flow.
2. THE LoginWindow SHALL NOT store `PasswordBox.Password` in any field, property, or log output.
3. WHEN `GoogleSignInButton_Click` is invoked, THE LoginWindow SHALL provide a placeholder handler for a future OAuth2 Google sign-in flow without throwing an exception.

---

### Requirement 12: Footer Sign-Up Link

**User Story:** As a new user, I want a sign-up link at the bottom of the login form, so that I can navigate to the registration screen.

#### Acceptance Criteria

1. THE LoginWindow SHALL display a centered `TextBlock` at the bottom of the form containing the text "Don't have an account? " in `Foreground="#888888"` followed by a `Hyperlink` with the text "Sign up" in `Foreground="#7b61ff"`.
2. WHEN the user clicks the "Sign up" `Hyperlink`, THE LoginWindow SHALL invoke `SignUpLink_Click` to navigate to the registration window (placeholder for future implementation).

---

### Requirement 13: Security Constraints

**User Story:** As a security-conscious developer, I want the login UI to follow secure credential-handling practices, so that user passwords are not inadvertently exposed.

#### Acceptance Criteria

1. THE LoginWindow SHALL use a `PasswordBox` control (not a `TextBox`) for password entry to prevent the password from appearing in the WPF visual tree or accessibility APIs.
2. THE LoginWindow SHALL NOT cache or persist credentials between sessions.
3. THE LoginWindow SHALL read `PasswordBox.Password` only at the moment of login submission and SHALL NOT assign it to any persistent field.
4. WHERE `MaxLength` is configurable on input controls, THE LoginWindow SHALL set `MaxLength` on `UsernameTextBox` and `EmailTextBox` to prevent excessively long inputs.

---

### Requirement 14: No Third-Party UI Dependencies

**User Story:** As a developer, I want the login UI to be implemented without third-party UI libraries, so that the project remains lightweight and avoids external licensing concerns.

#### Acceptance Criteria

1. THE LoginWindow SHALL achieve all visual styling exclusively through native WPF `ControlTemplate`s, `Style` resources, and `Trigger`s defined in `App.xaml` and `MainWindow.xaml`.
2. THE LoginWindow SHALL NOT introduce any NuGet packages for UI rendering (e.g., MahApps.Metro, MaterialDesignThemes).
