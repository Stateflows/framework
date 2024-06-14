using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Testing.Activities.Cradle
{
    internal class NodeTestCradleBuilder<TActionNode> : INodeTestCradleBuilder
        where TActionNode : ActionNode
    {
        private readonly IServiceProvider serviceProvider;

        public NodeTestCradleBuilder(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly Dictionary<string, string> contextValuesCollection = new Dictionary<string, string>();
        private IContextValues contextValues;
        private IContextValues ContextValues
            => contextValues ??= new ContextValues(contextValuesCollection);

        private readonly List<TokenHolder> InputTokens = new List<TokenHolder>();

        INodeTestCradleBuilder INodeTestCradleBuilder.AddGlobalContextValue<T>(string key, T value)
        {
            ContextValues.Set(key, value);

            return this;
        }

        INodeTestCradleBuilder INodeTestCradleBuilder.AddInputToken<T>(T token)
        {
            InputTokens.Add(new TokenHolder<T>() { Payload = token });

            return this;
        }

        INodeTestCradleBuilder INodeTestCradleBuilder.AddInputTokens<T>(IEnumerable<T> tokens)
        {
            InputTokens.AddRange(tokens.Select(token => new TokenHolder<T>() { Payload = token }));

            return this;
        }

        ITestCradle INodeTestCradleBuilder.Build()
        {
            ContextValuesHolder.GlobalValues.Value = ContextValues;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return new NodeTestCradle(serviceProvider.GetService<TActionNode>(), InputTokens, ContextValues);
        }
    }
}
