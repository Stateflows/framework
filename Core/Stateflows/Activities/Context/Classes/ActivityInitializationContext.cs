using System.Linq;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityInitializationContext<TInitializationEvent> :
        BaseContext,
        IActivityInitializationContext<TInitializationEvent>,
        IRootContext
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, TInitializationEvent initializationEvent, List<TokenHolder> inputTokens)
            : base(context, nodeScope)
        {
            InitializationEvent = initializationEvent;
            InputTokens = inputTokens ?? new List<TokenHolder>();
        }

        public ActivityInitializationContext(BaseContext context, TInitializationEvent initializationEvent, List<TokenHolder> inputTokens)
            : base(context)
        {
            InitializationEvent = initializationEvent;
            InputTokens = inputTokens ?? new List<TokenHolder>();
        }

        IActivityContext IActivityActionContext.Activity => Activity;

        public TInitializationEvent InitializationEvent { get; }

        public List<TokenHolder> InputTokens;

        public void Output<TToken>(TToken token)
            => OutputRange(new TToken[] { token });

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => InputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()).ToArray());
    }

    internal class ActivityInitializationContext :
        ActivityInitializationContext<object>,
        IActivityInitializationInspectionContext,
        IRootContext
    {
        public ActivityInitializationContext(BaseContext context, EventHolder initializationEventHolder, List<TokenHolder> inputTokens)
            : base(context, initializationEventHolder.BoxedPayload, inputTokens)
        { }

        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, EventHolder initializationEventHolder, List<TokenHolder> inputTokens)
            : base(context, nodeScope, initializationEventHolder.BoxedPayload, inputTokens)
        { }

        IActivityInspectionContext IActivityInitializationInspectionContext.Activity => Activity;
    }
}
