using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.Utils;

namespace Stateflows.Activities.Models
{
    /// <summary>
    /// Describes which tokens a declaration selects: a CLR type (matched by assignability) or a set of payload names
    /// (matched by the payload token's name). A payload key uses <see cref="Payload"/> as its <see cref="Type"/>, so typed
    /// checks against wildcard declarations such as <see cref="object"/> keep working.
    /// </summary>
    internal sealed class TokenKey : IEquatable<TokenKey>
    {
        private TokenKey(Type type, IReadOnlyList<string> names)
        {
            Type = type;
            Names = names;
        }

        public static TokenKey Of(Type type) => new(type ?? throw new ArgumentNullException(nameof(type)), null);

        public static TokenKey Of<TToken>() => Of(typeof(TToken));

        public static TokenKey OfNames(IReadOnlyList<string> names)
            => new(typeof(Payload), (names ?? throw new ArgumentNullException(nameof(names))).ToArray());

        /// <summary>Creates a typed key, or a payload key when names are given.</summary>
        public static TokenKey Of(Type type, IReadOnlyList<string> names)
            => names == null ? Of(type) : OfNames(names);

        public Type Type { get; }

        /// <summary>Payload names, or <c>null</c> for a typed key.</summary>
        public IReadOnlyList<string> Names { get; }

        public bool IsPayload => Names != null;

        /// <summary>Stable identifier: the token name of a typed key, or the ordered name set of a payload key.</summary>
        public string Identifier => IsPayload ? PayloadNameSet.Key(Names) : Type.GetTokenName();

        public bool Matches(TokenHolder token)
            => IsPayload
                ? token.BoxedPayload is Payload payload && Names.Contains(payload.Name, StringComparer.Ordinal)
                : Type.IsAssignableFrom(token.GetEffectivePayloadType());

        /// <summary>True when every token selected by <paramref name="other"/> is also selected by this key.</summary>
        public bool Contains(TokenKey other)
            => IsPayload
                ? other.IsPayload && !other.Names.Except(Names, StringComparer.Ordinal).Any()
                : Type.IsAssignableFrom(other.Type);

        /// <summary>True when one key contains the other (the typed "assignable either way" rule).</summary>
        public bool IsCompatibleWith(TokenKey other)
            => Contains(other) || other.Contains(this);

        public bool Equals(TokenKey other)
            => other is not null && Type == other.Type &&
               (IsPayload ? other.IsPayload && PayloadNameSet.SetEquals(Names, other.Names) : !other.IsPayload);

        public override bool Equals(object obj) => obj is TokenKey key && Equals(key);

        public override int GetHashCode() => IsPayload ? Identifier.GetHashCode() : Type.GetHashCode();

        public override string ToString() => IsPayload ? $"[{string.Join(", ", Names)}]" : Type.GetTokenName();
    }
}