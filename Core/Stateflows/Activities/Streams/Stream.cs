using System.Collections.Generic;

namespace Stateflows.Activities.Streams
{
    public class Stream
    {
        public string EdgeIdentifier { get; set; }

        public bool IsActivated { get; set; } = false;

        public bool IsPersistent { get; set; } = false;

        public Queue<Token> Tokens { get; set; } = new Queue<Token>();

        public void Consume(IEnumerable<Token> tokens, bool isPersistent)
        {
            IsPersistent = isPersistent;
            foreach (var token in tokens)
            {
                Consume(token);
            }
        }

        public void Consume<TToken>(TToken token)
            where TToken : Token, new()
        {
            if (token is ControlToken)
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
