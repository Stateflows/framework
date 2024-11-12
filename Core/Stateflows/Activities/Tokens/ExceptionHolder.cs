using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class ExceptionHolder : TokenHolder
    {
        public Exception Exception { get; set; }
    }

    public class ExceptionHolder<TException> : ExceptionHolder
        where TException : Exception
    {
        public ExceptionHolder()
        {
            Exception = default;
        }

        public override string Name => name ??= typeof(TException).GetReadableName();

        public TException TypedException
        {
            get => Exception as TException;
            set => Exception = value;
        }

        protected override object GetBoxedPayload()
            => Exception;

        protected override Type GetPayloadType()
            => typeof(TException);

        public override bool Equals(object obj)
            => obj is ExceptionHolder token && token.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
