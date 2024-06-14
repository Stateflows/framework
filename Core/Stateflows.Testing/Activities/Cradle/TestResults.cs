using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Testing.Activities.Cradle
{
    internal class TestResults : ITestResults
    {
        public TestResults(List<TokenHolder> outputTokens, IContextValues contextValues)
        {
            this.outputTokens = outputTokens;
            this.contextValues = contextValues;
        }

        private readonly List<TokenHolder> outputTokens;
        private readonly IContextValues contextValues;

        bool ITestResults.IsGlobalContextValueSet(string key)
            => contextValues.IsSet(key);

        bool ITestResults.TryGetGlobalContextValue<T>(string key, out T value)
            => contextValues.TryGet(key, out value);

        IEnumerable<T> ITestResults.GetOutputTokens<T>()
            => outputTokens.OfType<TokenHolder<T>>().Select(holder => holder.Payload).ToArray();
    }
}
