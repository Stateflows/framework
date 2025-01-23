using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
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
            if (context.TryLocateActivity(activityName, $"{context.StateMachine.Id.Instance}.{context.CurrentState.Name}.{actionName}.{Guid.NewGuid()}", out var a))
            {
                Task.Run(async () =>
                {
                    var integratedActivityBuilder = new StateActionActivityBuilder(buildAction);
                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize().ToEventHolder();

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActivityBuilder.GetSubscribe(context.StateMachine.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.StateMachine.Values).Values }.ToEventHolder(),
                        initializationEvent,
                        integratedActivityBuilder.GetUnsubscribe(context.StateMachine.Id).ToEventHolder()
                    });
                        
                    return a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"On{actionName}Activity '{activityName}' not found", context.StateMachine.Id.StateMachineClass);
            }
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardActivity<TEvent>(ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
        {
            var result = false;
            if (context.TryLocateActivity(activityName, $"{context.StateMachine.Id.Instance}.{context.Source.Name}.{Constants.Guard}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                await Task.Run(async () =>
                {
                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize().ToEventHolder();

                    var tokensInput = new TokensInput();
                    tokensInput.Add(ev);

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActivityBuilder.GetSubscribe(context.StateMachine.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = (context.StateMachine.Values as ContextValuesCollection).Values }.ToEventHolder(),
                        initializationEvent,
                        tokensInput.ToEventHolder(),
                        integratedActivityBuilder.GetUnsubscribe(context.StateMachine.Id).ToEventHolder(),
                        new TokensOutputRequest<bool>().ToEventHolder()
                    });

                    var requestResult = await a.RequestAsync(request);
                    var responseHolder = requestResult.Response.Results.Last().Response as EventHolder<TokensOutput<bool>>;
                    result = responseHolder?.Payload?.GetAll()?.FirstOrDefault() ?? false;
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
        {
            if (context.TryLocateActivity(activityName, $"{context.StateMachine.Id.Instance}.{context.Source.Name}.{Event<TEvent>.Name}.{Constants.Effect}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                Task.Run(async () =>
                {
                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize().ToEventHolder();

                    var tokensInput = new TokensInput();
                    tokensInput.Add(ev);

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActivityBuilder.GetSubscribe(context.StateMachine.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = (context.StateMachine.Values as ContextValuesCollection).Values }.ToEventHolder(),
                        initializationEvent,
                        tokensInput.ToEventHolder(),
                        integratedActivityBuilder.GetUnsubscribe(context.StateMachine.Id).ToEventHolder()
                    });

                    return a.SendAsync(request);
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
