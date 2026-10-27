using Stateflows.Common;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Registers events whose processing is postponed while the state is active and its deferral guard
    /// accepts.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
    public interface IStateUtils<out TReturn>
    {
        /// <summary>Defers a matching event while the current state is active and its deferral guard accepts.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="buildAction">Optional callback configuring deferral guard. Without a guard, matching events are
        /// deferred unconditionally while the state is active.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        TReturn AddDeferredEvent(string eventName, DeferralBuildAction<StructuralEvent> buildAction = null)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            var vertex = ((IVertexBuilder)this).Vertex;
            var builder = new EventDeferralBuilder<StructuralEvent>(vertex);
            buildAction?.Invoke(builder);
            vertex.Deferrals.Add(eventName, builder.Logic);
            vertex.DeferralTypes.Add(eventName, typeof(StructuralEvent));
            vertex.StructuralDeferrals.Add(eventName);
            return (TReturn)this;
        }

        /// <summary>Defers a matching event while the current state is active and its deferral guard accepts.</summary>
        /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
        /// <param name="buildAction">Optional callback configuring deferral guard. Without a guard, matching events are
        /// deferred unconditionally while the state is active.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        TReturn AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction = null);

        /// <summary>Defers a matching event while the current state is active and its deferral guard accepts.</summary>
        /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
        /// <typeparam name="TDeferralGuard">Type of the deferral guard.</typeparam>
        /// <param name="buildAction">Optional callback configuring additional deferral guard.</param>
        /// <returns>The current state builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        TReturn AddDeferredEvent<TEvent, TDeferralGuard>(DeferralBuildAction<TEvent> buildAction = null)
            where TDeferralGuard : class, IDeferralGuard<TEvent>
            => AddDeferredEvent<TEvent>(b =>
            {
                b.AddGuard<TDeferralGuard>();
                buildAction?.Invoke(b);
            });
    }
}
