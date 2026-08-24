using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Actions.Context;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Exceptions;
using Stateflows.Actions.Models;
using Stateflows.Actions.Registration.Builders;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Initializer;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions.Registration
{
    internal class ActionsRegister : IActionsRegister, IOwnedRegistration
    {
        public readonly List<ActionExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories = [];

        public readonly List<ActionInterceptorFactoryAsync> GlobalInterceptorFactories = [];

        public readonly List<ActionObserverFactoryAsync> GlobalObserverFactories = [];

        private readonly MethodInfo ActionTypeAddedAsyncMethod = typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.ActionTypeAddedAsync));

        private readonly MethodInfo CustomEventAddedAsyncMethod = typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.CustomEventAddedAsync));

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

            ActionModel actionModel = null;

            Func<IActionVisitor, Task> visitingAction = async v =>
            {
                var hasDefaultInstance = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
                    .Any(t => t.BehaviorClass.Type == ActionClass.Type && t.BehaviorClass.Name == actionName);

                await v.ActionAddingAsync(actionName, version, actionModel.BehaviorClassType, actionModel?.OwnerClass, actionModel?.ParentClass, hasDefaultInstance);
                await v.ActionAddedAsync(actionName, version);
                var eventTypes = new List<Type>();
                eventTypes.AddRange(actionModel.ConsumedEventTypes);
                eventTypes.AddRange(actionModel.ConsumedTokenTypes);
                var methods = eventTypes.Select(t => CustomEventAddedAsyncMethod.MakeGenericMethod(t)).ToArray();
                foreach (var method in methods)
                {
                    await (Task)method.Invoke(v, [actionName, version, new[] { BehaviorStatus.Initialized }]);
                }
            };

            actionModel = new ActionModel()
            {
                Name = actionName,
                Version = version,
                Delegate = actionDelegate,
                VisitingAction = visitingAction,
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

            var method = ActionTypeAddedAsyncMethod.MakeGenericMethod(actionType);

            // Assign to local variable to avoid value being overriden when invoking lambda function at a later stage
            var ownerClass = OwnerClass;
            var parentClass = ParentClass;

            // Func<IActionVisitor, Task> visitingAction = async v =>
            // {
            //     var hasDefaultInstance = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
            //         .Any(t => t.BehaviorClass.Type == ActionClass.Type && t.BehaviorClass.Name == actionName);
            //
            //     await v.ActionAddingAsync(actionName, version, ownerClass, parentClass, hasDefaultInstance);
            //     await v.ActionAddedAsync(actionName, version);
            //     await (Task)method.Invoke(v, [actionName, version]);
            // };
            
            ActionModel actionModel = null;
            
            Func<IActionVisitor, Task> visitingAction = async v =>
            {
                var hasDefaultInstance = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
                    .Any(t => t.BehaviorClass.Type == actionModel.BehaviorClassType && t.BehaviorClass.Name == actionName);

                await v.ActionAddingAsync(actionName, version, actionModel.BehaviorClassType, actionModel?.OwnerClass, actionModel?.ParentClass, hasDefaultInstance);
                await v.ActionAddedAsync(actionName, version);
                await (Task)ActionTypeAddedAsyncMethod.Invoke(v, [actionName, version]);
                var eventTypes = new List<Type>();
                eventTypes.AddRange(actionModel.ConsumedEventTypes);
                eventTypes.AddRange(actionModel.ConsumedTokenTypes);
                var methods = eventTypes.Select(t => CustomEventAddedAsyncMethod.MakeGenericMethod(t)).ToArray();
                foreach (var method in methods)
                {
                    await (Task)method.Invoke(v, [actionName, version, new[] { BehaviorStatus.Initialized }]);
                }
            };

            actionModel = new ActionModel()
            {
                Name = actionName,
                Version = version,
                // Delegate = actionDelegate,
                VisitingAction = visitingAction,
            };

            buildAction?.Invoke(new ActionBuilder(actionModel));

            actionModel.Delegate = async context =>
            {
                ActionsContextHolder.ActionContext.Value = (IActionContext)context.Behavior;
                ActionsContextHolder.BehaviorContext.Value = context.Behavior;
                ActionsContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                    ? parentBehaviorContext
                    : null;
                ActionsContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                    ? ownerBehaviorContext
                    : null;
                ActionsContextHolder.ExecutionContext.Value = context;
                ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;

                try
                {
                    var instance = (IAction)await StateflowsActivator.CreateModelElementInstanceAsync(
                        ((ActionDelegateContext)context).ServiceProvider,
                        actionType,
                        "action"
                    );

                    actionModel.ConfigurationAction?.Invoke(instance);

                    await instance.ExecuteAsync(context.CancellationToken);
                }
                finally
                {
                    ActionsContextHolder.ExecutionContext.Value = null;
                    ContextValues.GlobalValuesHolder.Value = null;
                }
            };

            Actions.Add(key, actionModel);

            if (IsNewestVersion(actionName, version))
            {
                Actions[currentKey] = actionModel;
            }
        }

        [DebuggerHidden]
        public void AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
            => AddAction(actionName ?? Action<TAction>.Name, version, typeof(TAction), b => buildAction?.Invoke(new ActionBuilder<TAction>(((ActionBuilder)b).Model)));

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
