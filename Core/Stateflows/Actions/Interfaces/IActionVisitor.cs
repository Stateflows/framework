using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Actions
{
    /// <summary>Receives notifications about registered action definitions and custom events.</summary>
    /// <remarks>Visitors are used for metadata integration and inspection; callbacks run as registrations are visited.</remarks>
    public interface IActionVisitor
    {
        /// <summary>Signals that an action definition is about to be visited.</summary>
        /// <param name="actionName">The action name.</param>
        /// <param name="actionVersion">The action version.</param>
        /// <param name="behaviorClassType">Optional custom behavior class type.</param>
        /// <param name="ownerClass">Optional owning behavior class.</param>
        /// <param name="parentClass">Optional parent behavior class.</param>
        /// <param name="hasDefaultInstance">Whether a default instance is registered.</param>
        Task ActionAddingAsync(string actionName, int actionVersion, string? behaviorClassType = null, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false);

        /// <summary>Signals that an action definition has been visited.</summary>
        /// <param name="actionName">The action name.</param>
        /// <param name="actionVersion">The action version.</param>
        Task ActionAddedAsync(string actionName, int actionVersion);
        
        /// <summary>Signals that a typed action implementation has been registered.</summary>
        /// <typeparam name="TAction">The action implementation type.</typeparam>
        /// <param name="actionName">The action name.</param>
        /// <param name="actionVersion">The action version.</param>
        Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
            where TAction : class, IAction;
        
        /// <summary>Signals that a typed custom event has been declared by an action.</summary>
        /// <typeparam name="TEvent">The custom event type.</typeparam>
        /// <param name="actionName">The action name.</param>
        /// <param name="actionVersion">The action version.</param>
        /// <param name="supportedStatuses">The behavior statuses supported by the event.</param>
        Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses);

        /// <summary>Signals that a custom event definition has been declared by an action.</summary>
        /// <param name="definition">The event definition.</param>
        /// <param name="actionName">The action name.</param>
        /// <param name="actionVersion">The action version.</param>
        /// <param name="supportedStatuses">The behavior statuses supported by the event.</param>
        Task CustomEventAddedAsync(EventDefinition definition, string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}
