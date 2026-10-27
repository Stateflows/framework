using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Stateflows.Common.Json;

/// <summary>Host-owned CLR type and validation aliases available to JSON behavior and event definitions.</summary>
/// <remarks>Documents refer only to aliases registered here; they cannot load assemblies or name arbitrary CLR types.</remarks>
public sealed class JsonOptions
{
    private readonly Dictionary<string, Type> types = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<JsonElement, ValidationAttribute>> validators = new(StringComparer.Ordinal);

    /// <summary>Registers a host-owned factory for a custom event validation attribute.</summary>
    /// <param name="alias">The document-visible name used by custom validation declarations.</param>
    /// <param name="factory">Creates a validation attribute from the validator's JSON options object.</param>
    /// <returns>This options instance for chaining registrations.</returns>
    /// <remarks>The JSON options passed to the factory are untrusted input and should be validated before use.</remarks>
    public JsonOptions AddValidator(string alias, Func<JsonElement, ValidationAttribute> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentNullException.ThrowIfNull(factory);
        validators.Add(alias, factory);
        return this;
    }

    internal IReadOnlyDictionary<string, Func<JsonElement, ValidationAttribute>> ValidatorSnapshot()
        => new ReadOnlyDictionary<string, Func<JsonElement, ValidationAttribute>>(
            new Dictionary<string, Func<JsonElement, ValidationAttribute>>(validators, StringComparer.Ordinal));

    /// <summary>Registers a closed host-owned CLR type under a document-visible alias.</summary>
    /// <typeparam name="T">The CLR type which JSON definitions may identify by alias.</typeparam>
    /// <param name="alias">The alias used in the JSON definition.</param>
    /// <returns>This options instance for chaining registrations.</returns>
    public JsonOptions AddType<T>(string alias) => AddType(alias, typeof(T));

    /// <summary>Registers a closed host-owned CLR type under a document-visible alias.</summary>
    /// <param name="alias">The alias used in the JSON definition.</param>
    /// <param name="type">The closed CLR type that the alias resolves to.</param>
    /// <returns>This options instance for chaining registrations.</returns>
    /// <exception cref="ArgumentException">The alias is invalid, contains an assembly separator, or the type cannot be used by a definition.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    /// <remarks>Documents cannot register or load assemblies. Register only types the host intends to expose to definitions.</remarks>
    public JsonOptions AddType(string alias, Type type)
    {
        ArgumentException.ThrowIfNullOrEmpty(alias);
        ArgumentNullException.ThrowIfNull(type);
        if (alias.Contains(','))
            throw new ArgumentException("CLR aliases cannot contain the assembly-name separator ','.", nameof(alias));
        if (type.ContainsGenericParameters || type == typeof(void) || type.IsByRef || type.IsPointer || type.IsByRefLike)
            throw new ArgumentException("An alias must identify a usable closed CLR type.", nameof(type));
        types.Add(alias, type);
        return this;
    }

    internal IReadOnlyDictionary<string, Type> Snapshot()
        => new ReadOnlyDictionary<string, Type>(new Dictionary<string, Type>(types, StringComparer.Ordinal));
}
