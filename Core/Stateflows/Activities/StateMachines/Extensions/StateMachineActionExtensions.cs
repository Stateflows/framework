using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;

namespace Stateflows.Activities
{
    public static class StateMachineActionExtensions
    {
        [DebuggerHidden]
        internal static Task RunStateActionAsync(string stateActionName, IStateActionContext context, string actionName)
        {
            if (!context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}:{new Random().Next()}", out var a))
            {
                throw new StateMachineRuntimeException($"On{stateActionName}Action '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var tokensInput = new TokensInput();

            var request = new CompoundRequest()
                .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                .Add(tokensInput)
            ;
                
            _ = a.SendAsync(request);

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        internal static Task<bool> RunDeferralGuardActionAsync<TEvent>(int guardIndex, IDeferralContext<TEvent> context, string actionName)
        {
            var deferralContext = (DeferralContext<TEvent>)context;
            var deferralGuardIdentifier = $"{deferralContext.State.Name}.{Event<TEvent>.Name}.{guardIndex.ToString()}.{actionName}";
            
            var guardResponse = context.Headers.OfType<TransitionGuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == deferralGuardIdentifier)
            {
                return Task.FromResult(true);
            }
            
            if (!context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var headers = deferralContext.Context.EventHolder.Headers
                .Where(h => !(h is TransitionGuardDelegation))
                .Append(
                    new DeferralGuardRequest()
                    {
                        GuardIdentifier = deferralGuardIdentifier,
                        StateName = deferralContext.State.Name,
                    }
                )
                .ToArray();
            
            var request = new CompoundRequest()
                    .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                    .Add(context.Event, headers)
                ;

            deferralContext.Context.EventHolder.Headers.Add(new DeferralGuardDelegation() { VertexIdentifier = deferralContext.State.Name, EventName = Event<TEvent>.Name});

            _ = a.RequestAsync(request);
            
            return Task.FromResult(false);
        }

        [DebuggerHidden]
        internal static Task<bool> RunTransitionGuardActionAsync<TEvent>(int guardIndex, ITransitionContext<TEvent> context, string actionName)
        {
            var transitionContext = (TransitionContext<TEvent>)context;
            var edgeGuardIdentifier = $"{transitionContext.Edge.Identifier}.{guardIndex.ToString()}.{actionName}";
            
            var guardResponse = context.Headers.OfType<TransitionGuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == edgeGuardIdentifier)
            {
                return Task.FromResult(true);
            }
            
            if (!context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var headers = transitionContext.Context.EventHolder.Headers
                .Where(h => !(h is TransitionGuardDelegation))
                .Append(
                    new TransitionGuardRequest()
                    {
                        GuardIdentifier = edgeGuardIdentifier,
                        TargetName = transitionContext.Edge.TargetName,
                        SourceName = transitionContext.Edge.SourceName,
                        EdgeType = transitionContext.Edge.Type
                    }
                )
                .ToArray();
            
            var request = new CompoundRequest()
                .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                .Add(context.Event, headers)
            ;

            transitionContext.Context.EventHolder.Headers.Add(new TransitionGuardDelegation() { EdgeIdentifier = transitionContext.Edge.Identifier });

            _ = a.RequestAsync(request);
            
            return Task.FromResult(false);
        }

        [DebuggerHidden]
        internal static Task RunEffectActionAsync<TEvent>(ITransitionContext<TEvent> context, string actionName)
        {
            if (!context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"EffectAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var ev = StateflowsJsonConverter.Clone(context.Event);

            var tokensInput = new TokensInput();
            
            tokensInput.Add(ev);

            var request = new CompoundRequest()
                .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                .Add(tokensInput)
            ;

            _ = a.SendAsync(request);

            return Task.CompletedTask;
        }
    }
}
