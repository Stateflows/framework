using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityLocator
    {
        IBehaviorLocator Locator { get; }

        bool TryLocateActivity(ActivityId id, out IActivity activity);
    }
}
