using Stateflows.Common;

namespace Stateflows.Entities;

[NoDeduplication]
public class FieldStateRequest<T> : IRequest<FieldState<T>>
{
    public string Name { get; set; }
}