using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Streams;

/// <summary>Kinds of structural invocations that can be suspended by a checkpoint barrier.</summary>
public enum ActivityContinuationKind
{
    /// <summary>A structured activity node.</summary>
    Structured,
    /// <summary>An iterative activity node processing its input partitions one after another.</summary>
    Iterative,
    /// <summary>A parallel activity node processing its input partitions concurrently.</summary>
    Parallel
}

/// <summary>
/// Persisted state of a structural node invocation (structured, iterative or parallel activity) that was
/// suspended by a checkpoint barrier. The frame retains the invocation's original input, its partition
/// progress and accumulated output, so recovery continues the same invocation without re-running its
/// initialization or already completed partitions.
/// </summary>
public sealed class ActivityContinuationFrame
{
    /// <summary>Unique identifier of the suspended invocation.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The enclosing suspended invocation, or <c>null</c> when the node lives in the Activity's root scope.</summary>
    public Guid? ParentFrameId { get; set; }

    /// <summary>The structural node being executed.</summary>
    public string NodeIdentifier { get; set; } = string.Empty;

    /// <summary>The incoming edge which activated the structural node.</summary>
    public string? UpstreamEdgeIdentifier { get; set; }

    /// <summary>The execution partition containing the structural node itself.</summary>
    public Guid OuterThreadId { get; set; }

    /// <summary>Indicates that the structural node belongs to the Activity's root execution scope.</summary>
    public bool IsRootThread { get; set; }

    /// <summary>The kind of structural invocation.</summary>
    public ActivityContinuationKind Kind { get; set; }

    /// <summary>Whether the suspended invocation belongs to streamed work.</summary>
    public bool IsStreamed { get; set; }

    /// <summary>The structural node's original input tokens.</summary>
    public List<TokenHolder> InputTokens { get; set; } = [];

    /// <summary>Input tokens which are not partitioned and are delivered to every partition.</summary>
    public List<TokenHolder> RestOfInput { get; set; } = [];

    /// <summary>The input partitions of an iterative or parallel invocation, in original order.</summary>
    public List<ActivityContinuationPartition> Partitions { get; set; } = [];

    /// <summary>Output collected from partitions that already completed.</summary>
    public List<TokenHolder> AccumulatedOutput { get; set; } = [];

    /// <summary>Set when an iterative invocation completed early because its final node was reached.</summary>
    public bool Completed { get; set; }
}

/// <summary>One input partition of a suspended iterative or parallel invocation.</summary>
public sealed class ActivityContinuationPartition
{
    /// <summary>The execution partition used by this input partition.</summary>
    public Guid ThreadId { get; set; } = Guid.NewGuid();

    /// <summary>The tokens selected for this partition.</summary>
    public List<TokenHolder> Selection { get; set; } = [];

    /// <summary>Whether this partition started executing.</summary>
    public bool Started { get; set; }

    /// <summary>Whether this partition finished and its output was accumulated.</summary>
    public bool Completed { get; set; }
}
