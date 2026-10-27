using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;

namespace Stateflows.Events.Json;

internal static class EventJsonValidation
{
    internal static ValidationAttribute Create(string type, JsonElement options)
    {
        switch (type)
        {
            case "Required":
                return new RequiredAttribute { AllowEmptyStrings = EventJson.Read<RequiredOptions>(options).AllowEmptyStrings };
            case "StringLength":
                var length = EventJson.Read<StringLengthOptions>(options);
                EventJson.Require(length.MaximumLength.HasValue && length.MinimumLength >= 0 &&
                    length.MaximumLength >= length.MinimumLength, "StringLength requires valid minimumLength/maximumLength.");
                return new StringLengthAttribute(length.MaximumLength!.Value) { MinimumLength = length.MinimumLength };
            case "MinLength":
            case "MaxLength":
                var size = EventJson.Read<LengthOptions>(options);
                EventJson.Require(size.Length >= 0, $"{type} requires a nonnegative length.");
                return type == "MinLength" ? new MinLengthAttribute(size.Length!.Value) : new MaxLengthAttribute(size.Length!.Value);
            case "Range":
                var range = EventJson.Read<RangeOptions>(options);
                var operand = range.OperandType switch
                {
                    "Int32" => typeof(int), "Int64" => typeof(long),
                    "Decimal" => typeof(decimal), "Double" => typeof(double),
                    _ => throw new JsonException("Range operandType must be Int32, Int64, Decimal or Double.")
                };
                var minimum = Limit(range.Minimum, operand);
                var maximum = Limit(range.Maximum, operand);
                EventJson.Require(((IComparable)minimum).CompareTo(maximum) <= 0, "Range minimum exceeds maximum.");
                return new RangeAttribute(operand, Convert.ToString(minimum, CultureInfo.InvariantCulture)!,
                    Convert.ToString(maximum, CultureInfo.InvariantCulture)!)
                {
                    ParseLimitsInInvariantCulture = true,
                    ConvertValueInInvariantCulture = true
                };
            case "RegularExpression":
                var regex = EventJson.Read<RegexOptions>(options);
                EventJson.Require(!string.IsNullOrEmpty(regex.Pattern) && regex.MatchTimeoutInMilliseconds > 0,
                    "RegularExpression requires a nonempty pattern and a positive timeout.");
                try { _ = new System.Text.RegularExpressions.Regex(regex.Pattern!,
                    System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromMilliseconds(regex.MatchTimeoutInMilliseconds)); }
                catch (ArgumentException exception) { throw new JsonException("Invalid regular expression.", exception); }
                return new RegularExpressionAttribute(regex.Pattern!) { MatchTimeoutInMilliseconds = regex.MatchTimeoutInMilliseconds };
            case "Compare":
                var compare = EventJson.Read<CompareOptions>(options);
                EventJson.Require(!string.IsNullOrWhiteSpace(compare.OtherProperty), "Compare requires otherProperty.");
                return new CompareAttribute(compare.OtherProperty!);
            case "EmailAddress": Empty(options); return new EmailAddressAttribute();
            case "Url": Empty(options); return new UrlAttribute();
            case "Phone": Empty(options); return new PhoneAttribute();
            case "CreditCard": Empty(options); return new CreditCardAttribute();
            default: throw new JsonException($"Unknown validation type '{type}'.");
        }
    }

    private static void Empty(JsonElement options) => EventJson.Read<EmptyOptions>(options);

    private static object Limit(JsonElement value, Type type)
    {
        EventJson.Require(value.ValueKind == JsonValueKind.Number, "Range limits must be JSON numbers.");
        try
        {
            if (type == typeof(int)) return value.GetInt32();
            if (type == typeof(long)) return value.GetInt64();
            if (type == typeof(decimal)) return value.GetDecimal();
            var number = value.GetDouble();
            EventJson.Require(double.IsFinite(number), "Range limits must be finite.");
            return number;
        }
        catch (FormatException exception) { throw new JsonException("Range limit is outside the operand type's bounds.", exception); }
    }

    private sealed class EmptyOptions;
    private sealed class RequiredOptions { public bool AllowEmptyStrings { get; set; } }
    private sealed class StringLengthOptions
    {
        public int? MaximumLength { get; set; }
        public int MinimumLength { get; set; }
    }
    private sealed class LengthOptions { public int? Length { get; set; } }
    private sealed class RangeOptions
    {
        public string? OperandType { get; set; }
        public JsonElement Minimum { get; set; }
        public JsonElement Maximum { get; set; }
    }
    private sealed class RegexOptions
    {
        public string? Pattern { get; set; }
        public int MatchTimeoutInMilliseconds { get; set; } = 2000;
    }
    private sealed class CompareOptions { public string? OtherProperty { get; set; } }
}
