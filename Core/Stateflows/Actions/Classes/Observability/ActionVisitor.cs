using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Actions
{
    /// <summary>No-op base class for finalized definition visitors.</summary>
    public abstract class ActionVisitor : IActionVisitor
    {
        public virtual Task ActionAddingAsync(string actionId, string actionName, int actionVersion, string? behaviorClassType = null, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false) => Task.CompletedTask;
        public virtual Task ActionAddedAsync(string actionId, string actionName, int actionVersion) => Task.CompletedTask;
        public virtual Task ActionTypeAddedAsync<TAction>(string actionId, string actionName, int actionVersion) where TAction : class, IAction => Task.CompletedTask;
        public virtual Task CustomEventAddedAsync<TEvent>(string bindingId, string actionName, int actionVersion, IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses) => Task.CompletedTask;
        public virtual Task ProducedEventAddedAsync<TEvent>(string bindingId, string actionName, int actionVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;
        public virtual Task InputTokenAddedAsync<TToken>(string pinId, string actionName, int actionVersion) => Task.CompletedTask;
        public virtual Task OutputTokenAddedAsync<TToken>(string pinId, string actionName, int actionVersion) => Task.CompletedTask;
        public virtual Task InputTokenNamesAddedAsync(string pinId, string actionName, int actionVersion,
            IReadOnlyList<string> tokenNames) => Task.CompletedTask;
        public virtual Task OutputTokenNamesAddedAsync(string pinId, string actionName, int actionVersion,
            IReadOnlyList<string> tokenNames) => Task.CompletedTask;
        public virtual Task ActionConfigurationAddedAsync(string actionId, string actionName, int actionVersion, bool isStateless) => Task.CompletedTask;
    }
}
