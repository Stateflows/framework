using System.Collections.ObjectModel;

namespace Stateflows.Common.Json;

/// <summary>Host-owned CLR aliases available to JSON state-machine and activity definitions.</summary>
public sealed class JsonOptions
{
    private readonly Dictionary<string, Type> types = new(StringComparer.Ordinal);

    /// <summary>Registers an executable, identity, token, event, exception, configuration, or metadata type.</summary>
    public JsonOptions AddType<T>(string alias) => AddType(alias, typeof(T));

    /// <summary>Registers a closed CLR type. Documents cannot register or load assemblies.</summary>
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
