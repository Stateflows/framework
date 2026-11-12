using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

/// <summary>Fluent builder for adding and configuring action nodes in an activity graph.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.1.-Action-Node" />
public interface IActionBuilder :
    IObjectFlowBase<IActionBuilder>,
    IControlFlowBase<IActionBuilder>,
    IExceptionHandlerBase<IActionBuilder>,
    INodeOptions<IActionBuilder>;

/// <summary>Fluent builder for configuring a typed action node.</summary>
/// <typeparam name="TAction">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.1.-Action-Node" />
public interface ITypedActionBuilder<out TAction> :
    IObjectFlowBase<ITypedActionBuilder<TAction>>,
    IControlFlowBase<ITypedActionBuilder<TAction>>,
    IExceptionHandlerBase<ITypedActionBuilder<TAction>>,
    IElementBuilderBase<TAction, ITypedActionBuilder<TAction>>,
    INodeOptions<ITypedActionBuilder<TAction>>
    where TAction : class, IActionNode;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.1.-Action-Node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
public interface IOverridenActionBuilder :
    IObjectFlowBase<IOverridenActionBuilder>,
    IControlFlowBase<IOverridenActionBuilder>,
    IExceptionHandlerBase<IOverridenActionBuilder>,
    INodeOptions<IOverridenActionBuilder>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <typeparam name="TAction">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.1.-Action-Node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
public interface IOverridenTypedActionBuilder<out TAction> :
    IObjectFlowBase<IOverridenTypedActionBuilder<TAction>>,
    IControlFlowBase<IOverridenTypedActionBuilder<TAction>>,
    IExceptionHandlerBase<IOverridenTypedActionBuilder<TAction>>,
    IElementBuilderBase<TAction, IOverridenTypedActionBuilder<TAction>>,
    INodeOptions<IOverridenTypedActionBuilder<TAction>>
    where TAction : class, IActionNode;
