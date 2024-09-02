using System.Collections.Generic;

namespace Stateflows.Activities.Streams
{
    public class Stream
    {
        public string EdgeIdentifier { get; set; }

        public bool IsActivated { get; set; } = false;

        public bool IsPersistent { get; set; } = false;

        public Queue<TokenHolder> Tokens { get; set; } = new Queue<TokenHolder>();

        public void Consume(IEnumerable<TokenHolder> tokens, bool isPersistent)
        {
            IsPersistent = isPersistent;
            foreach (var token in tokens)
            {
                Consume(token);
            }
        }

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
