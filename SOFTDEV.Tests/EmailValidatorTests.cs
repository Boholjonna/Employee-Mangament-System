// EmailValidatorTests.cs
// Property-based and example-based tests for EmailValidator.IsValidEmailFormat.
//
// Properties covered:
//   Property 6 – Pure function:       same input always returns the same result.
//   Property 7 – Invalid formats:     strings that violate the email shape return false.
//   Property 8 – Valid formats:       well-formed local@domain.tld strings return true.
//
// Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;

namespace SOFTDEV.Tests
{
    public class EmailValidatorTests
    {
        // -----------------------------------------------------------------------
        // Property 6 – Pure function
        // For any non-null string, calling IsValidEmailFormat twice must return
        // the same result both times (no hidden state, no side-effects).
        // -----------------------------------------------------------------------

        /// <summary>
        /// **Validates: Requirements 9.1**
        /// IsValidEmailFormat is a pure function: identical inputs always produce
        /// identical outputs regardless of how many times the method is called.
        /// </summary>
        [Property(DisplayName = "Property 6: IsValidEmailFormat is a pure function")]
        public bool Property6_PureFunction(NonNull<string> emailWrapper)
        {
            string email = emailWrapper.Get;
            bool first  = EmailValidator.IsValidEmailFormat(email);
            bool second = EmailValidator.IsValidEmailFormat(email);
            return first == second;
        }

        // -----------------------------------------------------------------------
        // Property 7 – Invalid formats are rejected
        // Deterministic [Theory] examples covering every rejection rule, plus one
        // FsCheck property for the "no @ at all" case.
        // -----------------------------------------------------------------------

        /// <summary>
        /// **Validates: Requirements 9.2, 9.3, 9.4, 9.5, 9.6, 9.7**
        /// Specific invalid email strings must all return false.
        /// </summary>
        [Theory(DisplayName = "Property 7: Known invalid email formats are rejected")]
        // No '@' at all
        [InlineData("notanemail")]
        // Multiple '@'
        [InlineData("a@@b.com")]
        [InlineData("a@b@c.com")]
        // Empty local part
        [InlineData("@domain.com")]
        // Domain without '.'
        [InlineData("user@domain")]
        // Domain ending with '.'
        [InlineData("user@domain.")]
        // Empty string
        [InlineData("")]
        public void Property7_InvalidFormats_AreRejected(string email)
        {
            bool result = EmailValidator.IsValidEmailFormat(email);
            Assert.False(result, $"Expected false for invalid email: \"{email}\"");
        }

        /// <summary>
        /// **Validates: Requirements 9.2**
        /// For any string that contains no '@' character, IsValidEmailFormat must
        /// return false (there is no way to split into exactly two parts).
        /// </summary>
        [Property(DisplayName = "Property 7b: Strings without '@' are always rejected")]
        public bool Property7b_NoAtSign_AlwaysRejected(NonNull<string> rawWrapper)
        {
            // Remove every '@' so the generator cannot accidentally produce a valid address.
            string email = rawWrapper.Get.Replace("@", string.Empty);
            return EmailValidator.IsValidEmailFormat(email) == false;
        }

        // -----------------------------------------------------------------------
        // Property 8 – Valid formats are accepted
        // A structured generator builds safe local@domain.tld triples where:
        //   • localPart   – non-empty, letters/digits only (no '@' or '.')
        //   • domainLabel – non-empty, letters/digits only (no '@' or '.')
        //   • tld         – non-empty, letters/digits only (no '.')
        // The resulting address must always be accepted.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates a non-empty string of lowercase letters and digits.
        /// The alphabet deliberately excludes '@' and '.' so the segments
        /// cannot accidentally break the email structure.
        /// </summary>
        private static Gen<string> SafeSegmentGen()
        {
            char[] alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

            // Pick a random character from the safe alphabet.
            Gen<char> charGen = Gen.Elements(alphabet);

            // Build a non-empty list of such characters and join them into a string.
            return Gen.NonEmptyListOf(charGen)
                      .Select(chars => string.Concat(chars));
        }

        /// <summary>
        /// **Validates: Requirements 9.1, 9.5, 9.6, 9.7**
        /// For any well-formed triple (localPart, domainLabel, tld) composed of
        /// safe characters, the address "localPart@domainLabel.tld" must be
        /// accepted by IsValidEmailFormat.
        /// </summary>
        [Property(DisplayName = "Property 8: Valid email triples are always accepted")]
        public Property Property8_ValidEmailTriples_AreAccepted()
        {
            // Compose a generator that produces (local, domain, tld) triples
            // using LINQ query syntax (SelectMany extension methods from FsCheck.Fluent).
            Gen<(string local, string domain, string tld)> tripleGen =
                from local  in SafeSegmentGen()
                from domain in SafeSegmentGen()
                from tld    in SafeSegmentGen()
                select (local, domain, tld);

            return Prop.ForAll(
                tripleGen.ToArbitrary(),
                triple =>
                {
                    string email = $"{triple.local}@{triple.domain}.{triple.tld}";
                    bool result  = EmailValidator.IsValidEmailFormat(email);
                    return result.Label($"Expected true for valid email: \"{email}\"");
                });
        }
    }
}
