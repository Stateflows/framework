using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Context.Classes
{
    public class BaseContext : IBehaviorLocator, IDisposable
    {
        public static Dictionary<BehaviorId, List<BaseContext>> Instances = [];

        public BaseContext(StateflowsContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            ServiceProvider = serviceProvider;
            
            lock (Instances)
            {
                if (!Instances.TryGetValue(Context.Id, out var contextList))
                {
                    contextList = [];
                    Instances.Add(Context.Id, contextList);
                }
                
                contextList.Add(this);
            }
        }

        public StateflowsContext Context { get; }

        public IServiceProvider ServiceProvider { get; }

        private BehaviorContext? behavior;
        internal BehaviorContext Behavior => behavior ??= new BehaviorContext(Context, ServiceProvider);

        // private BehaviorContext? parentBehavior;
        // internal BehaviorContext ParentBehavior => parentBehavior ??= new BehaviorContext(Context, ServiceProvider);
        //
        // private BehaviorContext? ownerBehavior;
        // internal BehaviorContext OwnerBehavior => ownerBehavior ??= new BehaviorContext(Context, ServiceProvider);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator => behaviorLocator ??= ServiceProvider.GetService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);
        
        public CancellationTokenSource CancellationTokenSource = new();
        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public void Dispose()
        {
            lock (Instances)
            {
                if (!Instances.TryGetValue(Context.Id, out var contextList))
                {
                    contextList = [];
                    Instances.Add(Context.Id, contextList);
                }
                
                contextList.Remove(this);
            }
            
            behavior?.Dispose();
            CancellationTokenSource?.Dispose();
        }
    }
}
