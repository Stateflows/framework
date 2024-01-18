using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IActionBuilder :
        IObjectFlow<IActionBuilder>,
        IControlFlow<IActionBuilder>,
        IExceptionHandler<IActionBuilder>,
        INodeOptions<IActionBuilderWithOptions>
    { }

    public interface IActionBuilderWithOptions : 
        IObjectFlow<IActionBuilderWithOptions>,
        IControlFlow<IActionBuilderWithOptions>,
        IExceptionHandler<IActionBuilderWithOptions>
    { }

    public interface ITypedActionBuilder :
        IObjectFlow<ITypedActionBuilder>,
        IControlFlow<ITypedActionBuilder>,
        IExceptionHandler<ITypedActionBuilder>
    { }
}
