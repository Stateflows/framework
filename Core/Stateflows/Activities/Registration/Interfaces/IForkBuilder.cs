using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IForkBuilder : IObjectFlow<IForkBuilder>, IControlFlow<IForkBuilder>
    { }
}
