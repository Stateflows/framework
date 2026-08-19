using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

public interface IParentMetadataBuilder
{
    Dictionary<string, object> ParentMetadata { get; }
}