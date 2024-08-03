using System;
using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class TokenHolder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        protected string name;
        public virtual string Name => name ??= GetType().GetTokenName();

        [JsonIgnore]
        public object BoxedPayload { get; }

        protected abstract object GetBoxedPayload();
    }

    public class TokenHolder<T> : TokenHolder
    {
        public TokenHolder()
        {
            Payload = default;
        }

        public override string Name => name ??= typeof(T).GetReadableName();

        public T Payload { get; set; }

        protected override object GetBoxedPayload()
            => Payload;

        public override bool Equals(object obj)
            => obj is TokenHolder token && token.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
