using System;

namespace Stateflows.Entities.Enums;

/// <summary>Specifies the behaviors on whose behalf notifications are published.</summary>
[Flags]
public enum PublishScope
{
    /// <summary>Do not publish notifications.</summary>
    None = 0,
    /// <summary>Publish on behalf of the current behavior.</summary>
    Self = 1,
    /// <summary>Publish on behalf of the containing parent behavior.</summary>
    Parent = 2,
    /// <summary>Publish on behalf of the owning behavior.</summary>
    Owner = 4
}