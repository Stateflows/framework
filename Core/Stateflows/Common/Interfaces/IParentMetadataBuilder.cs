using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

/// <summary>Provides metadata configured for the parent of an element.</summary>
public interface IParentMetadataBuilder
{
    /// <summary>Gets the parent metadata keyed by metadata name.</summary>
    Dictionary<string, object> ParentMetadata { get; }
}