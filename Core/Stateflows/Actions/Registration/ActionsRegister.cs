using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Context;
using Stateflows.Actions.Context.Classes;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Actions.Models;
using Stateflows.Actions.Exceptions;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions.Registration
{
    internal class ActionsRegister : IActionsRegister
    {
        private IServiceCollection Services { get; }

        public List<ActionExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories { get; set; } = new List<ActionExceptionHandlerFactoryAsync>();
        
        public List<ActionInterceptorFactoryAsync> GlobalInterceptorFactories { get; set; } = new List<ActionInterceptorFactoryAsync>();

        private readonly MethodInfo ActionTypeAddedAsyncMethod =
            typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.ActionTypeAddedAsync));

        private readonly StateflowsBuilder stateflowsBuilder = null;

        public ActionsRegister(StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            this.stateflowsBuilder = stateflowsBuilder;
            Services = services;
        }

        public readonly Dictionary<string, ActionModel> Actions
            = new Dictionary<string, ActionModel>();

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

            Func<IActionVisitor, Task> visitingAction = v => v.ActionAddedAsync(actionName, version);

            var actionModel = new ActionModel()
            {
                Name = actionName,
                Version = version,
                Reentrant = reentrant,
                Delegate = actionDelegate,
                VisitingAction = visitingAction
            };
            
            if (!Actions.TryAdd(key, actionModel))
            {
                throw new ActionDefinitionException($"Action '{actionName}' with version '{version}' is already registered", new ActionClass(actionName));
            }

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = actionModel;
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
                if (((IStateflowsContextProvider)context).Context.ContextOwnerId == null)
                {
                    ActionsContextHolder.ActionContext.Value = (IActionContext)context.Behavior;
                }
                ActionsContextHolder.BehaviorContext.Value = context.Behavior;
                ActionsContextHolder.ExecutionContext.Value = context;
                ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                
                try
                {
                    var instance = (IAction)await StateflowsActivator.CreateModelElementInstanceAsync(
                        ((ActionDelegateContext)context).ServiceProvider,
                        actionType,
                        "action"
                    );
                    
                    await instance.ExecuteAsync(context.CancellationToken);
                }
                finally
                {
                    ActionsContextHolder.ExecutionContext.Value = null;
                    ContextValues.GlobalValuesHolder.Value = null;
                }
            };

            var method = ActionTypeAddedAsyncMethod.MakeGenericMethod(actionType);
            Func<IActionVisitor, Task> visitingAction = async v =>
            {
                await v.ActionAddedAsync(actionName, version);
                await (Task)method.Invoke(v, [actionName, version]);
            };
            
            var actionModel = new ActionModel()
            {
                Name = actionName,
                Version = version,
                Reentrant = reentrant,
                Delegate = actionDelegate,
                VisitingAction = visitingAction
            };

            Actions.Add(key, actionModel);

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = actionModel;
            }
        }

        [DebuggerHidden]
        public void AddAction<TAction>(string actionName = null, int version = 1, bool reentrant = true)
            where TAction : class, IAction
            => AddAction(actionName ?? Action<TAction>.Name, version, typeof(TAction), reentrant);

        public Task VisitActionsAsync(IActionVisitor visitor)
        {
            foreach (var action in Actions.Where(kv => kv.Key.EndsWith(".current")).Select(kv => kv.Value))
            {
                action.VisitingAction(visitor);
            }
            
            return Task.CompletedTask;
        }

        #region Observability
        [DebuggerHidden]
        public void AddInterceptor(ActionInterceptorFactoryAsync interceptorFactoryAsync)
            => GlobalInterceptorFactories.Add(interceptorFactoryAsync);
        
        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActionInterceptor
            => AddInterceptor(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider));
        
        [DebuggerHidden]
        public void AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);
        
        [DebuggerHidden]
        public void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActionExceptionHandler
            => AddExceptionHandler(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TExceptionHandler>(serviceProvider));
        #endregion
    }
}
