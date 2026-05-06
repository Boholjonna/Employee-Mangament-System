// AdminToDoViewModelTests.cs
// Property-based tests for AdminToDoViewModel using FsCheck.
//
// Properties covered:
//   Property 1 — AdminName reflects constructor input    (Requirement 2.3)
//   Property 2 — CanExecute false for empty/whitespace   (Requirements 4.4, 8.5)
//   Property 3 — Save adds exactly one item              (Requirement 10.1)
//   Property 4 — Cancel resets all form fields           (Requirements 8.3, 14.2)
//
// AdminToDoViewModel is a pure C# class (no WPF window dependency), so no STA
// thread is required for any of these tests.
// Each property runs a minimum of 100 FsCheck iterations.

using System.Linq;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using SOFTDEV.ViewModels;

namespace SOFTDEV.Tests
{
    public class AdminToDoViewModelTests
    {
        // -----------------------------------------------------------------------
        // Property 1 — AdminName reflects constructor input
        // Feature: admin-todo-tab, Property 1: AdminName binding reflects constructor input
        //
        // For any non-null string passed as adminName to the AdminToDoViewModel
        // constructor, the AdminName property SHALL equal that string.
        //
        // Validates: Requirement 2.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any non-null string, constructing AdminToDoViewModel(name) and reading
        /// AdminName must return the original string unchanged.
        /// Minimum 100 FsCheck iterations.
        ///
        /// FsCheck auto-generates <see cref="NonNull{T}"/> values when used as a
        /// method parameter — no explicit Arb.Default needed.
        ///
        /// <b>Validates: Requirement 2.3</b>
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 1: AdminName binding reflects constructor input")]
        public bool Property1_AdminName_ReflectsConstructorInput(NonNull<string> name)
        {
            // Feature: admin-todo-tab, Property 1: AdminName binding reflects constructor input
            var vm = new AdminToDoViewModel(name.Get);
            return vm.AdminName == name.Get;
        }

        // -----------------------------------------------------------------------
        // Property 2 — SaveTaskCommand.CanExecute is false for empty or whitespace TaskTitle
        // Feature: admin-todo-tab, Property 2: SaveTaskCommand.CanExecute is false for empty or whitespace TaskTitle
        //
        // For any string that is null, empty, or composed entirely of whitespace,
        // setting TaskTitle to that value SHALL cause SaveTaskCommand.CanExecute
        // to return false.
        //
        // Validates: Requirements 4.4, 8.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any null/empty/whitespace TaskTitle, SaveTaskCommand.CanExecute must
        /// return false.  Non-whitespace inputs are vacuously accepted (skipped).
        /// Minimum 100 FsCheck iterations.
        ///
        /// <b>Validates: Requirements 4.4, 8.5</b>
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 2: CanExecute false for empty or whitespace TaskTitle")]
        public Property Property2_CanExecute_FalseForWhitespaceTitle()
        {
            // Feature: admin-todo-tab, Property 2: SaveTaskCommand.CanExecute is false for empty or whitespace TaskTitle
            // Use a generator that produces null, empty, and whitespace-only strings.
            // FsCheck's default string generator already includes null and empty;
            // we filter to only the whitespace/null/empty cases and test those.
            Gen<string?> whitespaceGen =
                Gen.OneOf(
                    Gen.Constant<string?>(null),
                    Gen.Constant<string?>(string.Empty),
                    // Generate strings of 1–10 space/tab/newline characters
                    Gen.Choose(1, 10)
                       .SelectMany(len =>
                           Gen.Elements(' ', '\t', '\n', '\r')
                              .ListOf(len)
                              .Select(chars => (string?)string.Concat(chars))));

            return Prop.ForAll(
                whitespaceGen.ToArbitrary(),
                title =>
                {
                    var vm = new AdminToDoViewModel("Admin");
                    vm.TaskTitle = title ?? string.Empty;

                    bool canExecute = vm.SaveTaskCommand.CanExecute(null);
                    return (!canExecute)
                        .Label($"Expected CanExecute == false for TaskTitle " +
                               $"\"{title?.Replace("\n", "\\n").Replace("\r", "\\r") ?? "(null)"}\", " +
                               $"but got true.");
                });
        }

        // -----------------------------------------------------------------------
        // Property 3 — Saving a valid task adds exactly one item to the Tasks collection
        // Feature: admin-todo-tab, Property 3: Saving a valid task adds exactly one item to the Tasks collection
        //
        // For any AdminToDoViewModel with a non-empty TaskTitle, invoking
        // SaveTaskCommand.Execute SHALL increase Tasks.Count by exactly 1, and the
        // newly added item SHALL have a Title equal to the trimmed TaskTitle value.
        //
        // Validates: Requirement 10.1
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any non-whitespace TaskTitle, executing SaveTaskCommand must add exactly
        /// one item to Tasks, and that item's Title must equal the trimmed input.
        /// Minimum 100 FsCheck iterations.
        ///
        /// FsCheck auto-generates <see cref="NonWhiteSpaceString"/> values when used
        /// as a method parameter.
        ///
        /// <b>Validates: Requirement 10.1</b>
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 3: Save adds exactly one item with correct trimmed Title")]
        public bool Property3_Save_AddsExactlyOneItemWithCorrectTitle(NonWhiteSpaceString title)
        {
            // Feature: admin-todo-tab, Property 3: Saving a valid task adds exactly one item to the Tasks collection
            var vm = new AdminToDoViewModel("Admin");
            vm.TaskTitle = title.Get;

            int countBefore = vm.Tasks.Count;
            vm.SaveTaskCommand.Execute(null);

            bool countIncreased = vm.Tasks.Count == countBefore + 1;
            bool titleMatches   = vm.Tasks.Last().Title == title.Get.Trim();

            return countIncreased && titleMatches;
        }

        // -----------------------------------------------------------------------
        // Property 4 — CancelTaskCommand resets all form fields
        // Feature: admin-todo-tab, Property 4: CancelTaskCommand resets all form fields
        //
        // For any AdminToDoViewModel with arbitrary values in TaskTitle,
        // TaskDescription, AssignedTo, and DueDate, invoking CancelTaskCommand.Execute
        // SHALL set TaskTitle, TaskDescription, and AssignedTo to string.Empty and
        // DueDate to null.
        //
        // Validates: Requirements 8.3, 14.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any combination of form field values, executing CancelTaskCommand must
        /// reset TaskTitle, TaskDescription, and AssignedTo to string.Empty and
        /// DueDate to null.
        /// Minimum 100 FsCheck iterations.
        ///
        /// <b>Validates: Requirements 8.3, 14.2</b>
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 4: Cancel resets all form fields")]
        public Property Property4_Cancel_ResetsAllFormFields()
        {
            // Feature: admin-todo-tab, Property 4: CancelTaskCommand resets all form fields
            // Compose a generator for (title, desc, assignee, dueDate?) using SelectMany.
            // FsCheck's default string generator produces null, empty, and arbitrary strings.
            Gen<string?> stringOrNullGen =
                Gen.OneOf(
                    Gen.Constant<string?>(null),
                    Gen.Constant<string?>(string.Empty),
                    Gen.NonEmptyListOf(Gen.Choose(32, 126).Select(c => (char)c))
                       .Select(chars => (string?)string.Concat(chars)));

            var gen =
                stringOrNullGen.SelectMany(title =>
                    stringOrNullGen.SelectMany(desc =>
                        stringOrNullGen.SelectMany(assignee =>
                            stringOrNullGen.Select(due =>
                                (title, desc, assignee, due)))));

            return Prop.ForAll(
                gen.ToArbitrary(),
                tuple =>
                {
                    var (title, desc, assignee, due) = tuple;

                    var vm = new AdminToDoViewModel("Admin");
                    vm.TaskTitle       = title    ?? string.Empty;
                    vm.TaskDescription = desc     ?? string.Empty;
                    vm.AssignedTo      = assignee ?? string.Empty;
                    vm.DueDate         = due;   // may be null — tests the null reset path

                    vm.CancelTaskCommand.Execute(null);

                    bool titleReset    = vm.TaskTitle       == string.Empty;
                    bool descReset     = vm.TaskDescription == string.Empty;
                    bool assigneeReset = vm.AssignedTo      == string.Empty;
                    bool dueDateReset  = vm.DueDate         == null;

                    return (titleReset && descReset && assigneeReset && dueDateReset)
                        .Label($"After Cancel: TaskTitle=\"{vm.TaskTitle}\", " +
                               $"TaskDescription=\"{vm.TaskDescription}\", " +
                               $"AssignedTo=\"{vm.AssignedTo}\", " +
                               $"DueDate={vm.DueDate ?? "(null)"}.");
                });
        }
    }
}
