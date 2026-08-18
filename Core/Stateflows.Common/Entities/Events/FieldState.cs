namespace Stateflows.Entities;

public class FieldState<T>
{
    public string Name { get; set; }
    public T Value { get; set; }
}