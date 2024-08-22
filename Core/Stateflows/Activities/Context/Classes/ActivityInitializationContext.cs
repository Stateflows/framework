using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using System.Collections.Generic;
using Stateflows.Activities.Extensions;
using System.Linq;
using Stateflows.Utils;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityInitializationContext<TInitializationEvent> :
        BaseContext,
        IActivityInitializationContext<TInitializationEvent>,
        IRootContext
        where TInitializationEvent : Event, new()
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
        ActivityInitializationContext<Event>,
        IActivityInitializationInspectionContext,
        IRootContext
    {
        public ActivityInitializationContext(BaseContext context, Event initializationEvent, List<TokenHolder> inputTokens)
            : base(context, initializationEvent, inputTokens)
        { }

        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, Event initializationEvent, List<TokenHolder> inputTokens)
            : base(context, nodeScope, initializationEvent, inputTokens)
        { }

        IActivityInspectionContext IActivityInitializationInspectionContext.Activity => Activity;
    }
}
