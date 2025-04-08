using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Transport.REST.Client;

internal class BehaviorProvider : IBehaviorProvider
{
    private readonly StateflowsApiClient _apiClient;

    public BehaviorProvider(StateflowsApiClient apiClient)
    {
        _apiClient = apiClient;

        Task.Run(async () =>
        {
            BehaviorClasses = await _apiClient.GetAvailableClassesAsync();
            BehaviorClassesChanged?.Invoke(this);
        });
    }

    public bool IsLocal => false;

    public IEnumerable<BehaviorClass> BehaviorClasses { get; set; } = new List<BehaviorClass>();

    public event ActionAsync<IBehaviorProvider>? BehaviorClassesChanged;

    public bool TryProvideBehavior(BehaviorId id, out IBehavior? behavior)
    {
        behavior = BehaviorClasses.Any(c => id.BehaviorClass == c)
            ? new Behavior(_apiClient, id)
            : null;

        return behavior != null;
    }
}
