using System;
using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    /// <summary>Represents an exception stored as an activity token.</summary>
    /// <remarks>Activity exception-handler nodes match the exception payload type to determine which exception tokens they can handle.</remarks>
    public abstract class ExceptionHolder : TokenHolder
    {
        /// <summary>Gets or sets the exception payload.</summary>
        public Exception Exception { get; set; }
    }

    /// <summary>Represents an exception token with a statically specified exception type.</summary>
    /// <typeparam name="TException">The exception payload type.</typeparam>
    /// <remarks>The generic type determines the token's declared payload type, while <see cref="Exception"/> contains the actual exception instance.</remarks>
    public class ExceptionHolder<TException> : ExceptionHolder
        where TException : Exception
    {
        /// <summary>Initializes an exception token with the default value of its exception payload.</summary>
        public ExceptionHolder()
        {
            Exception = default;
        }

        /// <inheritdoc />
        public override string Name => name ??= typeof(TException).GetReadableName(TypedElements.Exceptions);

        /// <summary>Gets or sets the payload as <typeparamref name="TException"/>.</summary>
        public TException TypedException
        {
            get => Exception as TException;
            set => Exception = value;
        }

        /// <inheritdoc />
        protected override object GetBoxedPayload()
            => Exception;

        /// <inheritdoc />
        protected override Type GetPayloadType()
            => typeof(TException);

        /// <inheritdoc />
        protected override Type GetActualPayloadType()
            => Exception.GetType();

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is ExceptionHolder token && token.Id == Id;

        /// <inheritdoc />
        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
