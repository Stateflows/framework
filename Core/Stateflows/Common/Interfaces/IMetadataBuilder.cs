using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

public interface IMetadataBuilder
{
    Dictionary<string, object> Metadata { get; }
}