using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Events
{
    public class ActivityInitializationRequest : InitializationRequest
    {
        public IEnumerable<Token> InputTokens { get; } = new List<Token>();
    }
}
