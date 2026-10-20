using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Interfaces;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    [TestMethod]
    public async Task BehaviorTurnWaitsForPublicationPersistence()
    {
        var filter = new PublicationPersistenceFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), filter);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "publication-barrier");
            using var scope = host.Services.CreateScope();
            using var behavior = await LocateBehaviorAsync(
                scope.ServiceProvider.GetRequiredService<IBehaviorLocator>(), id, WatchTimeout);
            var send = behavior.SendAsync(new PublishCustomNotification { Text = "persist-before-return" });
            await filter.Entered.Task.WaitAsync(WatchTimeout);
            Assert.IsFalse(send.IsCompleted, "The behavior must not return before publication persistence completes.");
            filter.Release.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, (await send.WaitAsync(WatchTimeout)).Status);
            var history = (await behavior.GetNotificationsAsync<CustomNotification>(DateTime.MinValue)).ToArray();
            Assert.AreEqual(1, history.Length);
            Assert.AreEqual("persist-before-return", history[0].Text);
        }
        finally
        {
            filter.Release.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task PublicationFailureKeepsSuccessfulResultAndRetriesFromOutboxReminder()
    {
        var filter = new PublicationPersistenceFilter { Reject = true };
        using var host = CreateHost(GetFreeTcpPorts(2), filter);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "publication-failure");
            using var scope = host.Services.CreateScope();
            using var behavior = await LocateBehaviorAsync(
                scope.ServiceProvider.GetRequiredService<IBehaviorLocator>(), id, WatchTimeout);
            filter.Release.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed,
                (await behavior.SendAsync(new PublishCustomNotification()).WaitAsync(WatchTimeout)).Status);
            Assert.IsFalse((await behavior.GetNotificationsAsync<CustomNotification>(DateTime.MinValue)).Any());
            filter.Reject = false;
            var client = host.Services.GetRequiredService<IClusterClient>();
            await client.GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id))
                .ReceiveReminder(StateflowsBehavior.OutboxReminderName, default);
            Assert.AreEqual(BehaviorStatus.Initialized,
                (await behavior.RequestAsync(new BehaviorInfoRequest()).WaitAsync(WatchTimeout)).Response.BehaviorStatus,
                "Post-commit publication failure cannot roll back initialization.");
            Assert.AreEqual(1, (await behavior.GetNotificationsAsync<CustomNotification>(DateTime.MinValue)).Count());
        }
        finally
        {
            filter.Release.TrySetResult();
            await host.StopAsync();
        }
    }

    private sealed class PublicationPersistenceFilter : IIncomingGrainCallFilter
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Reject;

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.Grain is ISubscriptionsGrain &&
                context.InterfaceMethod.Name is nameof(ISubscriptionsGrain.PublishAsync) or nameof(ISubscriptionsGrain.PublishOutboxAsync))
            {
                Entered.TrySetResult();
                await Release.Task;
                if (Reject) throw new InvalidOperationException("Expected publication persistence failure.");
            }
            await context.Invoke();
        }
    }
}
