using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    public static class StateMachineActionExtensions
    {
        [DebuggerHidden]
        internal static void RunStateAction(string stateActionName, IStateActionContext context, string actionName, StateActionActionBuildAction buildAction)
        {
            if (context.TryLocateAction(actionName, $"{context.StateMachine.Id.Instance}.{context.CurrentState.Name}.{stateActionName}.{Guid.NewGuid()}", out var a))
            {
                _ = Task.Run(async () =>
                {
                    var integratedActionBuilder = new StateActionActionBuilder(buildAction);
                    EventHolder initializationEvent = (integratedActionBuilder.InitializationBuilder != null)
                        ? await integratedActionBuilder.InitializationBuilder(context)
                        : new Initialize().ToEventHolder();

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActionBuilder.GetSubscribe(context.StateMachine.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.StateMachine.Values).Values }.ToEventHolder(),
                        initializationEvent,
                        new TokensInput().ToEventHolder(),
                        integratedActionBuilder.GetUnsubscribe(context.StateMachine.Id).ToEventHolder()
                    });
                        
                    _ = a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"On{stateActionName}Action '{actionName}' not found", context.StateMachine.Id.StateMachineClass);
            }
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardAction<TEvent>(ITransitionContext<TEvent> context, string actionName, TransitionActionBuildAction<TEvent> buildAction)
        {
            var result = false;
            if (context.TryLocateAction(actionName, $"{context.StateMachine.Id.Instance}.{context.Source.Name}.{Constants.Guard}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                await Task.Run(async () =>
                {
                    var integratedActionBuilder = new TransitionActionBuilder<TEvent>(buildAction);
                    
                    var tokensInput = new TokensInput();
                    tokensInput.Add(ev);

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActionBuilder.GetSubscribe(context.StateMachine.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.StateMachine.Values).Values }.ToEventHolder(),
                        tokensInput.ToEventHolder(),
                        integratedActionBuilder.GetUnsubscribe(context.StateMachine.Id).ToEventHolder()
                    });

                    var requestResult = await a.RequestAsync(request);
                    var responseHolder = requestResult.Response.Results.ToArray()[^2].Response as EventHolder<TokensOutput>;
                    result = responseHolder?.Payload?.GetOfType<bool>().FirstOrDefault() ?? false;
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.StateMachine.Id.StateMachineClass);
            }

            return result;
        }

        [DebuggerHidden]
        internal static Task RunEffectAction<TEvent>(ITransitionContext<TEvent> context, string actionName, TransitionActionBuildAction<TEvent> buildAction)
        {
            if (context.TryLocateAction(actionName, $"{context.StateMachine.Id.Instance}.{context.Source.Name}.{Event<TEvent>.Name}.{Constants.Effect}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                _ = Task.Run(() =>
                {
                    var integratedActionBuilder = new TransitionActionBuilder<TEvent>(buildAction);

                    var tokensInput = new TokensInput();
                    tokensInput.Add(ev);

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActionBuilder.GetSubscribe(context.StateMachine.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.StateMachine.Values).Values }.ToEventHolder(),
                        tokensInput.ToEventHolder(),
                        integratedActionBuilder.GetUnsubscribe(context.StateMachine.Id).ToEventHolder()
                    });

                    _ = a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"EffectAction '{actionName}' not found", context.StateMachine.Id.StateMachineClass);
            }

            return Task.CompletedTask;
        }
    }
}
