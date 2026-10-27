using System.Linq;

namespace Stateflows.Common
{
    /// <summary>Provides naming transformations used by Stateflows identifiers and integrations.</summary>
    public static class StringExtensions
    {
        /// <summary>Removes namespace prefixes from a name using simple dot and generic-delimiter splitting.</summary>
        /// <param name="value">The name to shorten.</param>
        /// <returns>The last dotted segment, with the same simple transformation applied after a generic delimiter.</returns>
        /// <remarks>This helper is not a general parser for nested or multi-argument CLR generic names.</remarks>
        public static string ToShortName(this string value)
            => value.Contains('<')
                ? $"{value.Split('<').First().Split('.').Last()}<{value.Split('<').Last().Split('.').Last()}"
                : value.Split('.').Last();
        
        /// <summary>Transforms a name using the System.Text.Json camel-case naming policy.</summary>
        /// <param name="value">The name to transform.</param>
        /// <returns>The camel-case name.</returns>
        public static string ToCamelCase(this string value)
            => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(value);
        
        /// <summary>Transforms a name to lower snake case after replacing dots and hyphens with underscores.</summary>
        /// <param name="value">The name to transform.</param>
        /// <returns>The normalized lower snake-case name.</returns>
        public static string ToSnakeCase(this string value)
            => System.Text.Json.JsonNamingPolicy.SnakeCaseLower.ConvertName(value.Replace('.', '_').Replace('-', '_'));
    }
}