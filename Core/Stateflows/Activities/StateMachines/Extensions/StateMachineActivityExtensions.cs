using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;

namespace Stateflows.Activities
{
    public static class StateMachineActivityExtensions
    {
        [DebuggerHidden]
        internal static async Task RunStateActivityAsync(string stateActionName, IStateActionContext context, string activityName, StateActionActivityBuildAction buildAction)
        {
            var integratedActivityBuilder = new ActionActivityBuilder(buildAction);
            var instance = await integratedActivityBuilder.GetInstanceAsync(context, context.GetBehaviorInstance($"{stateActionName}:{new Random().Next()}"));
            if (!context.TryLocateActivity(activityName, instance, out var a))
            {
                throw new StateMachineRuntimeException($"On{stateActionName}Activity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var initializationEvent = integratedActivityBuilder.InitializationBuilder != null
                ? await integratedActivityBuilder.InitializationBuilder(context)
                : new Initialize();

            var request = new CompoundRequest()
                .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
            ;
            request.Events.Add(initializationEvent.ToTypedEventHolder());
                
            _ = a.SendAsync(request);
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardActivityAsync<TEvent>(int guardIndex, ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
        {
            var transitionContext = (TransitionContext<TEvent>)context;
            var edgeGuardIdentifier = $"{transitionContext.Edge.Identifier}.{guardIndex.ToString()}.{activityName}";
            
            var guardResponse = context.Headers.OfType<GuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == edgeGuardIdentifier)
            {
                return true;
            }
            
            var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
            var instance = await integratedActivityBuilder.GetInstanceAsync(
                context,
                context.GetBehaviorInstance(context.Target != null
                    ? $"{Event<TEvent>.Name}:{context.Target.Name}:{Constants.Guard}:{context.EventId}"
                    : $"{Event<TEvent>.Name}:{Constants.Guard}:{context.EventId}"
                )
            );
            if (!context.TryLocateActivity(activityName, instance, out var a))
            {
                throw new StateMachineRuntimeException($"GuardActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var headers = transitionContext.Context.EventHolder.Headers
                .Where(h => !(h is GuardDelegation))
                .Append(
                    new GuardRequest()
                    {
                        GuardIdentifier = edgeGuardIdentifier,
                        TargetName = transitionContext.Edge.TargetName,
                        SourceName = transitionContext.Edge.SourceName,
                        EdgeType = transitionContext.Edge.Type
                    }
                )
                .ToArray();
            
            var ev = StateflowsJsonConverter.Clone(context.Event);
            var initializationEvent = integratedActivityBuilder.InitializationBuilder != null
                ? await integratedActivityBuilder.InitializationBuilder(context)
                : new Initialize();
            
            // var tokensInput = new TokensInput();
            // tokensInput.Add(ev);
            
            var request = new CompoundRequest()
                .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
            ;
            request.Events.Add(initializationEvent.ToTypedEventHolder());
            request.Add(ev, headers)
                // .Add(new TokensOutputRequest<bool>())
            ;
            // var request = new CompoundRequest()
            //     .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
            //     .Add(context.Event, headers)
            // ;

            transitionContext.Context.EventHolder.Headers.Add(new GuardDelegation() { EdgeIdentifier = transitionContext.Edge.Identifier });

            _ = a.RequestAsync(request);
            
            return false;
            // var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
            // var instance = await integratedActivityBuilder.GetInstanceAsync(context, context.GetBehaviorInstance($"{Event<TEvent>.Name}.{Constants.Guard}.{context.EventId}"));
            // if (!context.TryLocateActivity(activityName, instance, out var a))
            // {
            //     throw new StateMachineRuntimeException($"GuardActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            // }
            //
            // var ev = StateflowsJsonConverter.Clone(context.Event);
            // var initializationEvent = integratedActivityBuilder.InitializationBuilder != null
            //     ? await integratedActivityBuilder.InitializationBuilder(context)
            //     : new Initialize();
            //
            // var tokensInput = new TokensInput();
            // tokensInput.Add(ev);
            //
            // var request = new CompoundRequest()
            //     .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
            //     //.Add(new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values })
            // ;
            // request.Events.Add(initializationEvent.ToTypedEventHolder());
            // request
            //     .Add(tokensInput)
            //     .Add(new TokensOutputRequest<bool>())
            // ;
            //
            // var requestResult = await a.RequestAsync(request);
            // var responseHolder = requestResult.Response.Results.Last().Response as EventHolder<TokensOutput<bool>>;
            // return responseHolder?.Payload?.GetAll()?.FirstOrDefault() ?? false;
        }

        [DebuggerHidden]
        internal static async Task RunEffectActivity<TEvent>(ITransitionContext<TEvent> context, string activityName, TransitionActivityBuildAction<TEvent> buildAction)
        {
            var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
            var instance = await integratedActivityBuilder.GetInstanceAsync(context, context.GetBehaviorInstance($"{Event<TEvent>.Name}.{Constants.Effect}.{context.EventId}"));
            if (!context.TryLocateActivity(activityName, instance, out var a))
            {
                throw new StateMachineRuntimeException($"EffectActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var ev = StateflowsJsonConverter.Clone(context.Event);
            var initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                ? await integratedActivityBuilder.InitializationBuilder(context)
                : new Initialize();

            var tokensInput = new TokensInput();
            tokensInput.Add(ev);

            var request = new CompoundRequest()
                .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
                //.Add(new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values })
            ;
            request.Events.Add(initializationEvent.ToTypedEventHolder());
            request.Add(tokensInput);

            _ = a.SendAsync(request);
        }
    }
}
