using System;
using System.Collections.Generic;

namespace Stateflows.Activities;

/// <summary>Describes a typed boundary pin and its connected flows.</summary>
public interface IPinContext : INodeContext
{
    /// <summary>The token type accepted or exported by this pin.</summary>
    Type TokenType { get; }

    /// <summary>The payload names accepted or exported by this pin, or an empty list for a typed pin.</summary>
    IReadOnlyList<string> TokenNames => Array.Empty<string>();
}
