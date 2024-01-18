using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityLocator
    {
        IBehaviorLocator Locator { get; }

        bool TryLocateActivity(ActivityId id, out IActivity activity);
    }
}
