using Microsoft.Extensions.AI;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class TransitionBuilderExtensions
{
    internal static ITransitionBuilder<TEvent> AddAgenticEffect<TEvent>(this ITransitionBuilder<TEvent> transitionBuilder,
        Func<TEvent, AIContent> aiContentFactory)
        => transitionBuilder.AddEffect(c => c.Behavior.Send(new ChatMessage()
            {
                Contents = [ aiContentFactory(c.Event) ],
                Role = ChatRole.System
            }
        ));

    internal static IInternalTransitionBuilder<TEvent> AddAgenticEffect<TEvent>(
        this IInternalTransitionBuilder<TEvent> transitionBuilder,
        Func<ITransitionContext<TEvent>, AIContent> aiContentFactory)
        => transitionBuilder.AddEffect(c => c.Behavior.Send(new ChatMessage()
            {
                Contents = [ aiContentFactory(c) ],
                Role = ChatRole.System
            }
        ));

    internal static IInternalTransitionBuilder<TEvent> AddAgenticEffect<TEvent>(
        this IInternalTransitionBuilder<TEvent> transitionBuilder,
        Func<ITransitionContext<TEvent>, ChatMessage> chatMessageFactory)
        => transitionBuilder.AddEffect(c => c.Behavior.Send(chatMessageFactory(c)));

    internal static ITransitionBuilder<TEvent> AddAgenticGuard<TEvent>(this ITransitionBuilder<TEvent> transitionBuilder,
        Func<TEvent, AIContent> aiContentFactory)
        => transitionBuilder.AddGuard(c =>
        {
            var inquiryAcceptance = c.Headers.Values.OfType<TransitionGuardInquiryAcceptance>().FirstOrDefault();
            if (inquiryAcceptance != null)
            {
                return true;
            }

            c.Behavior.Send(new AgenticChatInquiry()
                {
                    Message = new ChatMessage()
                    {
                        Contents = [ aiContentFactory(c.Event) ],
                        Role = ChatRole.System
                    },
                    GuardTriggerHolder = c.Event.ToEventHolder(c.Headers)
                }
            );

            return false;
        });

    internal static IInternalTransitionBuilder<TEvent> AddAgenticGuard<TEvent>(
        this IInternalTransitionBuilder<TEvent> transitionBuilder,
        Func<TEvent, AIContent> aiContentFactory)
        => transitionBuilder.AddGuard(c =>
        {
            var inquiryAcceptance = c.Headers.Values.OfType<TransitionGuardInquiryAcceptance>().FirstOrDefault();
            if (inquiryAcceptance != null)
            {
                return true;
            }

            c.Behavior.Send(new AgenticChatInquiry()
                {
                    Message = new ChatMessage()
                    {
                        Contents = [ aiContentFactory(c.Event) ],
                        Role = ChatRole.System
                    },
                    GuardTriggerHolder = c.Event.ToEventHolder(c.Headers)
                }
            );

            return false;
        });
}