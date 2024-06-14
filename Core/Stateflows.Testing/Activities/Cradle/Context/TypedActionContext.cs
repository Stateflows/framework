using System;
using System.Threading;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Testing.Activities.Cradle.Context
{
    internal class TypedActionContext : ITypedActionContext
    {
        internal NodeContext currentNode;
        public INodeContext CurrentNode => currentNode ??= new NodeContext();

        internal CancellationToken cancellationToken;
        public CancellationToken CancellationToken => cancellationToken;

        internal ActivityContext activity;
        public IActivityContext Activity => activity ??= new ActivityContext();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
        {
            throw new NotImplementedException();
        }
    }
}
