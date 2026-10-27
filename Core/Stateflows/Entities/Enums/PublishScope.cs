using System;

namespace Stateflows.Entities.Enums;

/// <summary>Specifies which related behaviors receive projection publications.</summary>
[Flags]
public enum PublishScope
{
    /// <summary>Do not publish projection updates to related behaviors.</summary>
    None = 0,
    /// <summary>Publish projection updates to the entity itself.</summary>
    Self = 1,
    /// <summary>Publish projection updates to the containing parent behavior.</summary>
    Parent = 2,
    /// <summary>Publish projection updates to the owning behavior.</summary>
    Owner = 4
}