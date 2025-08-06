using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;

namespace Stateflows.Activities
{
    public static class StateMachineActionExtensions
    {
        [DebuggerHidden]
        internal static void RunStateAction(string stateActionName, IStateActionContext context, string actionName, StateActionActionBuildAction buildAction)
        {
            if (context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}.{context.State.Name}.{stateActionName}.{Guid.NewGuid()}", out var a))
            {
                _ = Task.Run(async () =>
                {
                    var integratedActionBuilder = new ActionActionBuilder(buildAction);

                    var tokensInput = new TokensInput();

                    if (integratedActionBuilder.InitializationBuilder != null)
                    {
                        tokensInput.Add((await integratedActionBuilder.InitializationBuilder(context)).BoxedPayload);
                    }
                    
                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        new SetContextOwner() { ContextOwner = context.Behavior.Id }.ToEventHolder(),
                        integratedActionBuilder.GetSubscribe(context.Behavior.Id).ToEventHolder(),
                        integratedActionBuilder.GetStartRelay(context.Behavior.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values }.ToEventHolder(),
                        tokensInput.ToEventHolder(),
                        integratedActionBuilder.GetStopRelay(context.Behavior.Id).ToEventHolder(),
                        integratedActionBuilder.GetUnsubscribe(context.Behavior.Id).ToEventHolder()
                    });
                        
                    _ = a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"On{stateActionName}Action '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardAction<TEvent>(ITransitionContext<TEvent> context, string actionName, TransitionActionBuildAction<TEvent> buildAction)
        {
            var result = false;
            if (context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}.{context.Source.Name}.{Constants.Guard}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                await Task.Run(async () =>
                {
                    var integratedActionBuilder = new TransitionActionBuilder<TEvent>(buildAction);

                    var tokensInput = new TokensInput();

                    if (integratedActionBuilder.InitializationBuilder != null)
                    {
                        tokensInput.Add((await integratedActionBuilder.InitializationBuilder(context)).BoxedPayload);
                    }
                    
                    tokensInput.Add(ev);

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActionBuilder.GetSubscribe(context.Behavior.Id).ToEventHolder(),
                        integratedActionBuilder.GetStartRelay(context.Behavior.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values }.ToEventHolder(),
                        tokensInput.ToEventHolder(),
                        integratedActionBuilder.GetStopRelay(context.Behavior.Id).ToEventHolder(),
                        integratedActionBuilder.GetUnsubscribe(context.Behavior.Id).ToEventHolder()
                    });

                    var requestResult = await a.RequestAsync(request);
                    var responseHolder = requestResult.Response.Results.ToArray()[^3].Response as EventHolder<TokensOutput>;
                    result = responseHolder?.Payload?.GetOfType<bool>().FirstOrDefault() ?? false;
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            return result;
        }

        [DebuggerHidden]
        internal static Task RunEffectAction<TEvent>(ITransitionContext<TEvent> context, string actionName, TransitionActionBuildAction<TEvent> buildAction)
        {
            if (context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}.{context.Source.Name}.{Event<TEvent>.Name}.{Constants.Effect}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                _ = Task.Run(async () =>
                {
                    var integratedActionBuilder = new TransitionActionBuilder<TEvent>(buildAction);

                    var tokensInput = new TokensInput();

                    if (integratedActionBuilder.InitializationBuilder != null)
                    {
                        tokensInput.Add((await integratedActionBuilder.InitializationBuilder(context)).BoxedPayload);
                    }
                    
                    tokensInput.Add(ev);

                    var request = new CompoundRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActionBuilder.GetSubscribe(context.Behavior.Id).ToEventHolder(),
                        integratedActionBuilder.GetStartRelay(context.Behavior.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values }.ToEventHolder(),
                        tokensInput.ToEventHolder(),
                        integratedActionBuilder.GetStopRelay(context.Behavior.Id).ToEventHolder(),
                        integratedActionBuilder.GetUnsubscribe(context.Behavior.Id).ToEventHolder()
                    });

                    _ = a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"EffectAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            return Task.CompletedTask;
        }
    }
}
