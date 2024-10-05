using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>
    /// Behavior handle
    /// </summary>
    public interface IBehavior : IWatches, IDisposable
    {
        BehaviorId Id { get; }

        Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null);
        
        //Task<SendResult> SendAsync(object @event, Type eventType, IEnumerable<EventHeader> headers = null);

        Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IEnumerable<EventHeader> headers = null);

        Task<RequestResult<CompoundResponse>> SendCompoundAsync(Action<ICompound> builderAction, IEnumerable<EventHeader> headers = null)
        {
            var compound = new CompoundRequest();
            builderAction(compound);
            return RequestAsync(compound, headers);
        }

        Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full)
            => SendAsync(new Reset() { Mode = resetMode });

        Task<SendResult> FinalizeAsync()
            => SendAsync(new Finalize());

        Task<RequestResult<BehaviorInfo>> GetStatusAsync()
            => RequestAsync(new BehaviorInfoRequest());

        async Task<IWatcher> WatchStatusAsync(Action<BehaviorInfo> handler, bool immediateRequest = true)
        {
            var watcher = await WatchAsync(handler);

            if (immediateRequest)
            {
                var result = await GetStatusAsync();
                if (result.Status == EventStatus.Consumed)
                {
                    _ = Task.Run(() => handler(result.Response));
                }
            }

            return watcher;
        }
    }
}