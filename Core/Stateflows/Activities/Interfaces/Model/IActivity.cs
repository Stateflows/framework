using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivity
    {
        void Build(ITypedActivityBuilder builder);
    }
}
