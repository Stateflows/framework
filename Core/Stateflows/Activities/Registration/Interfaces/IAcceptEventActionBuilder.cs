using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IAcceptEventActionBuilder :
        IObjectFlow<IAcceptEventActionBuilder>,
        IControlFlow<IAcceptEventActionBuilder>,
        IExceptionHandler<IActionBuilder>
    { }
}
