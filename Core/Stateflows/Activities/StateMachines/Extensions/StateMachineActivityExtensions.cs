using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;

namespace Stateflows.Activities
{
    public static class StateMachineActivityExtensions
    {
        [DebuggerHidden]
        internal static void RunStateActivity(string actionName, IStateActionContext context, string activityName, StateActionActivityBuildAction buildAction)
        {
            if (context.TryLocateActivity(activityName, $"{actionName}:{new Random().Next()}", out var a))
            {
                _ = Task.Run(async () =>
                {
                    var integratedActivityBuilder = new ActionActivityBuilder(buildAction);
                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize().ToEventHolder();

                    var request = new CompoundRequestBuilderRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActivityBuilder.GetSubscribe(context.Behavior.Id).ToEventHolder(),
                        integratedActivityBuilder.GetStartRelay(context.Behavior.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values }.ToEventHolder(),
                        initializationEvent,
                        integratedActivityBuilder.GetStopRelay(context.Behavior.Id).ToEventHolder(),
                        integratedActivityBuilder.GetUnsubscribe(context.Behavior.Id).ToEventHolder()
                    });
                        
                    _ = a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"On{actionName}Activity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardActivity<TEvent>(ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
        {
            var result = false;
            if (context.TryLocateActivity(activityName, $"{context.Behavior.Id.Instance}.{context.Source.Name}.{Constants.Guard}.{context.EventId}", out var a))
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

                    var request = new CompoundRequestBuilderRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActivityBuilder.GetSubscribe(context.Behavior.Id).ToEventHolder(),
                        integratedActivityBuilder.GetStartRelay(context.Behavior.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values }.ToEventHolder(),
                        initializationEvent,
                        tokensInput.ToEventHolder(),
                        integratedActivityBuilder.GetStopRelay(context.Behavior.Id).ToEventHolder(),
                        integratedActivityBuilder.GetUnsubscribe(context.Behavior.Id).ToEventHolder(),
                        new TokensOutputRequest<bool>().ToEventHolder()
                    });

                    var requestResult = await a.RequestAsync(request);
                    var responseHolder = requestResult.Response.Results.Last().Response as EventHolder<TokensOutput<bool>>;
                    result = responseHolder?.Payload?.GetAll()?.FirstOrDefault() ?? false;
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"GuardActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            return result;
        }

        [DebuggerHidden]
        internal static Task RunEffectActivity<TEvent>(ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
        {
            if (context.TryLocateActivity(activityName, $"{context.Behavior.Id.Instance}.{context.Source.Name}.{Event<TEvent>.Name}.{Constants.Effect}.{context.EventId}", out var a))
            {
                var ev = StateflowsJsonConverter.Clone(context.Event);
                _ = Task.Run(async () =>
                {
                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                        ? await integratedActivityBuilder.InitializationBuilder(context)
                        : new Initialize().ToEventHolder();

                    var tokensInput = new TokensInput();
                    tokensInput.Add(ev);

                    var request = new CompoundRequestBuilderRequest();
                    request.Events.AddRange(new EventHolder[]
                    {
                        integratedActivityBuilder.GetSubscribe(context.Behavior.Id).ToEventHolder(),
                        integratedActivityBuilder.GetStartRelay(context.Behavior.Id).ToEventHolder(),
                        new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values }.ToEventHolder(),
                        initializationEvent,
                        tokensInput.ToEventHolder(),
                        integratedActivityBuilder.GetStopRelay(context.Behavior.Id).ToEventHolder(),
                        integratedActivityBuilder.GetUnsubscribe(context.Behavior.Id).ToEventHolder()
                    });

                    _ = a.SendAsync(request);
                });
            }
            else
            {
                throw new StateMachineRuntimeException($"EffectActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            return Task.CompletedTask;
        }
    }
}
