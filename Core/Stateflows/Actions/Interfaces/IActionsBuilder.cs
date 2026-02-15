using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Actions.Registration.Interfaces.Base;

namespace Stateflows.Actions
{
    public interface IActionsBuilder : IActionObservability<IActionsBuilder>
    {
        IActionsBuilder AddFromAssembly(Assembly assembly);
        IActionsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);
        IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);
        IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;
        IActionsBuilder AddAction<TAction>(int version, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;
        IActionsBuilder AddAction<TAction>(ActionBuildAction<TAction> buildAction)
            where TAction : class, IAction;
    }
}
