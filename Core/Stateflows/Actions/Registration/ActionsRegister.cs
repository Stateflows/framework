using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Models;
using Stateflows.Actions.Context;
using Stateflows.Actions.Exceptions;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Registration.Builders;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions.Registration
{
    internal class ActionsRegister : IActionsRegister, IOwnedRegistration
    {
        public readonly List<ActionExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories = [];

        public readonly List<ActionInterceptorFactoryAsync> GlobalInterceptorFactories = [];
        
        public readonly List<ActionObserverFactoryAsync> GlobalObserverFactories = [];

        private readonly MethodInfo ActionTypeAddedAsyncMethod =
            typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.ActionTypeAddedAsync));

        public readonly Dictionary<string, ActionModel> Actions = new();

        private readonly Dictionary<string, int> CurrentVersions = new();

        public BehaviorClass? OwnerClass { get; set; }
        public BehaviorClass? ParentClass { get; set; }

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
        public void AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            var key = $"{actionName}.{version}";
            var currentKey = $"{actionName}.current";

            var actionModel = new ActionModel()
            {
                Name = actionName,
                Version = version,
                Delegate = actionDelegate,
                VisitingAction = VisitingActionAsync,
                OwnerClass = OwnerClass,
                ParentClass = ParentClass,
            };
            
            buildAction?.Invoke(new ActionBuilder(actionModel));
            
            if (!Actions.TryAdd(key, actionModel))
            {
                throw new ActionDefinitionException($"Action '{actionName}' with version '{version}' is already registered", new ActionClass(actionName));
            }

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = actionModel;
            }

            return;

            Task VisitingActionAsync(IActionVisitor v)
            {
                // Assign to local variable to avoid value being overriden when invoking lambda function at a later stage
                var ownerClass = OwnerClass;

                return v.ActionAddedAsync(actionName, version, ownerClass);
            }
        }

        [DebuggerHidden]
        public void AddAction(string actionName, int version, Type actionType, ActionBuildAction buildAction = null)
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

            // Assign to local variable to avoid value being overriden when invoking lambda function at a later stage
            var ownerClass = OwnerClass;

            Func<IActionVisitor, Task> visitingAction = async v =>
            {
                await v.ActionAddedAsync(actionName, version, ownerClass);
                await (Task)method.Invoke(v, [actionName, version]);
            };

            var actionModel = new ActionModel()
            {
                Name = actionName,
                Version = version,
                Delegate = actionDelegate,
                VisitingAction = visitingAction,
            };
            
            buildAction?.Invoke(new ActionBuilder(actionModel));

            Actions.Add(key, actionModel);

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = actionModel;
            }
        }

        [DebuggerHidden]
        public void AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction buildAction = null)
            where TAction : class, IAction
            => AddAction(actionName ?? Action<TAction>.Name, version, typeof(TAction), buildAction);

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
        public void AddObserver(ActionObserverFactoryAsync observerFactoryAsync)
            => GlobalObserverFactories.Add(observerFactoryAsync);
        
        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IActionObserver
            => AddObserver(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider));
        
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
