﻿using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Stateflows.Common.Utilities
{
    public static class StateflowsJsonConverter
    {
        private static readonly JsonSerializerSettings polymorphicCamelSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly JsonSerializerSettings polymorphicSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly JsonSerializerSettings camelSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        [DebuggerStepThrough]
        public static T CloneObject<T>(T value)
            where T : class
            => DeserializeObject(SerializePolymorphicObject(value)) as T;

        [DebuggerStepThrough]
        public static T Clone<T>(T value)
            => (T)DeserializeObject(SerializePolymorphicObject(value));

        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        [DebuggerStepThrough]
        public static string SerializePolymorphicObject(object value, bool useCamelCase = false, Formatting formatting = Formatting.None)
        {
            settings.Formatting = formatting;
            return JsonConvert.SerializeObject(value, null, useCamelCase ? polymorphicCamelSettings : polymorphicSettings);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        [DebuggerStepThrough]
        public static string SerializeObject(object value, bool useCamelCase = false, Formatting formatting = Formatting.None)
        {
            settings.Formatting = formatting;
            return JsonConvert.SerializeObject(value, null, useCamelCase ? camelSettings : settings);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        [DebuggerStepThrough]
        public static object DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject(value, null, polymorphicSettings);
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
            return JsonConvert.DeserializeObject(value, type, polymorphicSettings);
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
            return JsonConvert.DeserializeObject<T>(value, polymorphicSettings);
        }
    }
}