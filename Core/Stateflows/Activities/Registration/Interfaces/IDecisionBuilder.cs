using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    public interface IDecisionBuilder : IObjectFlow<IDecisionBuilder>, IControlFlow<IDecisionBuilder>
    { }
}
