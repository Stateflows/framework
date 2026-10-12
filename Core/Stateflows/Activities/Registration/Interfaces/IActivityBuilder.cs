using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;

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
    IAcceptEventBase<IActivityBuilder>,
    ISendEventBase<IActivityBuilder>,
    IPublishEventBase<IActivityBuilder>,
    IEntityOwnerBase<IActivityBuilder>,
    IActivityOverride<IOverridenActivityBuilder>;

public interface IOverridenActivityBuilder :
    IActivityUtils<IOverridenActivityBuilder>,
    IReactiveActivityBase<IOverridenActivityBuilder>,
    IReactiveActivityOverrides<IOverridenActivityBuilder>,
    IActivitySpecials<IOverridenActivityBuilder>,
    IActivityEvents<IOverridenActivityBuilder>,
    IInitialBase<IOverridenActivityBuilder>,
    IFinalBase<IOverridenActivityBuilder>,
    IInputBase<IOverridenActivityBuilder>,
    IOutputBase<IOverridenActivityBuilder>,
    IAcceptEventBase<IOverridenActivityBuilder>,
    IAcceptEventOverrides<IOverridenActivityBuilder>,
    ISendEventBase<IOverridenActivityBuilder>,
    ISendEventOverrides<IOverridenActivityBuilder>,
    IPublishEventBase<IOverridenActivityBuilder>,
    IEntityOwnerBase<IOverridenActivityBuilder>,
    IPublishEventOverrides<IOverridenActivityBuilder>,
    IActivitySpecialsOverrides<IOverridenActivityBuilder>;
