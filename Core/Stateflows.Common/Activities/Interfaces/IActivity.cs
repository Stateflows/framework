﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Events;

namespace Stateflows.Activities
{
    public interface IActivity : IBehavior
    {
        Task<RequestResult<ExecutionResponse>> ExecuteAsync(InitializationRequest initializationRequest = null, IEnumerable<Token> inputTokens = null);

        Task<RequestResult<CancelResponse>> CancelAsync();
    }
}
