using System;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsTypeMapper
    {
        bool TryMapType(Type type, out IEnumerable<Type> types);
    }
}
