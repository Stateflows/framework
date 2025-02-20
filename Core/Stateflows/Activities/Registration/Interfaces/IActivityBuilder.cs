using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    public interface IActivityBuilder :
        IActivityUtils<IActivityBuilder>,
        IReactiveActivity<IActivityBuilder>,
        IActivitySpecials<IActivityBuilder>,
        IActivityEvents<IActivityBuilder>,
        IInitial<IActivityBuilder>,
        IFinal<IActivityBuilder>,
        IInput<IActivityBuilder>,
        IOutput<IActivityBuilder>,
        IAcceptEvent<IActivityBuilder>,
        ISendEvent<IActivityBuilder>
    { }
}
