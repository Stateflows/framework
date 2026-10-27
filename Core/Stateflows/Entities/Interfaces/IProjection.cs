namespace Stateflows.Entities;

/// <summary>Provides a static projection operation for an entity template.</summary>
/// <typeparam name="TEntityTemplate">The entity template type.</typeparam>
/// <typeparam name="TProjectionTemplate">The projection value type.</typeparam>
public interface IProjection<in TEntityTemplate, out TProjectionTemplate>
{
    /// <summary>Calculates a projection from the entity's current state.</summary>
    /// <param name="template">The entity to project.</param>
    /// <returns>The computed projection value.</returns>
    static abstract TProjectionTemplate Project(TEntityTemplate template);
}