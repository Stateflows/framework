using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    internal class Namespace : INamespace
    {
        private readonly string namespaceName;
        private readonly Func<IContextValues> namespaceSelector;
        private readonly string collectionName;
        private readonly IContextValues parentValueSet;

        public Namespace(string namespaceName, Func<IContextValues> namespaceSelector, string collectionName)
        {
            this.namespaceName = namespaceName;
            this.namespaceSelector = namespaceSelector;
            this.collectionName = collectionName;
            this.parentValueSet = namespaceSelector?.Invoke() ?? throw new StateflowsDefinitionException($"{collectionName} set is not available in current context");
        }
        
        public IValue<T> GetValue<T>(string key)
            => new Value<T>($"{this.namespaceName}.{key}", namespaceSelector, collectionName);

        public INamespace GetNamespace(string namespaceName)
            => new Namespace($"{this.namespaceName}.{namespaceName}", namespaceSelector, collectionName);

        public Task ClearAsync()
            => parentValueSet.RemovePrefixedAsync($"{namespaceName}.");
    }
}
