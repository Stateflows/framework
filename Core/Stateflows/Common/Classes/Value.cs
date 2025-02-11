using System;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    internal class Value<T> : BaseValueAccessor<T>, IValue<T>
    {
        public Value(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
            : base(valueName, valueSetSelector, collectionName)
        { }
    }
}
