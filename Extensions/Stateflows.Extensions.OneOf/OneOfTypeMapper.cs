using System;
using System.Linq;
using System.Collections.Generic;
using OneOf;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.OneOf
{
    internal class OneOfTypeMapper : IStateflowsTypeMapper
    {
        public bool TryMapType(Type type, out IEnumerable<Type> types)
        {
            types = type.GetInterfaces().Contains(typeof(IOneOf))
                ? type
                    .GetGenericArguments()
                    .ToArray()
                : new Type[0];

            return types.Any();
        }
    }
}
