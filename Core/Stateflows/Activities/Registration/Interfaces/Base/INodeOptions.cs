using System;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface INodeOptions<out TReturn>
    {
        TReturn SetOptions(NodeOptions nodeOptions);
        TReturn UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater);
        TReturn NoImplicitJoin()
            => UpdateOptions(options => options & ~NodeOptions.ImplicitJoin);
        TReturn NoImplicitFork()
            => UpdateOptions(options => options & ~NodeOptions.ImplicitFork);
    }
}
