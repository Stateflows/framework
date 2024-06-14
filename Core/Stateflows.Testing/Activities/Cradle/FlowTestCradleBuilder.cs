using System;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Testing.Activities.Cradle
{
    internal class FlowTestCradleBuilder<TToken, TActionNode> : IFlowTestCradleBuilder<TToken>, IFlowTestCradleBuilderWithInput
        where TActionNode : Flow<TToken>
    {
        private readonly IServiceProvider serviceProvider;

        public FlowTestCradleBuilder(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly Dictionary<string, string> contextValuesCollection = new Dictionary<string, string>();
        private IContextValues contextValues;
        private IContextValues ContextValues
            => contextValues ??= new ContextValues(contextValuesCollection);

        private readonly List<TokenHolder> InputTokens = new List<TokenHolder>();

        IFlowTestCradleBuilderWithInput IFlowTestCradleBuilderWithInput.AddGlobalContextValue<T>(string key, T value)
        {
            ContextValues.Set(key, value);

            return this;
        }

        IFlowTestCradleBuilderWithInput IFlowTestCradleBuilder<TToken>.AddInputToken(TToken token)
        {
            InputTokens.Add(new TokenHolder<TToken>() { Payload = token });

            return this;
        }

        ITestCradle IFlowTestCradleBuilderWithInput.Build()
        {
            ContextValuesHolder.GlobalValues.Value = ContextValues;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return new FlowTestCradle<TToken>(serviceProvider.GetService<TActionNode>(), InputTokens, ContextValues);
        }
    }
}
