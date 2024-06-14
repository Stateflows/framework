using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Testing.Activities.Cradle
{
    internal class NodeTestCradle : ITestCradle
    {
        private readonly ActionNode testedNode;

        private readonly List<TokenHolder> inputTokens;

        private readonly IContextValues contextValues;

        public NodeTestCradle(ActionNode node, List<TokenHolder> inputTokens, IContextValues contextValues)
        {
            testedNode = node;
            this.inputTokens = inputTokens;
            this.contextValues = contextValues;
        }

        private readonly List<TokenHolder> outputTokens = new List<TokenHolder>();

        async Task<ITestResults> ITestCradle.SwingAsync()
        {
            InputTokensHolder.Tokens.Value = inputTokens;
            OutputTokensHolder.Tokens.Value = outputTokens;

            await testedNode.ExecuteAsync();

            InputTokensHolder.Tokens.Value = null;
            OutputTokensHolder.Tokens.Value = null;

            return new TestResults(outputTokens, contextValues);
        }
    }
}
