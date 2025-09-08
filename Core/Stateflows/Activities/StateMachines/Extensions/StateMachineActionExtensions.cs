using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;

namespace Stateflows.Activities
{
    public static class StateMachineActionExtensions
    {
        [DebuggerHidden]
        internal static async Task RunStateActionAsync(string stateActionName, IStateActionContext context, string actionName, StateActionActionBuildAction buildAction)
        {
            var integratedActionBuilder = new ActionActionBuilder(buildAction);
            var instance = await integratedActionBuilder.GetInstanceAsync(context, context.GetBehaviorInstance($"{stateActionName}:{new Random().Next()}"));
            if (!context.TryLocateAction(actionName, instance, out var a))
            {
                throw new StateMachineRuntimeException($"On{stateActionName}Action '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var tokensInput = new TokensInput();

            var request = new CompoundRequest()
                .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
                .Add(tokensInput)
            ;
                
            _ = a.SendAsync(request);
        }

        [DebuggerHidden]
        internal static async Task<bool> RunGuardActionAsync<TEvent>(int guardIndex, ITransitionContext<TEvent> context, string actionName, TransitionActionBuildAction<TEvent> buildAction)
        {
            var transitionContext = (TransitionContext<TEvent>)context;
            var edgeGuardIdentifier = $"{transitionContext.Edge.Identifier}.{guardIndex.ToString()}.{actionName}";
            
            var guardResponse = context.Headers.OfType<GuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == edgeGuardIdentifier)
            {
                return true;
            }
            
            var integratedActionBuilder = new TransitionActionBuilder<TEvent>(buildAction);
            var instance = await integratedActionBuilder.GetInstanceAsync(
                context,
                context.GetBehaviorInstance(context.Target != null
                    ? $"{Event<TEvent>.Name}:{context.Target.Name}:{Constants.Guard}:{context.EventId}"
                    : $"{Event<TEvent>.Name}:{Constants.Guard}:{context.EventId}"
                )
            );
            if (!context.TryLocateAction(actionName, instance, out var a))
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
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
            
            var request = new CompoundRequest()
                .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
                .Add(context.Event, headers)
            ;

            transitionContext.Context.EventHolder.Headers.Add(new GuardDelegation() { EdgeIdentifier = transitionContext.Edge.Identifier });

            _ = a.RequestAsync(request);
            
            return false;
        }

        [DebuggerHidden]
        internal static async Task RunEffectActionAsync<TEvent>(ITransitionContext<TEvent> context, string actionName, TransitionActionBuildAction<TEvent> buildAction)
        {
            var integratedActionBuilder = new TransitionActionBuilder<TEvent>(buildAction);
            var instance = await integratedActionBuilder.GetInstanceAsync(
                context,
                context.GetBehaviorInstance(context.Target != null
                    ? $"{Event<TEvent>.Name}:{context.Target.Name}:{Constants.Effect}:{context.EventId}"
                    : $"{Event<TEvent>.Name}:{Constants.Effect}:{context.EventId}"
                )
            );
            if (!context.TryLocateAction(actionName, instance, out var a))
            {
                throw new StateMachineRuntimeException($"EffectAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var ev = StateflowsJsonConverter.Clone(context.Event);

            var tokensInput = new TokensInput();
            
            tokensInput.Add(ev);

            var request = new CompoundRequest()
                .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
                .Add(tokensInput)
            ;

            _ = a.SendAsync(request);
        }
    }
}
