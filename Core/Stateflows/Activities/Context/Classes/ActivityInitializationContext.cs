using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Utils;
using Stateflows.Payloads;
using Stateflows.Activities.Models;

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

        public IEnumerable<Payload> GetPayloadTokens(string tokenName)
            => GetPayloadTokens([tokenName]);

        public IEnumerable<Payload> GetPayloadTokens(IReadOnlyList<string> tokenNames)
        {
            var names = PayloadNameSet.Validate(tokenNames, nameof(tokenNames));
            return InputTokens
                .Where(token => token.BoxedPayload is Payload payload && names.Contains(payload.Name, StringComparer.Ordinal))
                .Select(token => (Payload)token.BoxedPayload)
                .ToArray();
        }

        public bool HasPayloadTokens(string tokenName)
            => HasPayloadTokens([tokenName]);

        public bool HasPayloadTokens(IReadOnlyList<string> tokenNames)
            => GetPayloadTokens(tokenNames).Any();

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => InputTokens.ToTokensOfType<TToken>().ToArray();

        public bool HasTokensOfType<TToken>()
            => InputTokens.OfType<TokenHolder<TToken>>().Any();

        public void PassPayloadTokensOn(IReadOnlyList<string> tokenNames)
        {
            var key = TokenKey.OfNames(PayloadNameSet.Validate(tokenNames, nameof(tokenNames)));
            OutputRange(InputTokens.Where(key.Matches).Select(token => (Payload)token.BoxedPayload));
        }

        public object LockHandle => Activity.LockHandle;
        public IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
