using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IActionBuilder :
        IObjectFlowBase<IActionBuilder>,
        IControlFlowBase<IActionBuilder>,
        IExceptionHandlerBase<IActionBuilder>,
        INodeOptions<IActionBuilderWithOptions>
    { }

    public interface IActionBuilderWithOptions : 
        IObjectFlowBase<IActionBuilderWithOptions>,
        IControlFlowBase<IActionBuilderWithOptions>,
        IExceptionHandlerBase<IActionBuilderWithOptions>
    { }

    public interface ITypedActionBuilder :
        IObjectFlowBase<ITypedActionBuilder>,
        IControlFlowBase<ITypedActionBuilder>,
        IExceptionHandlerBase<ITypedActionBuilder>
    { }
}
