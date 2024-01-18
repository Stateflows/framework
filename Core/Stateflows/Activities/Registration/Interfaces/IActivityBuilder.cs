using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IActivityBuilder :
        IReactiveActivity<IActivityBuilder>,
        IActivityEvents<IActivityBuilder>,
        IInitial<IActivityBuilder>,
        IFinal<IActivityBuilder>,
        IInput<IActivityBuilder>,
        IOutput<IActivityBuilder>,
        IAcceptEvent<IActivityBuilder>,
        ISendEvent<IActivityBuilder>
    { }

    public interface ITypedActivityBuilder :
        IReactiveActivity<ITypedActivityBuilder>,
        IInitial<ITypedActivityBuilder>,
        IFinal<ITypedActivityBuilder>,
        IInput<ITypedActivityBuilder>,
        IOutput<ITypedActivityBuilder>,
        IAcceptEvent<ITypedActivityBuilder>,
        ISendEvent<ITypedActivityBuilder>
    { }
}
