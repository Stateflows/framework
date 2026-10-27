using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

/// <summary>Filters the event payload types available to a processing operation.</summary>
public interface IStateflowsEventFilter
{
    /// <summary>Returns the event types that pass the filter.</summary>
    /// <param name="types">The event payload types to evaluate.</param>
    /// <returns>The types retained by the filter.</returns>
    Task<IEnumerable<Type>> FilterTypesAsync(IEnumerable<Type> types);
}