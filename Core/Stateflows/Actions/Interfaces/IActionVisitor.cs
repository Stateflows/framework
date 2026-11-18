using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Actions
{
    /// <summary>
    /// Receives an ordered description of a finalized action definition during an
    /// action-registration visit. Each callback is awaited before the next callback
    /// is invoked; a callback failure stops the visit and is propagated to its caller.
    /// The action identifier is provided before the action name and version, followed
    /// by callback-specific metadata.
    /// </summary>
    public interface IActionVisitor
    {
        /// <summary>
        /// Runs first for an action definition, before implementation type, binding,
        /// configuration, and completion notifications.
        /// </summary>
        /// <param name="actionId">The engine identifier for the action definition; it identifies the action kind and is not necessarily unique among action names.</param>
        /// <param name="actionName">The registered action name associated with this definition.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <param name="behaviorClassType">The behavior-class kind under which the action is registered, when supplied.</param>
        /// <param name="ownerClass">The owning behavior class, when the action is owned by another behavior.</param>
        /// <param name="parentClass">The parent behavior class, when the action is nested under another behavior.</param>
        /// <param name="hasDefaultInstance">Whether a default instance initialization is registered for this action's behavior class.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActionAddingAsync(string actionId, string actionName, int actionVersion, string? behaviorClassType = null, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false) => Task.CompletedTask;

        /// <summary>
        /// Runs after all action type, binding, and configuration notifications; this
        /// is the final callback for the visited action definition.
        /// </summary>
        /// <param name="actionId">The engine identifier for the action definition.</param>
        /// <param name="actionName">The registered action name associated with this definition.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActionAddedAsync(string actionId, string actionName, int actionVersion) => Task.CompletedTask;

        /// <summary>
        /// Reports the CLR implementation type registered for the action, when one
        /// exists; it follows the initial action notification.
        /// </summary>
        /// <typeparam name="TAction">The concrete action implementation type.</typeparam>
        /// <param name="actionId">The engine identifier for the action definition.</param>
        /// <param name="actionName">The registered action name associated with this definition.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActionTypeAddedAsync<TAction>(string actionId, string actionName, int actionVersion) where TAction : class, IAction => Task.CompletedTask;

        /// <summary>
        /// Reports one consumed event binding, after the action type notification and
        /// before configuration, produced-event, and token-binding notifications.
        /// Event names are the finalized mapped names accepted by this binding.
        /// </summary>
        /// <typeparam name="TEvent">The CLR event type represented by this binding.</typeparam>
        /// <param name="bindingId">The engine identity of this consumed-event binding.</param>
        /// <param name="actionName">The registered action name associated with this binding.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <param name="eventNames">The ordered event names accepted by the binding after type mapping.</param>
        /// <param name="supportedStatuses">The behavior statuses in which this binding is supported.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task CustomEventAddedAsync<TEvent>(string bindingId, string actionName, int actionVersion, IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses) => Task.CompletedTask;

        /// <summary>
        /// Reports one produced-event binding, after the action configuration
        /// notification. Names are the finalized mapped event names this binding can
        /// produce.
        /// </summary>
        /// <typeparam name="TEvent">The CLR event type represented by this produced binding.</typeparam>
        /// <param name="bindingId">The engine identity of this produced-event binding.</param>
        /// <param name="actionName">The registered action name associated with this binding.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <param name="eventNames">The ordered event names produced by the binding after type mapping.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ProducedEventAddedAsync<TEvent>(string bindingId, string actionName, int actionVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;

        /// <summary>
        /// Reports one input-token binding, in the order the action definition
        /// registered its distinct consumed token types.
        /// </summary>
        /// <typeparam name="TToken">The CLR token type accepted by this input binding.</typeparam>
        /// <param name="pinId">The engine identity of this input-token binding.</param>
        /// <param name="actionName">The registered action name associated with this binding.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task InputTokenAddedAsync<TToken>(string pinId, string actionName, int actionVersion) => Task.CompletedTask;

        /// <summary>Reports payload token names accepted by an action input.</summary>
        Task InputTokenNamesAddedAsync(string pinId, string actionName, int actionVersion,
            IReadOnlyList<string> tokenNames) => Task.CompletedTask;

        /// <summary>
        /// Reports one output-token binding, in the order the action definition
        /// registered its distinct produced token types.
        /// </summary>
        /// <typeparam name="TToken">The CLR token type produced by this output binding.</typeparam>
        /// <param name="pinId">The engine identity of this output-token binding.</param>
        /// <param name="actionName">The registered action name associated with this binding.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task OutputTokenAddedAsync<TToken>(string pinId, string actionName, int actionVersion) => Task.CompletedTask;

        /// <summary>Reports payload token names produced by an action output.</summary>
        Task OutputTokenNamesAddedAsync(string pinId, string actionName, int actionVersion,
            IReadOnlyList<string> tokenNames) => Task.CompletedTask;

        /// <summary>
        /// Reports the action's finalized configuration after its consumed-event
        /// bindings and before produced-event and token bindings.
        /// </summary>
        /// <param name="actionId">The engine identifier for the action definition.</param>
        /// <param name="actionName">The registered action name associated with this definition.</param>
        /// <param name="actionVersion">The registered version of the action definition.</param>
        /// <param name="isStateless">Whether the action is configured as stateless.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActionConfigurationAddedAsync(string actionId, string actionName, int actionVersion, bool isStateless) => Task.CompletedTask;
    }
}
