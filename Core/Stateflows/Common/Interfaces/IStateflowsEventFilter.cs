using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

public interface IStateflowsEventFilter
{
    Task<IEnumerable<Type>> FilterTypesAsync(IEnumerable<Type> types);
}