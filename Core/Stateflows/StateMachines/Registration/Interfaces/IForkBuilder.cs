using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines effect-only outgoing paths that activate distinct regions of an orthogonal state.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-fork" />
    public interface IForkBuilder :
        IForkTransitions<IForkBuilder>
    { }
    
    /// <summary>Extends the outgoing paths of an inherited fork.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-fork" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenForkBuilder :
        IForkTransitions<IOverridenForkBuilder>
    { }
}
