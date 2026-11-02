using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class FluentApiCoverage : StateflowsTestClass
{
    private readonly TaskCompletionSource<int> receivedEvent = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<int> publishedEvent = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int resetActivityExecutions;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddActivities(activities => activities
            .AddActivity("fluent-event-target", activity => activity
                .AddInitial(initial => initial.AddControlFlow("receive"))
                .AddAcceptEventAction<FluentEvent>("receive", context =>
                {
                    receivedEvent.TrySetResult(context.Event.Value);
                    return Task.CompletedTask;
                })
            )
            .AddActivity("fluent-send-event", activity => activity
                .AddInitial(initial => initial.AddControlFlow("send"))
                .AddSendEventAction<FluentEvent>(
                    "send",
                    _ => Task.FromResult(new FluentEvent { Value = 23 }),
                    _ => Task.FromResult<BehaviorId>(new ActivityId("fluent-event-target", "receiver")),
                    action => action.AddControlFlow<FinalNode>()
                )
                .AddFinal()
            )
            .AddActivity("fluent-publish-event", activity => activity
                .AddInitial(initial => initial.AddControlFlow("publish"))
                .AddPublishEventAction<FluentEvent>(
                    "publish",
                    _ => Task.FromResult(new FluentEvent { Value = 42 }),
                    action => action.AddControlFlow<FinalNode>()
                )
                .AddFinal()
            )
            .AddActivity("fluent-reset", activity => activity
                .AddInitial(initial => initial.AddControlFlow("run"))
                .AddAction("run", _ =>
                {
                    resetActivityExecutions++;
                    return Task.CompletedTask;
                }, action => action.AddControlFlow<FinalNode>())
                .AddFinal()
            )
        );
    }

    [TestMethod]
    public async Task FluentSendEventActionDeliversToTheSelectedActivity()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(
            new ActivityId("fluent-event-target", "receiver"),
            out var receiver
        ));
        Assert.AreEqual(EventStatus.Initialized, (await receiver.SendAsync(new Initialize())).Status);
        Assert.IsTrue((await receiver.GetStatusAsync()).Response.ActiveNodes.Contains("receive"));

        Assert.IsTrue(ActivityLocator.TryLocateActivity(
            new ActivityId("fluent-send-event", "sender"),
            out var sender
        ));
        Assert.AreEqual(EventStatus.Initialized, (await sender.SendAsync(new Initialize())).Status);
        Assert.AreEqual(23, await receivedEvent.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(BehaviorStatus.Finalized, (await sender.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task FluentPublishEventActionEmitsItsEvent()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(
            new ActivityId("fluent-publish-event", "publisher"),
            out var publisher
        ));
        await using var watcher = await publisher.WatchAsync<FluentEvent>(
            @event => publishedEvent.TrySetResult(@event.Value)
        );

        Assert.AreEqual(EventStatus.Initialized, (await publisher.SendAsync(new Initialize())).Status);
        Assert.AreEqual(42, await publishedEvent.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(BehaviorStatus.Finalized, (await publisher.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task ActivityCanBeResetAndInitializedAgain()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("fluent-reset", "instance"), out var activity));

        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        Assert.AreEqual(1, resetActivityExecutions);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);

        Assert.AreEqual(EventStatus.Consumed, (await activity.ResetAsync(ResetMode.KeepVersionAndSubscriptions)).Status);
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        Assert.AreEqual(2, resetActivityExecutions);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    public sealed class FluentEvent
    {
        public int Value { get; set; }
    }
}
