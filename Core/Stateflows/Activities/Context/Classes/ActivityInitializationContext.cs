using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityInitializationContext<TInitializationEvent> :
        ActivityInitializationContext,
        IActivityInitializationContext<TInitializationEvent>
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, EventHolder<TInitializationEvent> initializationEventHolder, List<TokenHolder> inputTokens)
            : base(context, nodeScope, inputTokens)
        {
            InitializationEventHolder = initializationEventHolder;
        }

        public EventHolder<TInitializationEvent> InitializationEventHolder { get; }

        public TInitializationEvent InitializationEvent => InitializationEventHolder.Payload;
    }

    internal class ActivityInitializationContext :
        BaseContext,
        IActivityInitializationContext,
        IRootContext
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, List<TokenHolder> inputTokens)
            : base(context, nodeScope)
        {
            InputTokens = inputTokens ?? [];
        }
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        public List<TokenHolder> InputTokens;

        public void Output<TToken>(TToken token)
            => OutputRange([ token ]);

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => InputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()).ToArray());

        public object LockHandle => Activity.LockHandle;
        public IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
