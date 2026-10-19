using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class BehaviorFactoryTests
{
    [TestMethod]
    public void ActionAndActivityLocatorsUseTheRegisteredBehaviorFactory()
    {
        var factory = new RecordingBehaviorFactory();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IBehaviorFactory>(factory);
        services.AddStateflows(stateflows => stateflows
            .AddActions(actions => actions.AddAction("factory-action", _ => Task.CompletedTask))
            .AddActivities(activities => activities.AddActivity(
                "factory-activity",
                activity => activity
                    .AddInitial(initial => initial.AddControlFlow<FinalNode>())
                    .AddFinal())));

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var actionLocator = scope.ServiceProvider.GetRequiredService<IActionLocator>();
        var activityLocator = scope.ServiceProvider.GetRequiredService<IActivityLocator>();

        Assert.IsTrue(actionLocator.TryLocateAction(new ActionId("factory-action", "test"), out var action));
        try
        {
            Assert.IsTrue(activityLocator.TryLocateActivity(new ActivityId("factory-activity", "test"), out var activity));
            try
            {
                CollectionAssert.AreEquivalent(
                    new[] { "factory-action", "factory-activity" },
                    factory.CreatedBehaviors.Select(id => id.Name).ToArray());
            }
            finally
            {
                activity.Dispose();
            }
        }
        finally
        {
            action.Dispose();
        }
    }

    private sealed class RecordingBehaviorFactory : IBehaviorFactory
    {
        private readonly List<BehaviorId> createdBehaviors = [];

        public IReadOnlyList<BehaviorId> CreatedBehaviors
        {
            get
            {
                lock (createdBehaviors)
                {
                    return createdBehaviors.ToArray();
                }
            }
        }

        public IBehavior CreateBehavior(BehaviorId behaviorId)
        {
            lock (createdBehaviors)
            {
                createdBehaviors.Add(behaviorId);
            }

            return new TestBehavior(behaviorId);
        }
    }

    private sealed class TestBehavior(BehaviorId id) : IBehavior
    {
        public BehaviorId Id => id;

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => throw new NotSupportedException();

        public Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(
            IRequest<TResponseEvent> request,
            IDictionary<string, EventHeader>? headers = null)
            => throw new NotSupportedException();

        public Task<IWatcher> WatchAsync<TNotification>(
            Func<TNotification, Task> asyncHandler,
            DateTime? replayNotificationsSince = null)
            => throw new NotSupportedException();

        public Task<IWatcher> WatchAsync(
            string[] notificationNames,
            Func<EventHolder, Task> asyncHandler,
            DateTime? replayNotificationsSince = null)
            => throw new NotSupportedException();

        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
            => throw new NotSupportedException();

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(
            string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
