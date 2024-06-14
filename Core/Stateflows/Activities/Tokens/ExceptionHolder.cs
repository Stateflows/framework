using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class ExceptionHolder : TokenHolder
    {
        public Exception Exception { get; set; }
    }

    public class ExceptionHolder<T> : ExceptionHolder
        where T : Exception
    {
        public ExceptionHolder()
        {
            Exception = default;
        }

        public override string Name => name ??= typeof(T).GetReadableName();

        public T TypedException
        {
            get => Exception as T;
            set => Exception = value;
        }

        protected override object GetBoxedPayload()
            => Exception;

        public override bool Equals(object obj)
            => obj is ExceptionHolder token && token.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
