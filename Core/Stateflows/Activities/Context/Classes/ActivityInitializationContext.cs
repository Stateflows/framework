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
        ActivityInitializationContext,
        IActivityInitializationContext<TInitializationEvent>,
        IRootContext
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
        IActivityInitializationInspectionContext,
        IRootContext
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, List<TokenHolder> inputTokens)
            : base(context, nodeScope)
        {
            InputTokens = inputTokens ?? new List<TokenHolder>();
        }

        IActivityInspectionContext IActivityInitializationInspectionContext.Activity => Activity;

        IActivityContext IActivityActionContext.Activity => Activity;

        public List<TokenHolder> InputTokens;

        public void Output<TToken>(TToken token)
            => OutputRange(new TToken[] { token });

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => InputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()).ToArray());
    }
}
