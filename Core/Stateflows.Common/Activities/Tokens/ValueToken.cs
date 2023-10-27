namespace Stateflows.Activities
{
    public class ValueToken<T> : Token
    {
        public ValueToken()
        {
            Value = default;
        }

        public T Value { get; set; }
    }
}
