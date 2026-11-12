using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a nested structured activity's input and output flows.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
    public interface IStructuredActivityBuilder :
        IObjectFlowBase<IStructuredActivityBuilder>,
        IControlFlowBase<IStructuredActivityBuilder>,
        IActivityBase<IStructuredActivityBuilder>,
        IActivitySpecials<IStructuredActivityBuilder>,
        IInitialBase<IStructuredActivityBuilder>,
        IFinalBase<IStructuredActivityBuilder>,
        IInputBase<IStructuredActivityBuilder>,
        IOutputBase<IStructuredActivityBuilder>,
        IExceptionHandlerBase<IStructuredActivityBuilder>,
        INodeOptions<IStructuredActivityBuilder>,
        IStructuredActivityEvents<IStructuredActivityBuilder>,
        ISendEventBase<IStructuredActivityBuilder>
    {
        /// <summary>Admits new arrivals into the active invocation. Mutually exclusive with Reentrant.</summary>
        IStructuredActivityBuilder Interleave()
            => UpdateOptions(options => options | NodeOptions.Interleave);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenStructuredActivityBuilder :
        IObjectFlowBase<IOverridenStructuredActivityBuilder>,
        IControlFlowBase<IOverridenStructuredActivityBuilder>,
        IActivityBase<IOverridenStructuredActivityBuilder>,
        IActivityOverrides<IOverridenStructuredActivityBuilder>,
        IActivitySpecials<IOverridenStructuredActivityBuilder>,
        IInitialBase<IOverridenStructuredActivityBuilder>,
        IFinalBase<IOverridenStructuredActivityBuilder>,
        IInputBase<IOverridenStructuredActivityBuilder>,
        IOutputBase<IOverridenStructuredActivityBuilder>,
        IExceptionHandlerBase<IOverridenStructuredActivityBuilder>,
        INodeOptions<IOverridenStructuredActivityBuilder>,
        IStructuredActivityEvents<IOverridenStructuredActivityBuilder>,
        ISendEventBase<IOverridenStructuredActivityBuilder>,
        ISendEventOverrides<IOverridenStructuredActivityBuilder>,
        IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>
    {
        /// <summary>Admits new arrivals into the active invocation. Mutually exclusive with Reentrant.</summary>
        IOverridenStructuredActivityBuilder Interleave()
            => UpdateOptions(options => options | NodeOptions.Interleave);
    }

}
