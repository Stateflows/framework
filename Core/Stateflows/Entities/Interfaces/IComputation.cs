namespace Stateflows.Entities;

public interface IComputation<in TEntityTemplate, out TFieldType>
{
    static abstract TFieldType Compute(TEntityTemplate entity);
}