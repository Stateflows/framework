using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common.Utilities;
using Stateflows.Utils;

namespace Stateflows.Common.Extensions;

internal static class TokenOutputExtensions
{
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
