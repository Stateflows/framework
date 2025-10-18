using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;

namespace Stateflows.Activities
{
    public static class StateMachineActivityExtensions
    {
        [DebuggerHidden]
        internal static Task RunStateActivityAsync(string stateActionName, IStateActionContext context, string activityName)
        {
            if (!context.TryLocateActivity(activityName, $"{context.Behavior.Id.Instance}:{new Random().Next()}", out var a))
            {
                throw new StateMachineRuntimeException($"On{stateActionName}Activity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }
            
            var request = new CompoundRequest()
                .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                .Add(new Initialize())
            ;
                
            _ = a.SendAsync(request);

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        internal static Task<bool> RunGuardActivityAsync<TEvent>(int guardIndex, ITransitionContext<TEvent> context, string activityName)
        {
            var transitionContext = (TransitionContext<TEvent>)context;
            var edgeGuardIdentifier = $"{transitionContext.Edge.Identifier}.{guardIndex.ToString()}.{activityName}";
            
            var guardResponse = context.Headers.OfType<GuardResponse>().FirstOrDefault();
            if (guardResponse != null && guardResponse.GuardIdentifier == edgeGuardIdentifier)
            {
                return Task.FromResult(true);
            }
            
            if (!context.TryLocateActivity(activityName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"GuardActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var headers = transitionContext.Context.EventHolder.Headers
                .Where(h => h is not GuardDelegation)
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
            
            var request = new CompoundRequest()
                .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                .Add(new Initialize())
                .Add(ev, headers)
            ;

            transitionContext.Context.EventHolder.Headers.Add(new GuardDelegation() { EdgeIdentifier = transitionContext.Edge.Identifier });

            _ = a.RequestAsync(request);
            
            return Task.FromResult(false);
        }

        [DebuggerHidden]
        internal static Task RunEffectActivity<TEvent>(ITransitionContext<TEvent> context, string activityName)
        {
            if (!context.TryLocateActivity(activityName, $"{context.Behavior.Id.Instance}:{context.EventId}", out var a))
            {
                throw new StateMachineRuntimeException($"EffectActivity '{activityName}' not found", context.Behavior.Id.BehaviorClass);
            }

            var ev = StateflowsJsonConverter.Clone(context.Event);

            var tokensInput = new TokensInput();
            tokensInput.Add(ev);

            var request = new CompoundRequest()
                .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                .Add(new Initialize())
            ;
            request.Add(tokensInput);

            _ = a.SendAsync(request);
            
            return Task.CompletedTask;
        }
    }
}
