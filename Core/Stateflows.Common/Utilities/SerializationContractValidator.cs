using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Stateflows.Common.Utilities;

/// <summary>A serialization contract diagnostic. Warnings indicate incomplete verification.</summary>
public sealed record SerializationContractDiagnostic(string Rule, string Path, string Message, bool IsError);

/// <summary>Inspects contracts using the serializer used by Stateflows polymorphic operations.</summary>
public static class SerializationContractValidator
{
    /// <summary>Inspects a type and its nested serialized contracts without constructing application objects.</summary>
    public static IReadOnlyList<SerializationContractDiagnostic> Validate(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var serializer = StateflowsJsonConverter.CreatePolymorphicSerializer();
        var diagnostics = new List<SerializationContractDiagnostic>();
        var visited = new HashSet<Type>();
        Inspect(type, type.FullName ?? type.Name);
        return diagnostics;

        void Report(string rule, string path, string message, bool error = false)
            => diagnostics.Add(new(rule, path, message, error));

        void Inspect(Type current, string path)
        {
            if (!visited.Add(current)) return;
            if (current.ContainsGenericParameters || current.IsPointer || current.IsByRef || typeof(Delegate).IsAssignableFrom(current))
            {
                Report("SER001", path, $"Type '{current}' cannot be used as a closed JSON payload.", true);
                return;
            }
            JsonContract contract;
            try { contract = serializer.ContractResolver.ResolveContract(current); }
            catch (JsonException exception)
            {
                Report("SER001", path, exception.Message, true);
                return;
            }
            if (contract.Converter != null || serializer.Converters.Any(converter => converter.CanConvert(current)))
            {
                Report("SER100", path, "Custom converter behavior requires representative round-trip samples.");
                return;
            }
            if (contract is JsonPrimitiveContract || contract is JsonStringContract) return;
            if (current == typeof(object) || current.IsInterface || current.IsAbstract)
                Report("SER101", path, "Runtime concrete types cannot be exhaustively verified.");
            if (contract is JsonArrayContract array)
            {
                if (array.CollectionItemType != null) Inspect(array.CollectionItemType, path + "[]");
                return;
            }
            if (contract is JsonDictionaryContract dictionary)
            {
                if (dictionary.DictionaryKeyType != null) Inspect(dictionary.DictionaryKeyType, path + ".Key");
                if (dictionary.DictionaryValueType != null) Inspect(dictionary.DictionaryValueType, path + ".Value");
                return;
            }
            if (contract is not JsonObjectContract obj)
            {
                Report("SER102", path, "This special JSON contract requires representative round-trip samples.");
                return;
            }
            if (!current.IsInterface && !current.IsAbstract && obj.DefaultCreator == null &&
                obj.OverrideCreator == null && obj.CreatorParameters.Count == 0 && !current.IsValueType)
                Report("SER002", path, "No usable JSON constructor. Select one with Newtonsoft.Json.JsonConstructor or provide an appropriate constructor.", true);

            foreach (var parameter in obj.CreatorParameters.Where(property => !property.Ignored))
            {
                var member = obj.Properties.GetClosestMatchProperty(parameter.PropertyName);
                if (member == null || member.Ignored || !member.Readable || member.PropertyType != parameter.PropertyType)
                    Report("SER003", path + "." + parameter.PropertyName,
                        "JSON constructor parameter does not match a readable serialized member by name and type; its value can be lost.", true);
            }
            foreach (var property in obj.Properties.Where(property => !property.Ignored && property.Readable))
            {
                var memberPath = path + "." + property.PropertyName;
                if (property.Converter != null || property.ItemConverter != null)
                {
                    Report("SER100", memberPath, "Member converter requires representative round-trip samples.");
                    continue;
                }
                if (!property.Writable && obj.CreatorParameters.GetClosestMatchProperty(property.PropertyName) == null)
                    Report("SER103", memberPath, "Read-only member population or computed state requires a representative round-trip sample.");
                if (property.PropertyType != null) Inspect(property.PropertyType, memberPath);
            }
            foreach (var request in current.GetInterfaces().Where(candidate =>
                candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IRequest<>)))
                Inspect(request.GetGenericArguments()[0], path + ".Response");
        }
    }
}
