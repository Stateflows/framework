using Examples.Common;
using Stateflows;
using Stateflows.Common;

namespace Blazor.Server;

public class BehaviorService
{
    private IBehaviorLocator _locator;
    private IWatcher watcher;
    public BehaviorService(IBehaviorLocator locator)
    {
        _locator = locator;
    }

    public async Task MethodAsync()
    {
        if (_locator.TryLocateBehavior(new BehaviorId("Action", "Action1", "xxxxx"), out var behavior))
        {
            // var result = await behavior.SendAsync(new SomeEvent());

            var result = await behavior.RequestAsync(new ExampleRequest());

            watcher = await behavior.WatchAsync<SomeNotification>(async n => { });
        }
    }
}