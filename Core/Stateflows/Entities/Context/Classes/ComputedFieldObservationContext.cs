namespace Stateflows.Entities.Context.Classes
{
    internal sealed class ComputedFieldObservationContext<TTemplate>(
        EntityActionContext actionContext,
        TTemplate entity,
        string fieldName)
        : DelegatingActionContext(actionContext), IComputedFieldObservationContext<TTemplate>
    {
        public TTemplate Entity { get; } = entity;

        public string FieldName { get; } = fieldName;
    }
}