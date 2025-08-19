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

        
        [JsonIgnore]
        public Type PayloadType => GetPayloadType();

        protected abstract Type GetPayloadType();
    }

    public class TokenHolder<TToken> : TokenHolder
    {
        public TokenHolder()
        {
            Payload = default;
        }

        public override string Name => name ??= typeof(TToken).GetReadableName(TypedElements.Tokens);

        public TToken Payload { get; set; }

        protected override object GetBoxedPayload()
            => Payload;

        protected override Type GetPayloadType()
            => typeof(TToken);

        public override bool Equals(object obj)
            => obj is TokenHolder holder && holder.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
