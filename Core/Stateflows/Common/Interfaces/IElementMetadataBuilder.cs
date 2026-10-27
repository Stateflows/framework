namespace Stateflows.Common.Interfaces;

/// <summary>Adds metadata to a model element during fluent registration.</summary>
/// <typeparam name="TReturn">The builder result type.</typeparam>
public interface IElementMetadataBuilder<out TReturn>
{
    /// <summary>Adds a metadata entry.</summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The value associated with the key.</param>
    /// <returns>The builder result for continued configuration.</returns>
    TReturn AddMetadata(string key, object value);
}