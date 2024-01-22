using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Tools.Tracer.Utils;

namespace Stateflows.Tools.Tracer.Classes
{
    internal class TraceRepository : BackgroundService
    {
        public EventQueue<Trace> Traces { get; } = new EventQueue<Trace>(true);

        private readonly string StorageDirectory;

        public TraceRepository(string storageDirectory)
        {
             StorageDirectory = storageDirectory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Traces.WaitAsync();

                var trace = Traces.Dequeue();

                if (trace != null)
                {
                    string data;
                    try
                    {
                        data = StateflowsJsonConverter.SerializeObject(trace, Newtonsoft.Json.Formatting.Indented);
                    }
                    catch (Exception)
                    {
                        trace.Context = null;
                        data = StateflowsJsonConverter.SerializeObject(trace, Newtonsoft.Json.Formatting.Indented);
                    }

                    var directory = $"{StorageDirectory}\\{trace.BehaviorId.BehaviorClass.Environment}\\{trace.BehaviorId.Type}\\{trace.BehaviorId.Name.ToValidFileName()}\\{trace.BehaviorId.Instance.ToValidFileName()}";
                    Directory.CreateDirectory(directory);
                    await File.WriteAllTextAsync($"{directory}\\{trace.ExecutedAt:yyyy-MM-dd HH_mm_ss_fff}-{trace.Event!.Name.ToValidFileName()}.trace.json", data);
                }
            }
        }
    }
}
