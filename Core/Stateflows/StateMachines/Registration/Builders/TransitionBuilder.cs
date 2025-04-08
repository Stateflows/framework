using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class TransitionBuilder<TEvent> :
        ITransitionBuilder<TEvent>,
        IElseTransitionBuilder<TEvent>,
        IInternalTransitionBuilder<TEvent>,
        IElseInternalTransitionBuilder<TEvent>,
        IDefaultTransitionBuilder,
        IDefaultTransitionEffectBuilder,
        IElseDefaultTransitionBuilder,
        IOverridenTransitionBuilder<TEvent>,
        IOverridenElseTransitionBuilder<TEvent>,
        IOverridenInternalTransitionBuilder<TEvent>,
        IOverridenElseInternalTransitionBuilder<TEvent>,
        IOverridenDefaultTransitionBuilder,
        IOverridenDefaultTransitionEffectBuilder,
        IOverridenElseDefaultTransitionBuilder,
        IBehaviorBuilder,
        IForwardedEventBuilder<TEvent>,
        IInternal,
        IEdgeBuilder
    {
        public Edge Edge { get; private set; }

        private readonly IEnumerable<VertexType> transitiveVertexTypes = new HashSet<VertexType>() {
            VertexType.Junction,
            VertexType.Choice,
        };

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Edge.Source.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Edge.Source.Graph.Version;

        public IServiceCollection Services { get; }

        public TransitionBuilder(Edge edge, IServiceCollection services)
        {
            Edge = edge;
            Services = services;
        }

        public ITransitionBuilder<TEvent> AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
        {
            foreach (var guardAsync in guardsAsync)
            {
                guardAsync.ThrowIfNull(nameof(guardAsync));

                if (Edge.Source.Type == VertexType.Fork)
                {
                    throw new TransitionDefinitionException(
                        $"Transition outgoing from fork '{Edge.SourceName}' cannot have guards",
                        Edge.Graph.Class
                    );
                }

                var guardHandler = guardAsync.AddStateMachineInvocationContext(Edge.Graph);

                Edge.Guards.Actions.Add(async c =>
                    {
                        if (transitiveVertexTypes.Contains(Edge.Source.Type))
                        {
                            c.SetEvent(new Completion().ToEventHolder());
                        }

                        var context = new GuardContext<TEvent>(c, Edge);
                        var result = false;
                        try
                        {
                            result = await guardHandler(context);
                        }
                        catch (Exception e)
                        {
                            if (e is StateflowsDefinitionException)
                            {
                                throw;
                            }
                            else
                            {
                                Trace.WriteLine($"⦗→s⦘ Activity '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception thrown '{e.Message}'");
                                if (!c.Executor.Inspector.OnTransitionGuardException(context, e))
                                {
                                    throw;
                                }
                                else
                                {
                                    throw new BehaviorExecutionException(e);
                                }
                            }
                        }
                        finally
                        {
                            if (transitiveVertexTypes.Contains(Edge.Source.Type))
                            {
                                c.ClearEvent();
                            }
                        }

                        return result;
                    }
                );
            }

            return this;
        }

        public ITransitionBuilder<TEvent> AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
        {
            foreach (var effectAsync in effectsAsync)
            {
                effectAsync.ThrowIfNull(nameof(effectAsync));

                var effectHandler = effectAsync.AddStateMachineInvocationContext(Edge.Graph);

                Edge.Effects.Actions.Add(async c =>
                    {
                        if (transitiveVertexTypes.Contains(Edge.Source.Type))
                        {
                            c.SetEvent(new Completion().ToEventHolder());
                        }

                        var context = new TransitionContext<TEvent>(c, Edge);
                        try
                        {
                            await effectHandler(context);
                        }
                        catch (Exception e)
                        {
                            if (e is StateflowsDefinitionException)
                            {
                                throw;
                            }
                            else
                            {
                                Trace.WriteLine($"⦗→s⦘ Activity '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception thrown '{e.Message}'");
                                if (!c.Executor.Inspector.OnTransitionEffectException(context, e))
                                {
                                    throw;
                                }
                                else
                                {
                                    throw new BehaviorExecutionException(e);
                                }
                            }
                        }
                        finally
                        {
                            if (transitiveVertexTypes.Contains(Edge.Source.Type))
                            {
                                c.ClearEvent();
                            }
                        }
                    }
                );
            }

            return this;
        }

        IInternalTransitionBuilder<TEvent> IEffect<TEvent, IInternalTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IInternalTransitionBuilder<TEvent>;

        IInternalTransitionBuilder<TEvent> IBaseGuard<TEvent, IInternalTransitionBuilder<TEvent>>.AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync) as IInternalTransitionBuilder<TEvent>;

        IElseTransitionBuilder<TEvent> IEffect<TEvent, IElseTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IElseTransitionBuilder<TEvent>;

        IElseInternalTransitionBuilder<TEvent> IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IElseInternalTransitionBuilder<TEvent>;

        IDefaultTransitionBuilder IDefaultEffect<IDefaultTransitionBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IDefaultTransitionBuilder;

        IDefaultTransitionEffectBuilder IDefaultEffect<IDefaultTransitionEffectBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IDefaultTransitionEffectBuilder;

        IElseDefaultTransitionBuilder IDefaultEffect<IElseDefaultTransitionBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IElseDefaultTransitionBuilder;

        IDefaultTransitionBuilder IBaseDefaultGuard<IDefaultTransitionBuilder>.AddGuard(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => (this as TransitionBuilder<Completion>)!.AddGuard(guardsAsync) as IDefaultTransitionBuilder;

        IForwardedEventBuilder<TEvent> IBaseGuard<TEvent, IForwardedEventBuilder<TEvent>>.AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync) as IForwardedEventBuilder<TEvent>;

        public ITransitionBuilder<TEvent> SetPolymorphicTriggers(bool polymorphicTriggers)
        {
            Edge.PolymorphicTriggers = polymorphicTriggers;
            return this;
        }

        IElseTransitionBuilder<TEvent> ITransitionUtils<IElseTransitionBuilder<TEvent>>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IElseTransitionBuilder<TEvent>;

        IElseInternalTransitionBuilder<TEvent> ITransitionUtils<IElseInternalTransitionBuilder<TEvent>>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IElseInternalTransitionBuilder<TEvent>;

        IDefaultTransitionBuilder ITransitionUtils<IDefaultTransitionBuilder>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IDefaultTransitionBuilder;

        IDefaultTransitionEffectBuilder ITransitionUtils<IDefaultTransitionEffectBuilder>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IDefaultTransitionEffectBuilder;

        IElseDefaultTransitionBuilder ITransitionUtils<IElseDefaultTransitionBuilder>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IElseDefaultTransitionBuilder;

        IOverridenTransitionBuilder<TEvent> ITransitionUtils<IOverridenTransitionBuilder<TEvent>>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IOverridenTransitionBuilder<TEvent>;

        IOverridenElseTransitionBuilder<TEvent> ITransitionUtils<IOverridenElseTransitionBuilder<TEvent>>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IOverridenElseTransitionBuilder<TEvent>;

        IOverridenElseInternalTransitionBuilder<TEvent> ITransitionUtils<IOverridenElseInternalTransitionBuilder<TEvent>>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IOverridenElseInternalTransitionBuilder<TEvent>;

        IOverridenDefaultTransitionBuilder ITransitionUtils<IOverridenDefaultTransitionBuilder>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IOverridenDefaultTransitionBuilder;

        IOverridenDefaultTransitionEffectBuilder ITransitionUtils<IOverridenDefaultTransitionEffectBuilder>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IOverridenDefaultTransitionEffectBuilder;

        IOverridenElseDefaultTransitionBuilder ITransitionUtils<IOverridenElseDefaultTransitionBuilder>.SetPolymorphicTriggers(bool polymorphicTriggers)
            => SetPolymorphicTriggers(polymorphicTriggers) as IOverridenElseDefaultTransitionBuilder;

        public ITransitionBuilder<TEvent> SetIsLocal(bool isLocal)
        {
            Edge.IsLocal = isLocal;

            return this;
        }

        IElseTransitionBuilder<TEvent> ITransitionUtils<IElseTransitionBuilder<TEvent>>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IElseTransitionBuilder<TEvent>;

        IElseInternalTransitionBuilder<TEvent> ITransitionUtils<IElseInternalTransitionBuilder<TEvent>>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IElseInternalTransitionBuilder<TEvent>;

        IDefaultTransitionBuilder ITransitionUtils<IDefaultTransitionBuilder>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IDefaultTransitionBuilder;

        IDefaultTransitionEffectBuilder ITransitionUtils<IDefaultTransitionEffectBuilder>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IDefaultTransitionEffectBuilder;

        IElseDefaultTransitionBuilder ITransitionUtils<IElseDefaultTransitionBuilder>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IElseDefaultTransitionBuilder;

        IOverridenTransitionBuilder<TEvent> ITransitionUtils<IOverridenTransitionBuilder<TEvent>>.SetIsLocal(
            bool isLocal)
            => SetIsLocal(isLocal) as IOverridenTransitionBuilder<TEvent>;

        IOverridenTransitionBuilder<TEvent> IEffect<TEvent, IOverridenTransitionBuilder<TEvent>>.AddEffect(
            params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IOverridenTransitionBuilder<TEvent>;

        IOverridenTransitionBuilder<TEvent> IBaseGuard<TEvent, IOverridenTransitionBuilder<TEvent>>.AddGuard(
            params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync) as IOverridenTransitionBuilder<TEvent>;

        IOverridenElseTransitionBuilder<TEvent> ITransitionUtils<IOverridenElseTransitionBuilder<TEvent>>.SetIsLocal(
            bool isLocal)
            => SetIsLocal(isLocal) as IOverridenElseTransitionBuilder<TEvent>;

        IOverridenElseTransitionBuilder<TEvent> IEffect<TEvent, IOverridenElseTransitionBuilder<TEvent>>.AddEffect(
            params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IOverridenElseTransitionBuilder<TEvent>;

        IOverridenInternalTransitionBuilder<TEvent> IEffect<TEvent, IOverridenInternalTransitionBuilder<TEvent>>.
            AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IOverridenInternalTransitionBuilder<TEvent>;

        IOverridenInternalTransitionBuilder<TEvent> IBaseGuard<TEvent, IOverridenInternalTransitionBuilder<TEvent>>.
            AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync) as IOverridenInternalTransitionBuilder<TEvent>;

        IOverridenElseInternalTransitionBuilder<TEvent>
            ITransitionUtils<IOverridenElseInternalTransitionBuilder<TEvent>>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IOverridenElseInternalTransitionBuilder<TEvent>;

        IOverridenElseInternalTransitionBuilder<TEvent> IEffect<TEvent, IOverridenElseInternalTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IOverridenElseInternalTransitionBuilder<TEvent>;

        IOverridenDefaultTransitionBuilder ITransitionUtils<IOverridenDefaultTransitionBuilder>.SetIsLocal(bool isLocal)
            => SetIsLocal(isLocal) as IOverridenDefaultTransitionBuilder;

        IOverridenDefaultTransitionBuilder IDefaultEffect<IOverridenDefaultTransitionBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IOverridenDefaultTransitionBuilder;

        IOverridenDefaultTransitionBuilder IBaseDefaultGuard<IOverridenDefaultTransitionBuilder>.AddGuard(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => (this as TransitionBuilder<Completion>)!.AddGuard(guardsAsync) as IOverridenDefaultTransitionBuilder;

        IOverridenDefaultTransitionEffectBuilder ITransitionUtils<IOverridenDefaultTransitionEffectBuilder>.SetIsLocal(
            bool isLocal)
            => SetIsLocal(isLocal) as IOverridenDefaultTransitionEffectBuilder;

        IOverridenDefaultTransitionEffectBuilder IDefaultEffect<IOverridenDefaultTransitionEffectBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IOverridenDefaultTransitionEffectBuilder;

        IOverridenElseDefaultTransitionBuilder ITransitionUtils<IOverridenElseDefaultTransitionBuilder>.SetIsLocal(
            bool isLocal)
            => SetIsLocal(isLocal) as IOverridenElseDefaultTransitionBuilder;

        IOverridenElseDefaultTransitionBuilder IDefaultEffect<IOverridenElseDefaultTransitionBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IOverridenElseDefaultTransitionBuilder;
    }
}
