using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes;

internal class NoOpEventFilter : IStateflowsEventFilter
{
    public Task<IEnumerable<Type>> FilterTypesAsync(IEnumerable<Type> types)
        => Task.FromResult(types);
}