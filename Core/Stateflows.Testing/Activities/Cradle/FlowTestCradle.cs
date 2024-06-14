using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Testing.Activities.Cradle
{
    internal class FlowTestCradle<TToken> : ITestCradle
    {
        private readonly Flow<TToken> testedFlow;

        private readonly List<TokenHolder> inputTokens;

        private readonly IContextValues contextValues;

        public FlowTestCradle(Flow<TToken> flow, List<TokenHolder> inputTokens, IContextValues contextValues)
        {
            testedFlow = flow;
            this.inputTokens = inputTokens;
            this.contextValues = contextValues;
        }

        private readonly List<TokenHolder> outputTokens = new List<TokenHolder>();

        async Task<ITestResults> ITestCradle.SwingAsync()
        {
            InputTokensHolder.Tokens.Value = inputTokens;
            OutputTokensHolder.Tokens.Value = outputTokens;

            if (await testedFlow.GuardAsync())
            {
            }

            InputTokensHolder.Tokens.Value = null;
            OutputTokensHolder.Tokens.Value = null;

            return new TestResults(outputTokens, contextValues);
        }
    }
}
