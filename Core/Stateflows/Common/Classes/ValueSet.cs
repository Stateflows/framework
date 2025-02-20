using System;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    internal class ValueSet : BaseValueSetAccessor, IValueSet
    {
        public ValueSet(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
            : base(valueName, valueSetSelector, collectionName)
        { }
    }
}
