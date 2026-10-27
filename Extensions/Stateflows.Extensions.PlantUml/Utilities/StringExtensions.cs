using Stateflows.Common;

namespace Stateflows
{
    /// <summary>Provides identifier formatting helpers used by diagram generation.</summary>
    public static class StringExtensions
    {
        /// <summary>Converts a type-like name to a short identifier and replaces generic delimiters with underscores.</summary>
        /// <param name="value">The source string to format.</param>
        /// <returns>The short name formatted as an identifier.</returns>
        public static string ToIdentifier(this string value)
            => value.ToShortName().Replace('<', '_').Replace('>', '_');
    }
}