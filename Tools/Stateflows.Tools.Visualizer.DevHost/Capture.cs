using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Tools.Visualizer.DevHost;

public sealed record Observation(
    long Sequence, Guid InvocationId, DateTimeOffset At, string Family, string Name, int Version,
    string Instance, Guid? EventId, string ExecutionMode, string ElementId,
    string Operation, string Stage, string? Status, int? TokenCount, int? RequiredTokens,
    string? ExceptionType);

public sealed class Capture(DefinitionCatalog catalog, DebugSession debugger)
{
    private const int Capacity = 4096;
    private readonly object gate = new();
    private readonly Queue<Observation> records = new();
    private long sequence;

    public void Add(IBehaviorActionContext context, string? elementId, string operation, Guid invocation,
        string stage, string? status = null, int? tokens = null, int? required = null, Exception? exception = null)
    {
        if (elementId is null)
            throw new InvalidOperationException($"The execution provider did not expose an identity for {operation}.");
        if (context is not IStateflowsContextProvider provider)
            throw new InvalidOperationException("The execution provider did not expose the selected definition version.");
        var id = context.Behavior.Id;
        var version = provider.Context.Version == 0 ? catalog.Version(id.Type, id.Name) : provider.Context.Version;
        lock (gate)
        {
            records.Enqueue(new Observation(++sequence, invocation, DateTimeOffset.UtcNow,
                id.Type == BehaviorType.StateMachine ? "stateMachine" : "activity",
                id.Name, version, id.Instance, context.ExecutionTriggerId, context.ExecutionMode.ToString(),
                elementId, operation, stage, status, tokens, required, exception?.GetType().FullName));
            if (records.Count > Capacity) records.Dequeue();
        }
    }

    public CaptureBatch Read(long after)
    {
        lock (gate)
            return new CaptureBatch(sequence, records.TryPeek(out var oldest) ? oldest.Sequence : sequence,
                records.Where(record => record.Sequence > after).ToArray());
    }

    public async Task ObserveAsync(IBehaviorActionContext context, string? elementId, string operation,
        Func<Task> next, string? completedStatus = null)
    {
        var invocation = Guid.NewGuid();
        if (elementId is not null) await debugger.BeforeAsync(context, elementId, operation);
        Add(context, elementId, operation, invocation, "started", "running");
        try
        {
            await next();
            Add(context, elementId, operation, invocation, "completed", completedStatus ?? "executed");
        }
        catch (Exception exception)
        {
            Add(context, elementId, operation, invocation, "failed", "failed", exception: exception);
            throw;
        }
    }

    public async Task<T> ObserveAsync<T>(IBehaviorActionContext context, string? elementId, string operation,
        Func<Task<T>> next, Func<T, string> status, Func<T, int?>? tokens = null, int? required = null)
    {
        var invocation = Guid.NewGuid();
        if (elementId is not null) await debugger.BeforeAsync(context, elementId, operation);
        Add(context, elementId, operation, invocation, "started", "running");
        try
        {
            var result = await next();
            Add(context, elementId, operation, invocation, "completed", status(result), tokens?.Invoke(result), required);
            return result;
        }
        catch (Exception exception)
        {
            Add(context, elementId, operation, invocation, "failed", "failed", exception: exception);
            throw;
        }
    }
}

public sealed record CaptureBatch(long Latest, long Oldest, Observation[] Records);
