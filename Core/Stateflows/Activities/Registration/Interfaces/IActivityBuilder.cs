using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IActivityBuilder :
        IActivity<IActivityBuilder>,
        IActivityEvents<IActivityBuilder>,
        IInitial<IActivityBuilder>,
        IFinal<IActivityBuilder>,
        IInput<IActivityBuilder>,
        IOutput<IActivityBuilder>,
        IAcceptEvent<IActivityBuilder>,
        ISendEvent<IActivityBuilder>
    { }
}
