namespace Stateflows.Entities;

/// <summary>Sets the value of an entity field.</summary>
/// <typeparam name="T">The field value type.</typeparam>
public class FieldState<T>
{
    /// <summary>Gets or sets the registered field name.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the new field value.</summary>
    public T Value { get; set; }
}