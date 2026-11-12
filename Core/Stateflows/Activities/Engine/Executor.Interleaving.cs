using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Utils;

namespace Stateflows.Activities.Engine;

internal partial class Executor
{
    private sealed class InterleavingInvocation(NodeScope scope)
    {
        internal NodeScope Scope { get; } = scope;
        internal Queue<TokenHolder[]> Arrivals { get; } = new();
        internal TaskCompletionSource Changed { get; set; } = Signal();
        internal TaskCompletionSource Routed { get; } = Signal();
        internal bool Open { get; set; } = true;
        internal bool Finalized { get; set; }
        private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly Dictionary<(Node Node, NodeScope Parent), InterleavingInvocation> interleavingInvocations = [];

    private async Task<InterleavingInvocation> AdmitInterleavingAsync(
        Node node, NodeScope parent, NodeScope existingScope, Edge edge,
        TokenHolder[] input, NodeExecutionLease lease)
    {
        while (true)
        {
            Task routed;
            lock (crossingScopesLock)
            {
                if (!interleavingInvocations.TryGetValue((node, parent), out var active))
                {
                    if (parent.CancellationToken.IsCancellationRequested)
                        return null;
                    var invocation = new InterleavingInvocation(existingScope ?? parent.CreateChildScope(node, edge));
                    interleavingInvocations.Add((node, parent), invocation);
                    lease.Dispose();
                    return invocation;
                }
                if (active.Open && !active.Scope.CancellationToken.IsCancellationRequested)
                {
                    active.Arrivals.Enqueue(input);
                    active.Changed.TrySetResult();
                    return null;
                }
                routed = active.Routed.Task;
            }
            lease.Dispose();
            try { await routed.WaitAsync(parent.CancellationToken); }
            catch (OperationCanceledException) when (parent.CancellationToken.IsCancellationRequested) { return null; }
            existingScope = null;
        }
    }

    private InterleavingInvocation GetInterleaving(ActionContext context)
    {
        lock (crossingScopesLock)
            return interleavingInvocations[(context.Node, FindOwningScope(context.NodeScope.BaseNodeScope, context.Node.Parent))];
    }

    private void FinishInterleaving(Node node, NodeScope scope)
    {
        if (!node.Options.HasFlag(NodeOptions.Interleave))
            return;
        lock (crossingScopesLock)
        {
            var key = (node, FindOwningScope(scope.BaseNodeScope, node.Parent));
            if (interleavingInvocations.TryGetValue(key, out var active) && active.Scope == scope)
            {
                active.Open = false;
                interleavingInvocations.Remove(key);
                active.Routed.TrySetResult();
                if (active.Finalized && crossingScopes.TryGetValue(key, out var crossing) && crossing == scope)
                    crossingScopes.Remove(key);
            }
        }
    }

    private async Task RunInterleavingNetworkAsync(
        ActionContext context, ActivityContinuationFrame frame,
        Func<TokenHolder[], IEnumerable<Task>> admit, Func<IEnumerable<Task>> schedule)
    {
        var active = GetInterleaving(context);
        var tasks = new List<Task>();
        Exception failure = null;
        try
        {
            while (true)
            {
                Task changed;
                lock (crossingScopesLock)
                {
                    while (active.Arrivals.TryDequeue(out var arrival))
                        frame.PendingInput.Add(arrival.ToList());
                    active.Changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    changed = active.Changed.Task;
                    tasks.AddRange(active.Scope.CrossingWork);
                    active.Scope.CrossingWork.Clear();
                }

                // Admission is recorded before starting work, so checkpoint recovery retains every arrival.
                foreach (var arrival in Context.PauseAfterFiring ? [] : frame.PendingInput.ToArray())
                {
                    try { tasks.AddRange(admit(arrival.ToArray())); }
                    catch (Exception exception)
                    {
                        if (failure == null || failure is ActivityCheckpointPauseException)
                            failure = exception;
                    }
                    frame.PendingInput.Remove(arrival);
                }
                if (failure == null && !Context.PauseAfterFiring)
                    lock (crossingScopesLock)
                        tasks.AddRange(schedule());

                foreach (var task in tasks.Where(task => task.IsCompleted).ToArray())
                {
                    try { await task; }
                    catch (Exception exception)
                    {
                        if (failure == null || failure is ActivityCheckpointPauseException)
                            failure = exception;
                    }
                    tasks.Remove(task);
                }

                lock (crossingScopesLock)
                {
                    if (failure != null)
                        active.Open = false;
                    if (active.Arrivals.Count != 0)
                        continue;
                    if (active.Scope.CrossingWork.Count != 0)
                        continue;
                    if (tasks.Count == 0)
                    {
                        // Iterative scheduling may have another partition after the one just collected.
                        if (failure == null && !Context.PauseAfterFiring)
                            tasks.AddRange(schedule());
                        if (tasks.Count == 0)
                        {
                            active.Open = false;
                            active.Scope.AcceptingCrossings = false;
                            active.Scope.CrossingClosed = failure != null ||
                                Context.IsNodeCompleted(context.Node, active.Scope);
                            break;
                        }
                    }
                }
                await Task.WhenAny(tasks.Append(changed));
            }
            if (failure != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            if (Context.PauseAfterFiring)
                throw new ActivityCheckpointPauseException();
        }
        catch (ActivityCheckpointPauseException)
        {
            Context.AddContinuationFrame(frame);
            throw;
        }
    }

    private async Task ExecuteInterleavedStructuredAsync(ActionContext context)
    {
        var scope = context.NodeScope;
        var node = context.Node;
        if (node.Anchored && HasSingleAnchoredInvocation(node))
        {
            if (Context.NodeThreads.TryGetValue(node.Identifier, out var storedThread))
                scope.ThreadId = storedThread;
            else
                Context.NodeThreads[node.Identifier] = scope.ThreadId;
        }
        var previousFrame = scope.ContinuationFrame ??
            Context.GetInterleavedIdleFrame(node.Identifier, scope.ThreadId);
        var idle = context.ResumeFrame == null && (previousFrame != null || Context.NodesToExecute.Contains(node));
        var frame = context.ResumeFrame ?? previousFrame ?? CreateFrame(context, ActivityContinuationKind.Structured);
        if (context.ResumeFrame == null && previousFrame != null)
            frame.ParentFrameId = currentFrame.Value?.Id;
        scope.ContinuationFrame = frame;
        currentFrame.Value = frame;
        if (context.ResumeFrame == null && previousFrame == null && !Context.NodesToExecute.Contains(node))
            await DoInitializeNodeAsync(node, context);

        var started = false;
        await RunInterleavingNetworkAsync(context, frame, input => [FeedInterleavedInputAsync(context, input)], () =>
        {
            if (started)
                return [];
            started = true;
            if (idle)
                return [
                    DoExecuteStructureAsync(node, scope, resume: true),
                    FeedInterleavedInputAsync(context, context.InputTokens.ToArray())
                ];
            var resuming = context.ResumeFrame != null && frame.NetworkStarted;
            frame.NetworkStarted = true;
            return [DoExecuteStructureAsync(node, scope, resuming ? null : frame.InputTokens, resume: resuming)];
        });

        context.OutputTokens.AddRange(node.OutputPins.SelectMany(pin => Context.TakeOutputTokens(pin.Identifier, scope.ThreadId)));
        context.OutputTokens.AddRange(node.ExceptionHandlers.SelectMany(handler => Context.GetOutputTokens(handler.Identifier, scope.ThreadId)));
        var finalized = Context.IsNodeCompleted(node, scope);
        Context.SetInterleavedIdleFrame(frame, !finalized);
        GetInterleaving(context).Finalized = finalized;
        if (finalized)
        {
            await DoFinalizeNodeAsync(node, context);
            foreach (var edge in node.Graph.AllEdges.Values.Where(edge => edge.Owner == node))
                Context.ClearStream(edge.Identifier, RootContext.GetStreamThreadId(edge, scope));
            Context.NodeThreads.Remove(node.Identifier);
        }
    }

    private Task FeedInterleavedInputAsync(ActionContext context, TokenHolder[] input)
    {
        if (context.NodeScope.CancellationToken.IsCancellationRequested)
            return Task.CompletedTask;
        if (!context.Node.AcceptsInput(input.Where(token => token.PayloadType != typeof(ControlToken))))
            throw new InvalidOperationException($"Activity scope '{context.Node.Name}' received tokens unsupported by its input pins.");
        return Task.WhenAll(context.Node.InputPins
            .Where(pin => input.Any(token => pin.PinTokenType.IsAssignableFrom(token.ActualPayloadType)))
            .Select(pin => DoHandleNodeAsync(pin, context.Edge, context.NodeScope,
                input.Where(token => pin.PinTokenType.IsAssignableFrom(token.ActualPayloadType)).ToArray())));
    }

    private async Task ExecuteInterleavedPartitionsAsync<TToken>(ActionContext context, bool parallel)
    {
        var node = context.Node;
        var frame = context.ResumeFrame ?? CreatePartitionedFrame<TToken>(context,
            parallel ? ActivityContinuationKind.Parallel : ActivityContinuationKind.Iterative, !parallel);
        currentFrame.Value = frame;
        context.NodeScope.ContinuationFrame = frame;
        var iterationThreadId = frame.IterationThreadId ?? frame.Partitions.FirstOrDefault()?.ThreadId ?? Guid.NewGuid();
        if (!parallel)
            frame.IterationThreadId = iterationThreadId;
        frame.Partitions.RemoveAll(partition => partition.Completed);
        if (context.ResumeFrame == null)
            await DoInitializeNodeAsync(node, context);
        var running = new HashSet<ActivityContinuationPartition>();

        IEnumerable<Task> Admit(TokenHolder[] input)
        {
            var typed = input.OfTokenType<TToken>().ToArray();
            var rest = input.Except(typed).ToList();
            lock (crossingScopesLock)
                foreach (var chunk in typed.Partition(node.ChunkSize))
                    frame.Partitions.Add(new ActivityContinuationPartition
                    {
                        ThreadId = parallel ? Guid.NewGuid() : iterationThreadId,
                        Selection = chunk.ToList(),
                        ArrivalInput = rest
                    });
            return [];
        }

        async Task Run(ActivityContinuationPartition partition)
        {
            var resuming = partition.Started;
            partition.Started = true;
            bool result;
            List<TokenHolder> output;
            await using (var partitionScope = CreatePartitionScope(context, partition))
            {
                result = await DoExecuteStructureAsync(node, partitionScope,
                    resuming ? null : (partition.ArrivalInput ?? frame.RestOfInput).Concat(partition.Selection).ToArray(),
                    resuming ? null : partition.Selection, resuming);
                output = CollectPartitionOutput(node, partition.ThreadId);
            }
            lock (crossingScopesLock)
            {
                frame.AccumulatedOutput.AddRange(output);
                partition.Completed = true;
                frame.Partitions.Remove(partition);
                running.Remove(partition);
                if (!parallel && result && node.FinalNode != null)
                    frame.Completed = true;
            }
        }

        IEnumerable<Task> Schedule()
        {
            if (frame.Completed || context.NodeScope.CancellationToken.IsCancellationRequested ||
                (!parallel && running.Count != 0))
                return [];
            var next = frame.Partitions.Where(partition => !partition.Completed && !running.Contains(partition));
            if (parallel && node.MaxConcurrency > 0)
                next = next.Take(Math.Max(0, node.MaxConcurrency - running.Count));
            var partitions = (parallel ? next : next.Take(1)).ToArray();
            foreach (var partition in partitions)
                running.Add(partition);
            return partitions.Select(partition => parallel && !SessionOwned
                ? Task.Run(() => Run(partition)) : Run(partition)).ToArray();
        }

        await RunInterleavingNetworkAsync(context, frame, Admit, Schedule);
        context.OutputTokens.AddRange(frame.AccumulatedOutput);
        GetInterleaving(context).Finalized = true;
        await DoFinalizeNodeAsync(node, context);
    }
}
