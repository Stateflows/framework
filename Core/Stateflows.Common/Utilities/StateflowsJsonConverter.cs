using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Stateflows.Common.Utilities
{
    /// <summary>Serializes and clones Stateflows data using Newtonsoft.Json settings that ignore reference loops.</summary>
    /// <remarks>Polymorphic operations include CLR type metadata and are intended for trusted Stateflows data, not untrusted JSON.</remarks>
    public static class StateflowsJsonConverter
    {
        private static readonly JsonSerializerSettings PolymorphicCamelSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly JsonSerializerSettings PolymorphicSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly JsonSerializerSettings CamelSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>Clones a reference value through a polymorphic JSON round trip.</summary>
        /// <typeparam name="T">The reference type of the value.</typeparam>
        /// <param name="value">The value to clone.</param>
        /// <returns>The deserialized clone when assignable to the requested type, or null.</returns>
        [DebuggerStepThrough]
        public static T CloneObject<T>(T value)
            where T : class
            => DeserializeObject(SerializePolymorphicObject(value)) as T;

        /// <summary>Clones a value through a polymorphic JSON round trip.</summary>
        /// <typeparam name="T">The value type to return.</typeparam>
        /// <param name="value">The value to clone.</param>
        /// <returns>A deserialized value cast to the requested type.</returns>
        [DebuggerStepThrough]
        public static T Clone<T>(T value)
            => (T)DeserializeObject(SerializePolymorphicObject(value));

        /// <summary>
        /// Serializes an object to JSON including CLR type metadata.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="useCamelCase">Specifies, if names should be normalized to camelCase.</param>
        /// <param name="formatting">Formatting settings.</param>
        /// <returns>A JSON string representation of the object.</returns>
        [DebuggerStepThrough]
        public static string SerializePolymorphicObject(object value, bool useCamelCase = false, Formatting formatting = Formatting.None)
        {
            var settings = useCamelCase
                ? PolymorphicCamelSettings
                : PolymorphicSettings;
            var oldFormatting = settings.Formatting;
            settings.Formatting = formatting;
            var result = JsonConvert.SerializeObject(value, value?.GetType(), settings);
            settings.Formatting = oldFormatting;
            
            return result;
        }

        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="useCamelCase">Specifies, if names should be normalized to camelCase.</param>
        /// <param name="formatting">Formatting settings.</param>
        /// <returns>A JSON string representation of the object.</returns>
        [DebuggerStepThrough]
        public static string SerializeObject(object value, bool useCamelCase = false, Formatting formatting = Formatting.None)
        {
            var settings = useCamelCase
                ? CamelSettings
                : Settings;
            var oldFormatting = settings.Formatting;
            settings.Formatting = formatting;
            var result = JsonConvert.SerializeObject(value, null, settings);
            settings.Formatting = oldFormatting;
            
            return result;
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        [DebuggerStepThrough]
        public static object DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject(value, null, PolymorphicSettings);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        [DebuggerStepThrough]
        public static object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, PolymorphicSettings);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        [DebuggerStepThrough]
        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, PolymorphicSettings);
        }
        
        /// <summary>Parses a JSON numeric token and converts its integer value to an enum.</summary>
        /// <typeparam name="T">The enum type represented by a 32-bit integer.</typeparam>
        /// <param name="value">The JSON numeric representation of the enum value.</param>
        /// <returns>The parsed value cast to the requested enum type.</returns>
        public static T ParseStringToEnum<T>(string value)
            => (T)(object)JToken.Parse(value).Value<int>();

        /// <summary>Parses a textual value using special handling for GUIDs and strings, or JSON conversion for other types.</summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="value">The text to parse; GUID text is unquoted and other non-string values use JSON syntax.</param>
        /// <returns>The converted value.</returns>
        public static T ParseStringToTypedValue<T>(string value)
            => typeof(T) == typeof(Guid)
                ? (T)(object)Guid.Parse(value)
                : typeof(T) == typeof(string)
                    ? JToken.Parse($"\"{value}\"").Value<T>()
                    : JToken.Parse(value).Value<T>();
    }
}