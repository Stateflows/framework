using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

public enum ContextSerializationStatus
{
    Pending,
    Complete
}

[TestClass]
public class ContextValueSerialization : StateflowsTestClass
{
    private bool statusRead;
    private ContextSerializationStatus status;
    private bool numberRead;
    private int? number;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddActivities(activities => activities
            .AddActivity("contextValueSerialization", activity => activity
                .AddInitial(initial => initial.AddControlFlow("serialize"))
                .AddAction("serialize", async context =>
                {
                    await context.Behavior.Values.SetAsync("status", ContextSerializationStatus.Complete);
                    await context.Behavior.Values.SetAsync("number", 42);
                    (statusRead, status) = await context.Behavior.Values.TryGetAsync<ContextSerializationStatus>("status");
                    (numberRead, number) = await context.Behavior.Values.TryGetAsync<int?>("number");
                })
            )
        );
    }

    [TestMethod]
    public async Task BehaviorContextValuesPreserveEnumAndNullableIntegerValuesAcrossSerialization()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("contextValueSerialization", "x"), out var activity));

        var result = await activity.SendAsync(new Initialize());

        Assert.AreEqual(EventStatus.Initialized, result.Status);
        Assert.IsTrue(statusRead);
        Assert.AreEqual(ContextSerializationStatus.Complete, status);
        Assert.IsTrue(numberRead);
        Assert.AreEqual(42, number);
    }
}
