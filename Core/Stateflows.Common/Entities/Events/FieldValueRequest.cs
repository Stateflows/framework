using Stateflows.Common;

namespace Stateflows.Entities
{
    public abstract class FieldValueEvent
    {
        public string FieldName { get; set; }
    }

    public sealed class FieldValueRequest<TFieldValue> : FieldValueEvent, IRequest<TFieldValue>;
}
