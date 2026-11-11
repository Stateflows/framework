using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines effect-only outgoing paths that activate distinct regions of an orthogonal state.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.3.-Fork" />
    public interface IForkBuilder :
        IForkTransitions<IForkBuilder>
    { }
    
    /// <summary>Extends the outgoing paths of an inherited fork.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.3.-Fork" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenForkBuilder :
        IForkTransitions<IOverridenForkBuilder>
    { }
}
