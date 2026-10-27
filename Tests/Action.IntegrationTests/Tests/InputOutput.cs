using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Common;

namespace Action.IntegrationTests.Tests;

[TestClass]
public class InputOutput : StateflowsTestClass
{
    private int[] receivedNumbers = [];
    private string[] receivedLabels = [];
    private int executionCount;

    [TestInitialize]
    public override void Initialize()
    {
        receivedNumbers = [];
        receivedLabels = [];
        executionCount = 0;
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddActions(actions => actions
            .AddAction("batch", context =>
            {
                receivedNumbers = context.GetTokensOfType<int>().ToArray();
                receivedLabels = context.GetTokensOfType<string>().ToArray();
                executionCount++;
                context.Output(receivedNumbers.Sum());
                context.Output("processed");
                return Task.CompletedTask;
            })
            .AddAction("variance", context =>
            {
                OutputBase token = new OutputDerived();
                context.Output(token);
                return Task.CompletedTask;
            })
            .AddAction("empty", context => Task.CompletedTask)
            .AddAction("failed", context =>
            {
                context.Output(42);
                throw new InvalidOperationException("Execution failed after producing a token.");
            }));

    [TestMethod]
    public async Task BuilderInputUnpacksMixedBatchAndReturnsOutput()
    {
        using var action = LocateAction();

        var result = await action.SendInputAsync(input => input
            .AddRange(2, 3, 5)
            .Add("order-42"));

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual(1, executionCount);
        CollectionAssert.AreEqual(new[] { 2, 3, 5 }, receivedNumbers);
        CollectionAssert.AreEqual(new[] { "order-42" }, receivedLabels);
        AssertOutput(result, 10);
    }

    [TestMethod]
    public async Task GenericInputUnpacksBatchAndReturnsOutput()
    {
        using var action = LocateAction();

        var result = await action.SendInputAsync<int>(2, 3, 5);

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual(1, executionCount);
        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 }, receivedNumbers,
            "Generic input must expose the batch contents as individual Action input tokens.");
        AssertOutput(result, 10);
    }

    [TestMethod]
    public async Task ExecuteUsesEmptyBatchAndReturnsOutput()
    {
        using var action = LocateAction();

        var result = await action.ExecuteAsync();

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual(1, executionCount);
        Assert.IsEmpty(receivedNumbers);
        Assert.IsEmpty(receivedLabels);
        AssertOutput(result, 0);
    }

    [TestMethod]
    [DataRow("execute", false)]
    [DataRow("execute", true)]
    [DataRow("builder", false)]
    [DataRow("builder", true)]
    [DataRow("generic", false)]
    [DataRow("generic", true)]
    [DataRow("initialize", false)]
    [DataRow("initialize", true)]
    [DataRow("event", false)]
    [DataRow("event", true)]
    public async Task ExecutionPublishesOutput(string invocation, bool typedWatch)
    {
        using var action = LocateAction();
        var notification = new TaskCompletionSource<int[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        await using var watcher = typedWatch
            ? await action.WatchOutputAsync<int>(
                tokens => notification.TrySetResult(tokens.ToArray()))
            : await action.WatchOutputAsync(
                tokens => notification.TrySetResult(tokens.GetAllOfType<int>().ToArray()));

        SendResult result = invocation switch
        {
            "execute" => await action.ExecuteAsync(),
            "builder" => await action.SendInputAsync(input => input.AddRange(2, 3, 5)),
            "generic" => await action.SendInputAsync<int>(2, 3, 5),
            "initialize" => await action.SendAsync(new Initialize()),
            "event" => await action.SendAsync(10),
            _ => throw new ArgumentOutOfRangeException(nameof(invocation))
        };

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual(1, executionCount);

        var completed = await Task.WhenAny(notification.Task, Task.Delay(TimeSpan.FromSeconds(3)));
        Assert.AreSame(
            notification.Task, completed,
            $"{invocation} execution did not publish output to the " +
            $"{(typedWatch ? "typed" : "untyped")} watcher.");

        CollectionAssert.AreEqual(
            new[] { invocation is "execute" or "initialize" ? 0 : 10 },
            await notification.Task);
    }

    [TestMethod]
    public async Task OutputNotificationsUseRuntimeTypesAndIncludeBaseTypesAndInterfaces()
    {
        using var action = LocateAction("variance");
        var derived = new TaskCompletionSource<OutputDerived[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var bases = new TaskCompletionSource<OutputBase[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var interfaces = new TaskCompletionSource<IOutputToken[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        await using var derivedWatcher = await action.WatchOutputAsync<OutputDerived>(
            tokens => derived.TrySetResult(tokens.ToArray()));
        await using var baseWatcher = await action.WatchOutputAsync<OutputBase>(
            tokens => bases.TrySetResult(tokens.ToArray()));
        await using var interfaceWatcher = await action.WatchOutputAsync<IOutputToken>(
            tokens => interfaces.TrySetResult(tokens.ToArray()));

        var result = await action.ExecuteAsync();

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.IsNotNull(result.Response);
        Assert.HasCount(1, result.Response.GetOfType<OutputDerived>());
        await Task.WhenAll(derived.Task, bases.Task, interfaces.Task)
            .WaitAsync(TimeSpan.FromSeconds(3));
        Assert.HasCount(1, await derived.Task);
        Assert.HasCount(1, await bases.Task);
        Assert.HasCount(1, await interfaces.Task);
    }

    [TestMethod]
    [DataRow("empty", EventStatus.Consumed)]
    [DataRow("failed", EventStatus.Failed)]
    public async Task EmptyOrFailedExecutionDoesNotPublishOutput(string actionName, EventStatus expectedStatus)
    {
        using var action = LocateAction(actionName);
        var published = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        await using var watcher = await action.WatchOutputAsync(
            _ => published.TrySetResult(true));

        var result = await action.ExecuteAsync();

        Assert.AreEqual(expectedStatus, result.Status);
        if (expectedStatus == EventStatus.Consumed)
        {
            Assert.IsNotNull(result.Response);
            Assert.IsEmpty(result.Response.Tokens);
        }

        var completed = await Task.WhenAny(published.Task, Task.Delay(TimeSpan.FromMilliseconds(200)));
        Assert.AreNotSame(published.Task, completed, "Empty or failed execution must not publish output.");
    }

    public interface IOutputToken;
    public class OutputBase : IOutputToken;
    public sealed class OutputDerived : OutputBase;

    private IActionBehavior LocateAction(string name = "batch")
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(
            new ActionId(name, Guid.NewGuid().ToString()), out var action));
        return action;
    }

    private static void AssertOutput(RequestResult<TokensOutput> result, int expectedTotal)
    {
        Assert.IsNotNull(result.Response, "Execution must return a TokensOutput response.");
        CollectionAssert.AreEqual(new[] { expectedTotal }, result.Response.GetOfType<int>().ToArray());
        CollectionAssert.AreEqual(new[] { "processed" }, result.Response.GetOfType<string>().ToArray());
    }
}
