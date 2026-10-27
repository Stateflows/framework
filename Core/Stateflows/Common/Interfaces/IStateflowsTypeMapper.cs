using System;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Maps a type to one or more alternative CLR types.</summary>
    public interface IStateflowsTypeMapper
    {
        /// <summary>Attempts to map a type to its registered alternatives.</summary>
        /// <param name="type">The type to map.</param>
        /// <param name="types">Receives the mapped types when a mapping is available.</param>
        /// <returns><see langword="true"/> when the type has a mapping.</returns>
        bool TryMapType(Type type, out IEnumerable<Type> types);
    }
}
