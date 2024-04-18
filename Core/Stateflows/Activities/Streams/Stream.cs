using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Streams
{
    public class Stream
    {
        public string EdgeIdentifier { get; set; }

        public bool IsActivated { get; set; } = false;

        public bool IsPersistent { get; set; } = false;

        public Queue<object> Tokens { get; set; } = new Queue<object>();

        public void Consume(IEnumerable<object> tokens, bool isPersistent)
        {
            IsPersistent = isPersistent;
            foreach (var token in tokens)
            {
                Consume(token);
            }
        }

        public void Consume<TToken>(TToken token)
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
