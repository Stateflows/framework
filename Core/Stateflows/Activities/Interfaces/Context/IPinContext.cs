using System;

namespace Stateflows.Activities;

/// <summary>Describes a typed boundary pin and its connected flows.</summary>
public interface IPinContext : INodeContext
{
    /// <summary>The token type accepted or exported by this pin.</summary>
    Type TokenType { get; }
}
