﻿using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    public static class StateMachineActivityExtensions
    {
        [DebuggerHidden]
        internal static void RunStateActivity(string actionName, IStateActionContext context, string activityName, StateActionActivityBuildAction buildAction)
        {
            if (context.TryLocateActivity(activityName, $"{context.StateMachine.Id.Instance}.{context.CurrentState.Name}.{actionName}.{context.ExecutionTrigger.Id}", out var a))
            {
                Task.Run(async () =>
                {
                    var integratedActivityBuilder = new StateActionActivityBuilder(buildAction);
                    Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize();
                    return a.SendCompoundAsync(
                        integratedActivityBuilder.GetSubscriptionRequest(context.StateMachine.Id),
                        new SetGlobalValues() { Values = (context.StateMachine.Values as ContextValuesCollection).Values },
                        new ExecutionRequest() { InitializationEvent = initializationEvent },
                        integratedActivityBuilder.GetUnsubscriptionRequest(context.StateMachine.Id)
                    );
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"On{actionName}Activity '{activityName}' not found", context.StateMachine.Id.StateMachineClass);
            }
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardActivity<TEvent>(ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
            where TEvent : Event, new()
        {
            var result = false;
            if (context.TryLocateActivity(activityName, $"{context.StateMachine.Id.Instance}.{context.SourceState.Name}.{Constants.Guard}.{context.Event.Id}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                await Task.Run(async () =>
                {
                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                    Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize();
                    var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
                    executionRequest.AddInputToken(ev);

                    var sendResult = await a.SendCompoundAsync(
                        integratedActivityBuilder.GetSubscriptionRequest(context.StateMachine.Id),
                        new SetGlobalValues() { Values = (context.StateMachine.Values as ContextValuesCollection).Values },
                        executionRequest,
                        integratedActivityBuilder.GetUnsubscriptionRequest(context.StateMachine.Id)
                    );

                    result = (sendResult.Response.Results.Skip(2).Take(1).First().Response as ExecutionResponse).OutputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"GuardActivity '{activityName}' not found", context.StateMachine.Id.StateMachineClass);
            }

            return result;
        }

        [DebuggerHidden]
        internal static Task RunEffectActivity<TEvent>(ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
            where TEvent : Event, new()
        {
            if (context.TryLocateActivity(activityName, $"{context.StateMachine.Id.Instance}.{context.SourceState.Name}.{EventInfo<TEvent>.Name}.{Constants.Effect}.{context.Event.Id}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                Task.Run(async () =>
                {
                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                    Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize();
                    var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
                    executionRequest.AddInputToken(ev);

                    return a.SendCompoundAsync(
                        integratedActivityBuilder.GetSubscriptionRequest(context.StateMachine.Id),
                        new SetGlobalValues() { Values = (context.StateMachine.Values as ContextValuesCollection).Values },
                        executionRequest,
                        integratedActivityBuilder.GetUnsubscriptionRequest(context.StateMachine.Id)
                    );
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"EffectActivity '{activityName}' not found", context.StateMachine.Id.StateMachineClass);
            }

            return Task.CompletedTask;
        }
    }
}