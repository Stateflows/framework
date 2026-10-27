using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Activities;

/// <summary>Fluent builder for defining an activity and its graph.</summary>
/// <remarks>Use the reactive registration APIs to construct nodes and flows. Activity overrides can replace
/// elements from an inherited definition.</remarks>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activities">Activities documentation</seealso>
/// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups">Behavioral groups documentation
/// </seealso>
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

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <remarks>Only elements explicitly selected through the override APIs are replaced; the remaining inherited
/// definition is retained.</remarks>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
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
