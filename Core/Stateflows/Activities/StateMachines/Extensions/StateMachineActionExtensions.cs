using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
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

            _ = a.SendAsync(
                new TokensInput(),
                new Dictionary<string, EventHeader>
                {
                    {
                        nameof(BehaviorEmbedding),
                        new BehaviorEmbedding()
                        {
                            OwnerId = context.TryGetOwnerBehaviorContext(out var ownerBehavior)
                                ? ownerBehavior.Id
                                : context.Behavior.Id,
                            ParentId = context.Behavior.Id
                        }
                    }
                }
            );

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        internal static Task<bool> RunDeferralGuardActionAsync<TEvent>(int guardIndex, IDeferralContext<TEvent> context, string actionName)
        {
            var deferralContext = (DeferralContext<TEvent>)context;
            var deferralGuardIdentifier = $"{deferralContext.State.Name}.{Event<TEvent>.Name}.{guardIndex.ToString()}.{actionName}";
            
            var guardResponse = context.Headers.Values.OfType<TransitionGuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == deferralGuardIdentifier)
            {
                return Task.FromResult(true);
            }
            
            if (!context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var headers = deferralContext.Context.EventHolder.Headers
                .Where(h => h.Value is not TransitionGuardDelegation)
                .ToDictionary();

            headers[nameof(DeferralGuardRequest)] = new DeferralGuardRequest
            {
                GuardIdentifier = deferralGuardIdentifier,
                StateName = deferralContext.State.Name,
            };

            headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = context.TryGetOwnerBehaviorContext(out var ownerBehavior)
                    ? ownerBehavior.Id
                    : context.Behavior.Id,
                ParentId = context.Behavior.Id
            };

            deferralContext.Context.EventHolder.Headers[nameof(DeferralGuardDelegation)] = new DeferralGuardDelegation
            {
                VertexIdentifier = deferralContext.State.Name,
                EventName = Event<TEvent>.Name
            };

            _ = a.SendAsync(context.Event, headers);
            
            return Task.FromResult(false);
        }

        [DebuggerHidden]
        internal static Task<bool> RunTransitionGuardActionAsync<TEvent>(int guardIndex, ITransitionContext<TEvent> context, string actionName)
        {
            var transitionContext = (TransitionContext<TEvent>)context;
            var edgeGuardIdentifier = $"{transitionContext.Edge.Identifier}.{guardIndex.ToString()}.{actionName}";
            
            var guardResponse = context.Headers.Values.OfType<TransitionGuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == edgeGuardIdentifier)
            {
                return Task.FromResult(true);
            }
            
            if (!context.TryLocateAction(actionName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"GuardAction '{actionName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var headers = transitionContext.Context.EventHolder.Headers
                .Where(h => h.Value is not TransitionGuardDelegation)
                .ToDictionary();

            headers[nameof(TransitionGuardRequest)] = new TransitionGuardRequest
            {
                GuardIdentifier = edgeGuardIdentifier,
                TargetName = transitionContext.Edge.TargetName,
                SourceName = transitionContext.Edge.SourceName,
                EdgeType = transitionContext.Edge.Type
            };

            headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = context.TryGetOwnerBehaviorContext(out var ownerBehavior)
                    ? ownerBehavior.Id
                    : context.Behavior.Id,
                ParentId = context.Behavior.Id
            };
            
            transitionContext.Context.EventHolder.Headers[nameof(TransitionGuardDelegation)] = new TransitionGuardDelegation
            {
                EdgeIdentifier = transitionContext.Edge.Identifier
            };
            
            var ev = StateflowsJsonConverter.Clone(context.Event);

            _ = a.SendAsync(ev, headers);
            
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

            _ = a.SendAsync(
                new TokensInput().Add(ev),
                new Dictionary<string, EventHeader>
                {
                    {
                        nameof(BehaviorEmbedding),
                        new BehaviorEmbedding()
                        {
                            OwnerId = context.TryGetOwnerBehaviorContext(out var ownerBehavior)
                                ? ownerBehavior.Id
                                : context.Behavior.Id,
                            ParentId = context.Behavior.Id
                        }
                    }
                }
            );

            return Task.CompletedTask;
        }
    }
}
