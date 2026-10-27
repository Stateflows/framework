using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Streams
{
    /// <summary>Stores activation and queued tokens for one activity flow edge.</summary>
    /// <remarks>The activity runtime uses stream state while advancing flows; persistent streams retain tokens across node executions.</remarks>
    public class Stream
    {
        /// <summary>Gets or sets the identifier of the flow edge represented by this stream.</summary>
        public string EdgeIdentifier { get; set; }

        /// <summary>Gets or sets whether a control token has activated this stream.</summary>
        public bool IsActivated { get; set; } = false;

        /// <summary>Gets or sets whether object tokens are retained on this stream for later executions.</summary>
        public bool IsPersistent { get; set; } = false;

        /// <summary>Gets or sets the queued object tokens currently held by this stream.</summary>
        public Queue<TokenHolder> Tokens { get; set; } = new Queue<TokenHolder>();

        /// <summary>Consumes a sequence of tokens into the stream and sets its persistence behavior.</summary>
        /// <param name="tokens">Tokens to consume.</param>
        /// <param name="isPersistent"><see langword="true"/> when the stream should retain object tokens.</param>
        public void Consume(IEnumerable<TokenHolder> tokens, bool isPersistent)
        {
            IsPersistent = isPersistent;
            foreach (var token in tokens)
            {
                Consume(token);
            }
        }

        /// <summary>Consumes one token, activating the stream for control tokens or queueing an object token.</summary>
        /// <param name="token">The token to consume.</param>
        public void Consume(TokenHolder token)
        {
            if (token is TokenHolder<ControlToken>)
            {
                IsActivated = true;
            }
            else
            {
                Tokens.Enqueue(token);
            }
        }
    }
}
