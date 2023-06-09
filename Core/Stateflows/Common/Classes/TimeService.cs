﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Stateflows.Common.Classes
{
    internal class TimeService : ITimeService, IHostedService
    {
        public List<TimeToken> Tokens { get; set; } = new List<TimeToken>();

        public Dictionary<TimeToken, string> IdsByTokens { get; set; } = new Dictionary<TimeToken, string>();

        public Dictionary<string, TimeToken> TokensByIds { get; set; } = new Dictionary<string, TimeToken>();

        public System.Timers.Timer Timer { get; private set; } = new System.Timers.Timer(1000 * 10) { AutoReset = true, Enabled = false };

        private IStateflowsStorage Storage { get; }

        private IServiceProvider Provider { get; }

        private IBehaviorLocator locator = null;

        private IBehaviorLocator Locator => locator ?? (locator = Provider.GetService<IBehaviorLocator>());

        public TimeService(IStateflowsStorage storage, IBehaviorClassesProvider behaviorClassesProvider, IServiceProvider provider)
        {
            Storage = storage;
            Provider = provider;

            Task.Run(async () =>
            {
                Tokens.AddRange(
                    await Storage.GetTimeTokens(
                        behaviorClassesProvider.LocalBehaviorClasses
                    )
                );

                foreach (var token in Tokens)
                {
                    TokensByIds[token.Id] = token;
                    IdsByTokens[token] = token.Id;
                }
            });
        }

        public async Task Clear(BehaviorId behaviorId, IEnumerable<string> ids)
        {
            if (ids.Count() == 0)
            {
                return;
            }

            foreach (var id in ids)
            {
                if (TokensByIds.TryGetValue(id, out var token))
                {
                    Tokens.Remove(token);
                    IdsByTokens.Remove(token);
                    TokensByIds.Remove(id);
                }
            }

            await Storage.ClearTimeTokens(behaviorId, ids);
        }

        public async Task Register(TimeToken[] timeTokens)
        {
            await Storage.AddTimeTokens(timeTokens);
            Tokens.AddRange(timeTokens);
            foreach (var token in timeTokens)
            {
                TokensByIds[token.Id] = token;
                IdsByTokens[token] = token.Id;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Timer.Elapsed += Tick;
            Timer.Enabled = true;

            return Task.CompletedTask;
        }

        private async void Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            var passedTokens = Tokens.Where(t => t.Event.ShouldTrigger(t.CreatedAt)).ToArray();
            if (passedTokens.Count() == 0)
            {
                return;
            }

            var ids = new List<string>();
            foreach (var token in passedTokens)
            {
                if (IdsByTokens.TryGetValue(token, out var id))
                {
                    ids.Add(id);
                }

                if (Locator.TryLocateBehavior(token.TargetId, out var behavior))
                {
                    await behavior.SendAsync(token.Event);
                }

                Tokens.Remove(token);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Timer.Enabled = false;

            return Task.CompletedTask;
        }
    }
}
