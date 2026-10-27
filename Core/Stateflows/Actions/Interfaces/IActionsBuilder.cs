using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Actions.Registration.Interfaces.Base;

namespace Stateflows.Actions
{
    /// <summary>Registers actions and global action observability services.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/actions">Actions documentation</seealso>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability">Observability documentation</seealso>
    public interface IActionsBuilder : IActionObservability<IActionsBuilder>
    {
        /// <summary>Discovers action implementations and registrations in an assembly.</summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddFromAssembly(Assembly assembly);

        /// <summary>Discovers action implementations and registrations in multiple assemblies.</summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);

        /// <summary>Registers an action delegate under the default version.</summary>
        /// <param name="actionName">The action's registration name.</param>
        /// <param name="actionDelegate">The action implementation to execute.</param>
        /// <param name="buildAction">Optional callback for action-specific configuration.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);

        /// <summary>Registers an action delegate under an explicit version.</summary>
        /// <param name="actionName">The action's registration name.</param>
        /// <param name="version">The action definition version.</param>
        /// <param name="actionDelegate">The action implementation to execute.</param>
        /// <param name="buildAction">Optional callback for action-specific configuration.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);

        /// <summary>Registers a typed action and optionally overrides its name and version.</summary>
        /// <typeparam name="TAction">The action implementation type.</typeparam>
        /// <param name="actionName">An optional name; when omitted, the type's inferred action name is used.</param>
        /// <param name="version">The action definition version.</param>
        /// <param name="buildAction">Optional callback for action-specific configuration.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;

        /// <summary>Registers a typed action under an explicit version and inferred name.</summary>
        /// <typeparam name="TAction">The action implementation type.</typeparam>
        /// <param name="version">The action definition version.</param>
        /// <param name="buildAction">Optional callback for action-specific configuration.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddAction<TAction>(int version, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;

        /// <summary>Registers a typed action using its inferred name and default version.</summary>
        /// <typeparam name="TAction">The action implementation type.</typeparam>
        /// <param name="buildAction">Optional callback for action-specific configuration.</param>
        /// <returns>This builder for further configuration.</returns>
        IActionsBuilder AddAction<TAction>(ActionBuildAction<TAction> buildAction)
            where TAction : class, IAction;
    }
}
