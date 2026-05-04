namespace SOFTDEV;

/// <summary>
/// Represents a single row in the Employee List.
/// </summary>
public record EmployeeEntry
{
    /// <summary>Gets the employee's display name.</summary>
    public string Name { get; init; }

    /// <summary>Gets the employee's job position.</summary>
    public string Position { get; init; }

    /// <summary>
    /// Initializes a new <see cref="EmployeeEntry"/>.
    /// <c>null</c> values for <paramref name="name"/> or <paramref name="position"/>
    /// are replaced with <see cref="string.Empty"/>.
    /// </summary>
    public EmployeeEntry(string name, string position)
    {
        Name = name ?? string.Empty;
        Position = position ?? string.Empty;
    }
}
