using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

/// <summary>Provides the metadata configured for an element.</summary>
public interface IMetadataBuilder
{
    /// <summary>Gets the element metadata keyed by metadata name.</summary>
    Dictionary<string, object> Metadata { get; }
}