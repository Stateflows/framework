using Microsoft.AspNetCore.SignalR.Client;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Transport.AspNetCore.SignalR.Client
{
    internal class BehaviorProvider : IBehaviorProvider
    {
        private string Url { get; }

        private HubConnection? Hub { get; set; }

        public BehaviorProvider(string url)
        {
            Url = url;

            if (Url[^1] != '/')
            {
                Url += '/';
            }

            Url += "stateflows_v1";

            Task.Run(() => GetHubAsync());
        }

        public bool IsLocal => false;

        public IEnumerable<BehaviorClass> BehaviorClasses { get; set; } = new List<BehaviorClass>();

        public event ActionAsync<IBehaviorProvider>? BehaviorClassesChanged;

        public async Task<HubConnection> GetHubAsync()
        {
            if (Hub != null)
            {
                if (Hub.State != HubConnectionState.Connected)
                {
                    await Hub.StartAsync();
                }
            }
            else
            {
                Hub = new HubConnectionBuilder()
                    .WithUrl(Url)
                    .Build();

                await Hub.StartAsync();

                BehaviorClasses = await Hub.InvokeAsync<IEnumerable<BehaviorClass>>("GetAvailableClasses");

                BehaviorClassesChanged?.Invoke(this);
            }

            return Hub;
        }

        public bool TryProvideBehavior(BehaviorId id, out IBehavior? behavior)
        {
            behavior = BehaviorClasses.Any(c => id.BehaviorClass == c)
                ? new Behavior(GetHubAsync(), id)
                : null;

            return behavior != null;
        }
    }
}
