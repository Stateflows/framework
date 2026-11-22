using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Registers events whose processing is postponed while the state is active and its deferral guard
    /// accepts.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
    public interface IStateUtils<out TReturn>
    {

        /// <summary>Defers events matching any of the given payload names while the current state is active and its
        /// deferral guard accepts.</summary>
        /// <param name="eventNames">Registered payload names to match. Must contain at least one name; names must be
        /// distinct and must not be null, empty, or whitespace.</param>
        /// <param name="buildAction">Optional callback configuring deferral guard. Without a guard, matching events are
        /// deferred unconditionally while the state is active.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        TReturn AddDeferredPayloadEvent(IReadOnlyList<string> eventNames, DeferralBuildAction<Payload> buildAction = null)
        {
            var names = PayloadNameSet.Validate(eventNames, nameof(eventNames));
            var vertex = ((IVertexBuilder)this).Vertex;
            var builder = new EventDeferralBuilder<Payload>(vertex);
            buildAction?.Invoke(builder);
            vertex.AddPayloadDeferral(names, builder.Logic);
            return (TReturn)this;
        }

        /// <summary>Defers events matching any of the given payload names, guarded by a deferral guard class.</summary>
        /// <typeparam name="TDeferralGuard">Type of the deferral guard; receives the deferred payload.</typeparam>
        /// <param name="eventNames">Registered payload names to match.</param>
        /// <param name="buildAction">Optional callback configuring additional deferral guard.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
        TReturn AddDeferredPayloadEvent<TDeferralGuard>(IReadOnlyList<string> eventNames, DeferralBuildAction<Payload> buildAction = null)
            where TDeferralGuard : class, IDeferralGuard<Payload>
            => AddDeferredPayloadEvent(eventNames, b =>
            {
                b.AddGuard<TDeferralGuard>();
                buildAction?.Invoke(b);
            });

        /// <summary>Defers a matching event while the current state is active and its deferral guard accepts.</summary>
        /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
        /// <param name="buildAction">Optional callback configuring deferral guard. Without a guard, matching events are
        /// deferred unconditionally while the state is active.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
        TReturn AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction = null);

        /// <summary>Defers a matching event while the current state is active and its deferral guard accepts.</summary>
        /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
        /// <typeparam name="TDeferralGuard">Type of the deferral guard.</typeparam>
        /// <param name="buildAction">Optional callback configuring additional deferral guard.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
        TReturn AddDeferredEvent<TEvent, TDeferralGuard>(DeferralBuildAction<TEvent> buildAction = null)
            where TDeferralGuard : class, IDeferralGuard<TEvent>
            => AddDeferredEvent<TEvent>(b =>
            {
                b.AddGuard<TDeferralGuard>();
                buildAction?.Invoke(b);
            });
    }
}
