using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Utils;

namespace Stateflows.Common.Extensions;

internal static class TokenOutputExtensions
{
    private static readonly ConcurrentDictionary<Type, Func<IStateflowsSubscriber, StateflowsContext, EventHolder, Task>> Publishers = new();

    internal static async Task PublishOutputNotificationsAsync(
        this IEnumerable<TokenHolder> tokens, IStateflowsSubscriber subscriber, StateflowsContext context)
    {
        foreach (var notification in tokens.ToOutputNotifications(context.Id))
        {
            var publish = Publishers.GetOrAdd(notification.PayloadType, type => typeof(TokenOutputExtensions)
                .GetMethod(nameof(PublishNotificationAsync), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type)
                .CreateDelegate<Func<IStateflowsSubscriber, StateflowsContext, EventHolder, Task>>());
            await publish(subscriber, context, notification);
        }
    }

    private static Task PublishNotificationAsync<TNotification>(
        IStateflowsSubscriber subscriber, StateflowsContext context, EventHolder notification)
        => subscriber.PublishRangeAsync(context.Id, new[] { ((EventHolder<TNotification>)notification).Payload }, context);

    internal static IEnumerable<EventHolder> ToOutputNotifications(
        this IEnumerable<TokenHolder> tokens, BehaviorId senderId)
    {
        var output = tokens.ToList();
        if (output.Count == 0)
        {
            return [];
        }

        var tokenTypes = output
            .Select(token => token.GetEffectivePayloadType())
            .Distinct()
            .SelectMany(type =>
            {
                var types = new List<Type>();
                for (var current = type; current != null && current != typeof(object); current = current.BaseType)
                {
                    types.Add(current);
                }

                types.AddRange(type.GetInterfaces());
                return types;
            })
            .Distinct();

        var notifications = tokenTypes.Select(type =>
        {
            var notificationType = typeof(TokensOutput<>).MakeGenericType(type);
            var notification = (TokensTransferEvent)Activator.CreateInstance(notificationType)!;
            notification.Tokens = output
                .Where(token => type.IsAssignableFrom(token.GetEffectivePayloadType()))
                .ToList();

            return notification.ToTypedEventHolder(senderId);
        }).ToList();

        notifications.Add(new TokensOutput { Tokens = output }.ToEventHolder(senderId));
        return notifications;
    }
}
