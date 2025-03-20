using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Context;
using Stateflows.Actions.Context.Classes;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Actions.Exceptions;

namespace Stateflows.Actions.Registration
{
    internal class ActionsRegister : IActionsRegister
    {
        private IServiceCollection Services { get; }

        // public List<ActionExceptionHandlerFactory> GlobalExceptionHandlerFactories { get; set; } = new List<ActionExceptionHandlerFactory>();
        //
        // public List<ActionInterceptorFactory> GlobalInterceptorFactories { get; set; } = new List<ActionInterceptorFactory>();
        //
        // public List<ActionObserverFactory> GlobalObserverFactories { get; set; } = new List<ActionObserverFactory>();

        private readonly StateflowsBuilder stateflowsBuilder = null;

        public ActionsRegister(StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            this.stateflowsBuilder = stateflowsBuilder;
            Services = services;
        }

        public readonly Dictionary<string, (string Name, int Version, bool Reentrant, ActionDelegateAsync Delegate)> Actions 
            = new Dictionary<string, (string Name, int Version, bool Reentrant, ActionDelegateAsync Delegate)>();

        public readonly Dictionary<string, int> CurrentVersions = new Dictionary<string, int>();

        private bool IsNewestVersion(string actionName, int version)
        {
            var result = false;

            if (CurrentVersions.TryGetValue(actionName, out var currentVersion))
            {
                if (currentVersion < version)
                {
                    result = true;
                    CurrentVersions[actionName] = version;
                }
            }
            else
            {
                result = true;
                CurrentVersions[actionName] = version;
            }

            return result;
        }

        [DebuggerHidden]
        public void AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, bool reentrant = true)
        {
            var key = $"{actionName}.{version}";
            var currentKey = $"{actionName}.current";

            if (!Actions.TryAdd(key, (actionName, version, reentrant, actionDelegate)))
            {
                throw new ActionDefinitionException($"Action '{actionName}' with version '{version}' is already registered", new ActionClass(actionName));
            }

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = (actionName, version, reentrant, actionDelegate);
            }
        }

        [DebuggerHidden]
        public void AddAction(string actionName, int version, Type actionType, bool reentrant = true)
        {
            var key = $"{actionName}.{version}";
            var currentKey = $"{actionName}.current";

            if (Actions.ContainsKey(key))
            {
                throw new ActionDefinitionException($"Action '{actionName}' with version '{version}' is already registered", new ActionClass(actionName));
            }

            ActionDelegateAsync actionDelegate = async context =>
            {
                ActionsContextHolder.ActionContext.Value = context.Action;
                ActionsContextHolder.ExecutionContext.Value = context;
                ContextValues.GlobalValuesHolder.Value = context.Action.Values;
                
                try
                {
                    var instance = (IAction)await StateflowsActivator.CreateInstanceAsync(
                        ((ActionDelegateContext)context).ServiceProvider,
                        actionType,
                        "action"
                    );
                    
                    await instance.ExecuteAsync(CancellationToken.None);
                }
                finally
                {
                    ActionsContextHolder.ActionContext.Value = null;
                    ActionsContextHolder.ExecutionContext.Value = null;
                    ContextValues.GlobalValuesHolder.Value = null;
                }
            };

            Actions.Add(key, (actionName, version, reentrant, actionDelegate));

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = (actionName, version, reentrant, actionDelegate);
            }
        }

        [DebuggerHidden]
        public void AddAction<TAction>(string actionName = null, int version = 1, bool reentrant = true)
            where TAction : class, IAction
            => AddAction(actionName ?? Action<TAction>.Name, version, typeof(TAction), reentrant);

        // #region Observability
        // [DebuggerHidden]
        // public void AddInterceptor(ActionInterceptorFactory interceptorFactory)
        //     => GlobalInterceptorFactories.Add(interceptorFactory);
        //
        // [DebuggerHidden]
        // public void AddInterceptor<TInterceptor>()
        //     where TInterceptor : class, IActionInterceptor
        //     => AddInterceptor(serviceProvider => StateflowsActivator.CreateInstance<TInterceptor>(serviceProvider));
        //
        // [DebuggerHidden]
        // public void AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory)
        //     => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);
        //
        // [DebuggerHidden]
        // public void AddExceptionHandler<TExceptionHandler>()
        //     where TExceptionHandler : class, IActionExceptionHandler
        //     => AddExceptionHandler(serviceProvider => StateflowsActivator.CreateInstance<TExceptionHandler>(serviceProvider));
        //
        // [DebuggerHidden]
        // public void AddObserver(ActionObserverFactory observerFactory)
        //     => GlobalObserverFactories.Add(observerFactory);
        //
        // [DebuggerHidden]
        // public void AddObserver<TObserver>()
        //     where TObserver : class, IActionObserver
        //     => AddObserver(serviceProvider => StateflowsActivator.CreateInstance<TObserver>(serviceProvider));
        // #endregion
    }
}
