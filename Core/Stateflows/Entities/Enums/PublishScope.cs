using System;

namespace Stateflows.Entities.Enums;

[Flags]
public enum PublishScope
{
    None = 0,
    Self = 1,
    Parent = 2,
    Owner = 4
}