using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Actions
{
    /// <summary>Convenience base class for action visitors with no-op callbacks.</summary>
    /// <remarks>Override only the registration callbacks needed by an integration.</remarks>
    public abstract class ActionVisitor : IActionVisitor
    {
        /// <inheritdoc/>
        public virtual Task ActionAddingAsync(string actionName, int actionVersion, string? behaviorClassType = null,
            BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ActionAddedAsync(string actionName, int actionVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion) where TAction : class, IAction
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task CustomEventAddedAsync(EventDefinition definition, string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}