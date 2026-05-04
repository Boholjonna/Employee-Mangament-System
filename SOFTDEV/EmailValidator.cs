namespace SOFTDEV
{
    /// <summary>
    /// Provides email-format validation logic, extracted so it can be
    /// tested independently of the WPF MainWindow.
    /// </summary>
    internal static class EmailValidator
    {
        /// <summary>
        /// Returns <c>true</c> when <paramref name="email"/> has the shape
        /// <c>local@domain.tld</c> (non-empty local part, domain that contains
        /// a dot and does not end with one).
        /// </summary>
        public static bool IsValidEmailFormat(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2)              return false;
            if (string.IsNullOrEmpty(parts[0])) return false;
            if (!parts[1].Contains('.'))        return false;
            if (parts[1].EndsWith('.'))         return false;
            return true;
        }
    }
}
