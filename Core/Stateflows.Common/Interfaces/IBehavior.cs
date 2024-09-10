using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        Task WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);

        Task UnwatchStatusAsync()
            => UnwatchAsync<BehaviorInfo>();
    }
}