using System.Linq;

namespace Stateflows.Common
{
    public static class StringExtensions
    {
        public static string ToShortName(this string value)
            => value.Contains('<')
                ? $"{value.Split('<').First().Split('.').Last()}<{value.Split('<').Last().Split('.').Last()}"
                : value.Split('.').Last();

        public static string ToCamelCase(this string value)
            => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(value);
    }
}