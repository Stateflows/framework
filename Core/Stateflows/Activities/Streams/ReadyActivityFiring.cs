using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Streams;

/// <summary>Identifies a routed Activity firing that was ready when a checkpoint barrier paused execution.</summary>
public sealed class ReadyActivityFiring
{
    /// <summary>The graph node to execute when the Activity resumes.</summary>
    public string NodeIdentifier { get; set; } = string.Empty;

    /// <summary>The incoming edge which made the node ready.</summary>
    public string? UpstreamEdgeIdentifier { get; set; }

    /// <summary>The suspended structural invocation which owns this firing, or <c>null</c> for the Activity's root scope.</summary>
    public Guid? FrameId { get; set; }

    /// <summary>The execution partition whose marking contains this firing's inputs.</summary>
    public Guid ThreadId { get; set; }

    /// <summary>Indicates that the firing belongs to the Activity's root execution scope.</summary>
    public bool IsRootThread { get; set; }
}
