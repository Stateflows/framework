using Stateflows.Common;

namespace Stateflows.Entities;

public class FieldStateRequest<T> : IRequest<FieldState<T>>
{
    public string Name { get; set; }
}