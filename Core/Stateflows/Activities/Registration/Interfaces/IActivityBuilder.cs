using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities;

public interface IActivityBuilder :
    IActivityUtils<IActivityBuilder>,
    IReactiveActivityBase<IActivityBuilder>,
    IActivitySpecials<IActivityBuilder>,
    IActivityEvents<IActivityBuilder>,
    IInitialBase<IActivityBuilder>,
    IFinalBase<IActivityBuilder>,
    IInputBase<IActivityBuilder>,
    IOutputBase<IActivityBuilder>,
    IAcceptEvent<IActivityBuilder>,
    ISendEventBase<IActivityBuilder>;
