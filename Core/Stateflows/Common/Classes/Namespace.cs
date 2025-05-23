using System;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    internal class Namespace : BaseNamespaceAccessor, INamespace
    {
        public Namespace(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
            : base(valueName, valueSetSelector, collectionName)
        { }
    }
}
