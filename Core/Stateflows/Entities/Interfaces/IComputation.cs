namespace Stateflows.Entities;

/// <summary>Provides a static computation for a registered entity field.</summary>
/// <typeparam name="TEntityTemplate">The entity template type.</typeparam>
/// <typeparam name="TFieldType">The computed field value type.</typeparam>
public interface IComputation<in TEntityTemplate, out TFieldType>
{
    /// <summary>Computes the field value from the current entity state.</summary>
    /// <param name="entity">The entity used as the computation input.</param>
    /// <returns>The computed field value.</returns>
    static abstract TFieldType Compute(TEntityTemplate entity);
}