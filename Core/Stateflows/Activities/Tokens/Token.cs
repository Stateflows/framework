using System;
using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class Token
    {
        protected string name;
        public virtual string Name => name ??= GetType().GetTokenName();

        [JsonIgnore]
        public object BoxedPayload { get; }

        protected abstract object GetBoxedPayload();
    }

    public class Token<T> : Token
    {
        public Token()
        {
            Payload = default;
        }

        public override string Name => name ??= typeof(T).GetReadableName();

        public T Payload { get; set; }

        protected override object GetBoxedPayload()
            => Payload;

        public override bool Equals(object obj)
            => obj is Token<T> token && token.Payload.Equals(Payload);

        public override int GetHashCode()
            => Payload.GetHashCode();
    }
}
