using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Context.Classes
{
    public class BaseContext : IBehaviorLocator, IDisposable
    {
        public static Dictionary<BehaviorId, List<BaseContext>> Instances = [];

        private readonly bool ownsCancellationTokenSource;
        private int disposed;

        public BaseContext(StateflowsContext context, IServiceProvider serviceProvider,
            CancellationTokenSource cancellationTokenSource = null)
        {
            Context = context;
            ServiceProvider = serviceProvider;
            ownsCancellationTokenSource = cancellationTokenSource == null;
            CancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
            
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
        
        public CancellationTokenSource CancellationTokenSource;
        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            lock (Instances)
            {
                if (Instances.TryGetValue(Context.Id, out var contextList))
                {
                    contextList.Remove(this);
                    if (contextList.Count == 0)
                    {
                        Instances.Remove(Context.Id);
                    }
                }
            }
            
            behavior?.Dispose();
            if (ownsCancellationTokenSource)
            {
                CancellationTokenSource?.Dispose();
            }
        }
    }
}
