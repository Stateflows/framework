using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Utils;

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
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = Behavior.Context.ContextParentId.HasValue
                ? Behavior
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = Behavior.Context.ContextOwnerId.HasValue
                ? Behavior
                : null;
            
            return ownerBehaviorContext != null;
        }

        public List<TokenHolder> InputTokens;

        public void Output<TToken>(TToken token)
            => OutputRange([ token ]);

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => InputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()).ToArray());

        public void PassTokensOfTypeOn<TToken>()
            => OutputRange(InputTokens.OfTokenType<TToken>());

        public void PassAllTokensOn()
            => OutputRange(InputTokens);

        public object LockHandle => Activity.LockHandle;
        public IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
