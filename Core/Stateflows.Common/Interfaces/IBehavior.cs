using System;
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

        Task<SendResult> SendAsync<TEvent>(TEvent @event, params EventHeader[] headers);

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, params EventHeader[] headers);

        Task<RequestResult<CompoundResponse>> SendCompoundAsync(Action<ICompound> builderAction, params EventHeader[] headers)
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