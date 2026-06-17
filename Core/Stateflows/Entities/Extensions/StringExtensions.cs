namespace Stateflows.Entities;

internal static class StringExtensions
{
    internal static string GetFieldKey(this string fieldName) => $"$field:{fieldName}";
    internal static string StripFieldKey(this string fieldKey) => fieldKey.StartsWith("$field:")
        ? fieldKey[7..]
        : fieldKey;
    internal static string GetProjectionKey(this string projectionName) => $"$projection:{projectionName}";
    internal static string StripProjectionKey(this string projectionKey) => projectionKey.StartsWith("$projection:")
        ? projectionKey[12..]
        : projectionKey;
}