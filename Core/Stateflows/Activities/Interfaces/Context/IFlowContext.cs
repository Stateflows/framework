using System;
using System.Collections.Generic;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Describes an activity flow and its endpoints and token types.</summary>
    public interface IFlowContext
    {
        /// <summary>The definition flow identity, when supplied by the execution provider.</summary>
        string? Identifier => null;

        /// <summary>The node from which tokens are transferred.</summary>
        ISourceNodeContext SourceNode { get; }

        /// <summary>The node to which tokens may be transferred.</summary>
        INodeContext TargetNode { get; }

        /// <summary>The declared token type of the flow.</summary>
        Type TokenType { get; }
        
        /// <summary>The token type accepted by the target node.</summary>
        Type TargetTokenType { get; }

        /// <summary>The payload names selected by the source side of this flow, or an empty list for typed flows.</summary>
        IReadOnlyList<string> TokenNames => Array.Empty<string>();

        /// <summary>The payload names produced by this flow, or an empty list for typed flows.</summary>
        IReadOnlyList<string> TargetTokenNames => Array.Empty<string>();

        /// <summary>The configured flow weight used by flow scheduling and selection.</summary>
        int Weight { get; }
    }

    /// <summary>Flow information available before token transfer is evaluated.</summary>
    public interface IBeforeFlowContext : IFlowContext
    {
        /// <summary>The number of tokens considered for transfer on this flow.</summary>
        int TokenCount { get; }
        
        /// <summary>The number of tokens currently available at the source node.</summary>
        int SourceTokenCount { get; }
    }

    /// <summary>Flow information available after transfer evaluation.</summary>
    public interface IAfterFlowContext : IBeforeFlowContext
    {
        /// <summary>Whether the flow was activated.</summary>
        bool Activated { get; }
        
        /// <summary>The number of tokens present at the target after evaluation.</summary>
        int TargetTokenCount { get; }
    }
}
