using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Transport.REST.Client;

internal class BehaviorProvider : IBehaviorProvider
{
    private readonly StateflowsApiClient _apiClient;
    private readonly IServiceProvider _serviceProvider;

    public BehaviorProvider(StateflowsApiClient apiClient, IServiceProvider serviceProvider)
    {
        _apiClient = apiClient;
        _serviceProvider = serviceProvider;

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
            ? new Behavior(_apiClient, id, _serviceProvider)
            : null;

        return behavior != null;
    }
}
