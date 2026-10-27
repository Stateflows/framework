namespace Stateflows.Common
{
    /// <summary>Defines scalar types supported by structural event fields.</summary>
    public enum FieldType
    {
        /// <summary>A Unicode string value.</summary>
        String,
        /// <summary>A Boolean value.</summary>
        Boolean,
        /// <summary>A signed 32-bit integer.</summary>
        Int32,
        /// <summary>A signed 64-bit integer.</summary>
        Int64,
        /// <summary>A decimal numeric value.</summary>
        Decimal,
        /// <summary>A double-precision floating-point value.</summary>
        Double,
        /// <summary>A globally unique identifier.</summary>
        Guid,
        /// <summary>A date and time value.</summary>
        DateTime,
        /// <summary>A date and time value with an explicit UTC offset.</summary>
        DateTimeOffset,
        /// <summary>A time interval.</summary>
        TimeSpan,
        /// <summary>A calendar date without a time component.</summary>
        DateOnly,
        /// <summary>A time of day without a date component.</summary>
        TimeOnly,
        /// <summary>A byte array.</summary>
        Binary,
        /// <summary>A string restricted to the values declared by the event schema.</summary>
        Enum
    }
}
