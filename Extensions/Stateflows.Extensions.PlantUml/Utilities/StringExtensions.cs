using Stateflows.Common;

namespace Stateflows
{
    public static class StringExtensions
    {
        public static string ToIdentifier(this string value)
            => value.ToShortName().Replace('<', '_').Replace('>', '_');
    }
}