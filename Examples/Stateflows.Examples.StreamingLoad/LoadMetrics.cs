using System.Diagnostics;

namespace Stateflows.Examples.StreamingLoad;

public sealed class LoadMetrics
{
    private readonly object sync = new();
    private readonly Dictionary<long, long> outstanding = new();
    private readonly Queue<double> latencies = new();
    private readonly Queue<HistoryPoint> history = new();
    private Guid? runId;
    private long started, sequence, admitted, completed, normal, priority, rejected, overloadedAdmissions, abandoned;
    private long previousAdmitted, previousCompleted;
    private double latencySum, throttledSeconds, targetRate, admittedRate, completedRate;
    private double cpuPercent, workingSetMiB, heapMiB, allocatedRate;
    private int peak, gen0, gen1, gen2;
    private string state = "Idle";
    private string? error;
    private LoadSettings load = LoadScenario.Throughput.Load;
    private WorkflowSettings workflow = LoadScenario.Throughput.Workflow;
    private AppliedControl? control;

    public string State { get { lock (sync) return state; } }
    public int Outstanding { get { lock (sync) return outstanding.Count; } }

    public void Begin(Guid id, LoadSettings settings, WorkflowSettings initial)
    {
        lock (sync)
        {
            runId = id;
            started = Stopwatch.GetTimestamp();
            sequence = admitted = completed = normal = priority = rejected = overloadedAdmissions = abandoned = 0;
            previousAdmitted = previousCompleted = 0;
            latencySum = throttledSeconds = targetRate = admittedRate = completedRate = 0;
            peak = 0;
            outstanding.Clear();
            latencies.Clear();
            history.Clear();
            load = settings;
            workflow = initial;
            control = null;
            error = null;
            state = "Starting";
        }
    }

    public void SetState(string value) { lock (sync) state = value; }
    public void SetLoad(LoadSettings value)
    {
        lock (sync)
        {
            if (value.OutstandingLimit < outstanding.Count)
                throw new InvalidOperationException("Drain outstanding tokens below the new bound before lowering it.");
            load = value;
        }
    }
    public LoadSettings GetLoad() { lock (sync) return load; }
    public TimeSpan Elapsed { get { lock (sync) return started == 0 ? TimeSpan.Zero : Stopwatch.GetElapsedTime(started); } }
    public void SetTarget(double value) { lock (sync) targetRate = value; }
    public void Throttled(double seconds) { lock (sync) throttledSeconds += seconds; }
    public void Fault(Exception exception)
    {
        lock (sync)
        {
            error = exception.Message;
            state = "Faulted";
        }
    }

    public WorkToken[] Reserve(int requested)
    {
        lock (sync)
        {
            if (runId == null) throw new InvalidOperationException("No benchmark run exists.");
            var count = Math.Min(requested, Math.Max(0, load.OutstandingLimit - outstanding.Count));
            var result = new WorkToken[count];
            for (var index = 0; index < count; index++)
            {
                var token = new WorkToken(runId.Value, ++sequence, Stopwatch.GetTimestamp());
                outstanding.Add(token.Sequence, token.CreatedTimestamp);
                result[index] = token;
            }
            peak = Math.Max(peak, outstanding.Count);
            return result;
        }
    }

    public void Admitted(int count) { lock (sync) admitted += count; }
    public void AdmissionOverloaded() { lock (sync) overloadedAdmissions++; }
    public void CancelReservation(WorkToken[] tokens)
    {
        lock (sync)
        {
            foreach (var token in tokens)
                if (token.RunId == runId) outstanding.Remove(token.Sequence);
        }
    }

    public void Reject(WorkToken[] tokens)
    {
        lock (sync)
        {
            foreach (var token in tokens)
            {
                if (token.RunId == runId && outstanding.Remove(token.Sequence))
                    rejected++;
            }
        }
    }

    public void Complete(WorkToken token, bool priority)
    {
        lock (sync)
        {
            if (token.RunId != runId || !outstanding.Remove(token.Sequence, out var created))
                return;
            completed++;
            if (priority) this.priority++; else normal++;
            var milliseconds = Stopwatch.GetElapsedTime(created).TotalMilliseconds;
            latencySum += milliseconds;
            latencies.Enqueue(milliseconds);
            if (latencies.Count > 4_096) latencies.Dequeue();
        }
    }

    public void Abandon()
    {
        lock (sync)
        {
            abandoned += outstanding.Count;
            outstanding.Clear();
        }
    }

    public void Sample(double seconds, double cpu, double workingSet, double heap, double allocation, int[] collections)
    {
        lock (sync)
        {
            admittedRate = (admitted - previousAdmitted) / seconds;
            completedRate = (completed - previousCompleted) / seconds;
            previousAdmitted = admitted;
            previousCompleted = completed;
            cpuPercent = cpu;
            workingSetMiB = workingSet;
            heapMiB = heap;
            allocatedRate = allocation;
            gen0 = collections[0]; gen1 = collections[1]; gen2 = collections[2];
            history.Enqueue(new(Elapsed.TotalSeconds, admittedRate, completedRate, cpu, workingSet, outstanding.Count));
            if (history.Count > 120) history.Dequeue();
        }
    }

    public DashboardSnapshot Snapshot(int activeWorkers)
    {
        lock (sync)
        {
            var sorted = latencies.Order().ToArray();
            var p95 = sorted.Length == 0 ? 0 : sorted[(int)Math.Ceiling(sorted.Length * 0.95) - 1];
            return new(state, error, runId, load, workflow, control, admitted, completed, normal, priority,
                rejected, overloadedAdmissions, abandoned, outstanding.Count, activeWorkers, peak, targetRate, admittedRate, completedRate,
                completed == 0 ? 0 : latencySum / completed, p95, cpuPercent, workingSetMiB, heapMiB,
                allocatedRate, gen0, gen1, gen2, throttledSeconds, Elapsed.TotalSeconds, history.ToArray());
        }
    }

    public void ControlApplied(AppliedControl value)
    {
        lock (sync)
        {
            workflow = value.Settings;
            control = value;
        }
    }
}
