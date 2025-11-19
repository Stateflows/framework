using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Classes;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Inspection.Classes;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Inspector
    {
        private readonly Executor Executor;

        public readonly CommonInterceptor GlobalInterceptor;

        private readonly ILogger Logger;

        public Inspector(Executor executor, ILogger logger)
        {
            Executor = executor;
            Logger = logger;

            GlobalInterceptor = Executor.ServiceProvider.GetRequiredService<CommonInterceptor>();

            ExceptionHandlerFactories.AddRange(Executor.Graph.ExceptionHandlerFactories);
            ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);
            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);

            ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
        }

        public async Task BuildAsync(IStateMachineActionContext context)
        {
            Observers = await Task.WhenAll(ObserverFactories.Select(t => t(Executor.ServiceProvider, context)));
            ReverseObservers = Observers.Reverse().ToArray();
            
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.ServiceProvider, context)));
            ReverseInterceptors = Interceptors.Reverse().ToArray();
            
            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider, context)));

            Plugins = Executor.ServiceProvider.GetService<IEnumerable<IStateMachinePlugin>>().ToArray();
            ReversePlugins = Plugins.Reverse().ToArray();
        }

        public ActionInspection InitializeInspection;

        public ActionInspection FinalizeInspection;

        public IDictionary<string, StateInspection> InspectionStates { get; } = new Dictionary<string, StateInspection>();

        public IDictionary<Edge, TransitionInspection> InspectionTransitions { get; } = new Dictionary<Edge, TransitionInspection>();

        private IStateMachineInspection inspection;
        public IStateMachineInspection Inspection => inspection ??= new StateMachineInspection(Executor, this);

        private readonly List<StateMachineExceptionHandlerFactoryAsync> ExceptionHandlerFactories = new List<StateMachineExceptionHandlerFactoryAsync>();

        private readonly List<StateMachineInterceptorFactoryAsync> InterceptorFactories = new List<StateMachineInterceptorFactoryAsync>();

        private readonly List<StateMachineObserverFactoryAsync> ObserverFactories = new List<StateMachineObserverFactoryAsync>();

        private IStateMachineObserver[] Observers;
        private IStateMachineObserver[] ReverseObservers;
        private IStateMachineInterceptor[] Interceptors;
        private IStateMachineInterceptor[] ReverseInterceptors;
        private IStateMachineExceptionHandler[] ExceptionHandlers;
        private IStateMachinePlugin[] Plugins;
        private IStateMachinePlugin[] ReversePlugins;

        public void BeforeStateMachineInitialize(StateMachineInitializationContext context, bool implicitInitialization)
        {
            if (InitializeInspection != null)
            {
                InitializeInspection.Active = true;
            }

            Plugins.RunSafe(o => o.BeforeStateMachineInitialize(context, implicitInitialization), nameof(BeforeStateMachineInitialize), Logger);
            Observers.RunSafe(o => o.BeforeStateMachineInitialize(context, implicitInitialization), nameof(BeforeStateMachineInitialize), Logger);
        }

        public void AfterStateMachineInitialize(StateMachineInitializationContext context, bool implicitInitialization, bool initialized)
        {
            ReverseObservers.RunSafe(o => o.AfterStateMachineInitialize(context, implicitInitialization, initialized), nameof(AfterStateMachineInitialize), Logger);
            ReversePlugins.RunSafe(o => o.AfterStateMachineInitialize(context, implicitInitialization, initialized), nameof(AfterStateMachineInitialize), Logger);

            if (InitializeInspection != null)
            {
                InitializeInspection.Active = false;
            }
        }

        public void BeforeStateMachineFinalize(StateMachineActionContext context)
        {
            if (FinalizeInspection != null)
            {
                FinalizeInspection.Active = true;
            }

            Plugins.RunSafe(o => o.BeforeStateMachineFinalize(context), nameof(BeforeStateMachineFinalize), Logger);
            Observers.RunSafe(o => o.BeforeStateMachineFinalize(context), nameof(BeforeStateMachineFinalize), Logger);
        }

        public void AfterStateMachineFinalize(StateMachineActionContext context)
        {
            ReverseObservers.RunSafe(o => o.AfterStateMachineFinalize(context), nameof(AfterStateMachineFinalize), Logger);
            ReversePlugins.RunSafe(o => o.AfterStateMachineFinalize(context), nameof(AfterStateMachineFinalize), Logger);

            if (FinalizeInspection != null)
            {
                FinalizeInspection.Active = false;
            }
        }

        public void BeforeStateInitialize(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Initialize);
            }

            Plugins.RunSafe(o => o.BeforeStateInitialize(context), nameof(BeforeStateInitialize), Logger);
            Observers.RunSafe(o => o.BeforeStateInitialize(context), nameof(BeforeStateInitialize), Logger);
        }

        public void AfterStateInitialize(StateActionContext context)
        {
            ReverseObservers.RunSafe(o => o.AfterStateInitialize(context), nameof(AfterStateInitialize), Logger);
            ReversePlugins.RunSafe(o => o.AfterStateInitialize(context), nameof(AfterStateInitialize), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Initialize);
            }
        }

        public void BeforeStateFinalize(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Finalize);
            }

            Plugins.RunSafe(o => o.BeforeStateFinalize(context), nameof(BeforeStateFinalize), Logger);
            Observers.RunSafe(o => o.BeforeStateFinalize(context), nameof(BeforeStateFinalize), Logger);
        }

        public void AfterStateFinalize(StateActionContext context)
        {
            ReverseObservers.RunSafe(o => o.AfterStateFinalize(context), nameof(AfterStateFinalize), Logger);
            ReversePlugins.RunSafe(o => o.AfterStateFinalize(context), nameof(AfterStateFinalize), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Finalize);
            }
        }

        public void BeforeStateEntry(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Entry);
            }

            Plugins.RunSafe(o => o.BeforeStateEntry(context), nameof(BeforeStateEntry), Logger);
            Observers.RunSafe(o => o.BeforeStateEntry(context), nameof(BeforeStateEntry), Logger);
        }

        public void AfterStateEntry(StateActionContext context)
        {
            ReverseObservers.RunSafe(o => o.AfterStateEntry(context), nameof(AfterStateEntry), Logger);
            ReversePlugins.RunSafe(o => o.AfterStateEntry(context), nameof(AfterStateEntry), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Entry);
            }
        }

        public void BeforeStateExit(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Exit);
            }

            Plugins.RunSafe(o => o.BeforeStateExit(context), nameof(BeforeStateExit), Logger);
            Observers.RunSafe(o => o.BeforeStateExit(context), nameof(BeforeStateExit), Logger);
        }

        public void AfterStateExit(StateActionContext context)
        {
            ReverseObservers.RunSafe(o => o.AfterStateExit(context), nameof(AfterStateExit), Logger);
            ReversePlugins.RunSafe(o => o.AfterStateExit(context), nameof(AfterStateExit), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Exit);
            }
        }

        public void BeforeTransitionGuard<TEvent>(GuardContext<TEvent> context)
        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                ((ActionInspection)stateInspection.Guard).Active = true;
            }

            Plugins.RunSafe(o => o.BeforeTransitionGuard(context), nameof(BeforeTransitionGuard), Logger);
            Observers.RunSafe(o => o.BeforeTransitionGuard(context), nameof(BeforeTransitionGuard), Logger);
        }

        public void AfterTransitionGuard<TEvent>(GuardContext<TEvent> context, bool guardResult)
        {
            ReverseObservers.RunSafe(o => o.AfterTransitionGuard(context, guardResult), nameof(AfterTransitionGuard), Logger);
            ReversePlugins.RunSafe(o => o.AfterTransitionGuard(context, guardResult), nameof(AfterTransitionGuard), Logger);

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                ((ActionInspection)stateInspection.Guard).Active = false;
            }
        }

        public void BeforeEffect<TEvent>(TransitionContext<TEvent> context)
        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                ((ActionInspection)stateInspection.Effect).Active = true;
            }

            Plugins.RunSafe(o => o.BeforeTransitionEffect(context), nameof(BeforeEffect), Logger);
            Observers.RunSafe(o => o.BeforeTransitionEffect(context), nameof(BeforeEffect), Logger);
        }

        public void AfterEffect<TEvent>(TransitionContext<TEvent> context)

        {
            ReverseObservers.RunSafe(o => o.AfterTransitionEffect(context), nameof(AfterEffect), Logger);
            ReversePlugins.RunSafe(o => o.AfterTransitionEffect(context), nameof(AfterEffect), Logger);

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                ((ActionInspection)stateInspection.Effect).Active = false;
            }
        }

        public void AfterHydrate(StateMachineActionContext context)
        {
            ReversePlugins.RunSafe(i => i.AfterHydrate(context), nameof(AfterHydrate), Logger);
            GlobalInterceptor.AfterHydrate(new BehaviorActionContext(context.Context.Context, Executor.ServiceProvider));
            ReverseInterceptors.RunSafe(i => i.AfterHydrate(context), nameof(AfterHydrate), Logger);
        }

        public void BeforeDehydrate(StateMachineActionContext context)
        {
            Interceptors.RunSafe(i => i.BeforeDehydrate(context), nameof(BeforeDehydrate), Logger);
            GlobalInterceptor.BeforeDehydrate(new BehaviorActionContext(context.Context.Context, Executor.ServiceProvider));
            Plugins.RunSafe(i => i.BeforeDehydrate(context), nameof(BeforeDehydrate), Logger);
        }

        public bool BeforeProcessEvent<TEvent>(Context.Classes.EventContext<TEvent> context, Common.Context.Classes.EventContext<TEvent> commonContext)
        {
            var plugin = Plugins.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);
            var global = GlobalInterceptor.BeforeProcessEvent(commonContext);
            var local = Interceptors.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);

            return global && local && plugin;
        }

        public void AfterProcessEvent<TEvent>(Context.Classes.EventContext<TEvent> context, Common.Context.Classes.EventContext<TEvent> commonContext, EventStatus eventStatus)
        {
            ReverseInterceptors.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);
            GlobalInterceptor.AfterProcessEvent(commonContext, eventStatus);
            ReversePlugins.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);

            if (context.Context.EventHolder.IsRespondedTo())
            {
                GlobalInterceptor
                    .RequestResponded(
                        context,
                        context.Context.EventHolder,
                        context.Context.EventHolder.GetResponseHolder()
                    );
            }
        }

        private static bool ShouldPropagateException(bool handled)
            => !handled;

        public bool OnStateMachineInitializationException(StateMachineInitializationContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnStateMachineInitializationException(context, exception), nameof(OnStateMachineInitializationException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnStateMachineInitializationException(context, exception), nameof(OnStateMachineInitializationException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnStateMachineFinalizationException(StateMachineActionContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnStateMachineFinalizationException(context, exception), nameof(OnStateMachineFinalizationException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnStateMachineFinalizationException(context, exception), nameof(OnStateMachineFinalizationException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnTransitionGuardException<TEvent>(GuardContext<TEvent> context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnTransitionGuardException(context, exception), nameof(OnTransitionGuardException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnTransitionGuardException(context, exception), nameof(OnTransitionGuardException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnTransitionEffectException<TEvent>(TransitionContext<TEvent> context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnTransitionEffectException(context, exception), nameof(OnTransitionEffectException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(h => h.OnTransitionEffectException(context, exception), nameof(OnTransitionEffectException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnStateInitializeException(StateActionContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnStateInitializationException(context, exception), nameof(OnStateInitializeException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnStateInitializationException(context, exception), nameof(OnStateInitializeException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnStateFinalizeException(StateActionContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnStateFinalizationException(context, exception), nameof(OnStateFinalizeException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnStateFinalizationException(context, exception), nameof(OnStateFinalizeException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnStateEntryException(StateActionContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnStateEntryException(context, exception), nameof(OnStateEntryException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnStateEntryException(context, exception), nameof(OnStateEntryException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public bool OnStateExitException(StateActionContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnStateExitException(context, exception), nameof(OnStateExitException), Logger, false);

            if (!handled)
            {
                Plugins.RunSafe(i => i.OnStateExitException(context, exception), nameof(OnStateExitException), Logger);
            }

            if (ShouldPropagateException(handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }
    }
}
