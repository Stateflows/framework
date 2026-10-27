using Stateflows.Common;

namespace Stateflows.Entities;

/// <summary>Requests the current value of an entity field.</summary>
/// <typeparam name="T">The expected field value type.</typeparam>
[NoDeduplication]
public class FieldStateRequest<T> : IRequest<FieldState<T>>
{
    /// <summary>Gets or sets the registered field name to read.</summary>
    public string Name { get; set; }
}