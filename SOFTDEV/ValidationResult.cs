namespace SOFTDEV
{
    /// <summary>
    /// Carries the outcome of form-field validation.
    /// IsValid = true  → all fields passed.
    /// IsValid = false → Message contains the first failure reason.
    /// </summary>
    internal readonly record struct ValidationResult(bool IsValid, string Message)
    {
        public static ValidationResult Ok()             => new(true,  string.Empty);
        public static ValidationResult Fail(string msg) => new(false, msg);
    }
}
