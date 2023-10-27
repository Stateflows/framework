using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IEventActionBuilder :
        IObjectFlow<IEventActionBuilder>, 
        IControlFlow<IEventActionBuilder>
    {
    }
}
