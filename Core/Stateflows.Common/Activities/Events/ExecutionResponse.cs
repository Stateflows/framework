using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionResponse : Response
    {
        public bool ExecutionSuccessful { get; set; }

        public IEnumerable<Token> OutputTokens { get; set; } = new List<Token>();
    }
}
