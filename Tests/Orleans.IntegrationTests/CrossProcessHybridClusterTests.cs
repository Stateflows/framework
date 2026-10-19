using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class CrossProcessHybridClusterTests
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan DiscoveryTimeout = TimeSpan.FromSeconds(30);

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task RemoteBehaviorsExecuteOnTheirOwningProcessesInBothDirections()
    {
        var ports = GetFreeTcpPorts(6);
        var clusterId = $"orleans-cross-process-{Guid.NewGuid():N}";
        var serviceId = $"stateflows-cross-process-{Guid.NewGuid():N}";
        var siloA = ChildSilo.Start(
            "silo-a",
            ports[0],
            ports[1],
            ports[2],
            primarySiloPort: null,
            clusterId,
            serviceId);
        ChildSilo? siloB = null;

        try
        {
            siloB = ChildSilo.Start(
                "silo-b",
                ports[3],
                ports[4],
                ports[5],
                ports[0],
                clusterId,
                serviceId);

            using var clientA = siloA.CreateClient();
            using var clientB = siloB.CreateClient();
            var readyA = await WaitForReadinessAsync(clientA, siloA);
            var readyB = await WaitForReadinessAsync(clientB, siloB);

            Assert.AreNotEqual(Environment.ProcessId, readyA.ProcessId, "Silo A must run in a child process.");
            Assert.AreNotEqual(Environment.ProcessId, readyB.ProcessId, "Silo B must run in a child process.");
            Assert.AreNotEqual(readyA.ProcessId, readyB.ProcessId, "Each silo must run in a distinct process.");
            Assert.AreEqual("silo-a", readyA.Node);
            Assert.AreEqual("silo-b", readyB.Node);
            TestContext.WriteLine(
                $"Cross-process silos: silo-a PID {readyA.ProcessId}, silo-b PID {readyB.ProcessId}, test PID {Environment.ProcessId}.");

            await WaitForDiscoveryAsync(clientA, "silo-b", siloA);
            await WaitForDiscoveryAsync(clientB, "silo-a", siloB);
            Assert.IsFalse(await IsDiscoveredAsync(clientA, "not-registered"));
            Assert.IsFalse(await IsDiscoveredAsync(clientB, "not-registered"));

            var executionOnB = await ExecuteAsync(clientA, "silo-b");
            AssertExecution(executionOnB, readyA.ProcessId, "silo-b");
            var executionOnA = await ExecuteAsync(clientB, "silo-a");
            AssertExecution(executionOnA, readyB.ProcessId, "silo-a");

            var effectsA = await GetEffectsAsync(clientA);
            var effectsB = await GetEffectsAsync(clientB);
            Assert.AreEqual(readyA.ProcessId, effectsA.ProcessId);
            Assert.AreEqual(readyB.ProcessId, effectsB.ProcessId);
            AssertEffect(effectsB, "silo-b-action", readyB.ProcessId);
            AssertEffect(effectsB, "silo-b-activity", readyB.ProcessId);
            AssertEffect(effectsB, "silo-b-machine", readyB.ProcessId);
            AssertEffect(effectsA, "silo-a-action", readyA.ProcessId);
            AssertEffect(effectsA, "silo-a-activity", readyA.ProcessId);
            AssertEffect(effectsA, "silo-a-machine", readyA.ProcessId);
            Assert.IsFalse(effectsA.Effects.ContainsKey("silo-b-action"));
            Assert.IsFalse(effectsA.Effects.ContainsKey("silo-b-activity"));
            Assert.IsFalse(effectsA.Effects.ContainsKey("silo-b-machine"));
            Assert.IsFalse(effectsB.Effects.ContainsKey("silo-a-action"));
            Assert.IsFalse(effectsB.Effects.ContainsKey("silo-a-activity"));
            Assert.IsFalse(effectsB.Effects.ContainsKey("silo-a-machine"));

            using var missingBehavior = await clientA.PostAsync("/execute/not-registered", content: null);
            Assert.AreEqual(HttpStatusCode.NotFound, missingBehavior.StatusCode);
            effectsA = await GetEffectsAsync(clientA);
            Assert.AreEqual(3, effectsA.Effects.Count);
        }
        finally
        {
            try
            {
                if (siloB is not null)
                {
                    await siloB.StopAsync();
                }
            }
            finally
            {
                await siloA.StopAsync();
            }
        }
    }

    private static void AssertExecution(ExecutionResponse response, int sourceProcessId, string target)
    {
        Assert.AreEqual(sourceProcessId, response.SourceProcessId);
        Assert.AreEqual(target, response.Target);
        Assert.AreEqual("Initialized", response.ActivityStatus);
        Assert.AreEqual("Initialized", response.InitializationStatus);
        Assert.AreEqual("ready", response.InitialState);
        Assert.AreEqual("Consumed", response.TransitionStatus);
        Assert.AreEqual("complete", response.CurrentState);
    }

    private static void AssertEffect(EffectsResponse response, string name, int expectedProcessId)
    {
        Assert.IsTrue(response.Effects.TryGetValue(name, out var effect), $"Expected execution effect '{name}'.");
        Assert.AreEqual(expectedProcessId, effect.ProcessId, $"Effect '{name}' ran in the wrong process.");
        Assert.AreEqual(1, effect.Count, $"Effect '{name}' should execute exactly once.");
    }

    private static async Task<ExecutionResponse> ExecuteAsync(HttpClient client, string target)
    {
        using var response = await client.PostAsync($"/execute/{target}", content: null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionResponse>())
            ?? throw new InvalidOperationException("The silo returned an empty execution response.");
    }

    private static async Task<EffectsResponse> GetEffectsAsync(HttpClient client)
        => (await client.GetFromJsonAsync<EffectsResponse>("/effects"))
            ?? throw new InvalidOperationException("The silo returned an empty effects response.");

    private static async Task<bool> IsDiscoveredAsync(HttpClient client, string target)
        => (await client.GetFromJsonAsync<DiscoveryResponse>($"/discovery/{target}"))?.Discovered == true;

    private static async Task<ReadyResponse> WaitForReadinessAsync(HttpClient client, ChildSilo silo)
    {
        var timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < StartupTimeout)
        {
            silo.ThrowIfExited("starting");
            try
            {
                var ready = await client.GetFromJsonAsync<ReadyResponse>("/ready");
                if (ready is not null)
                {
                    return ready;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        Assert.Fail($"Child silo '{silo.Node}' did not become ready within {StartupTimeout}.{Environment.NewLine}{silo.GetLogTail()}");
        throw new InvalidOperationException("Unreachable after test failure.");
    }

    private static async Task WaitForDiscoveryAsync(HttpClient client, string target, ChildSilo silo)
    {
        var timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < DiscoveryTimeout)
        {
            silo.ThrowIfExited("discovering behavior metadata");
            try
            {
                if (await IsDiscoveredAsync(client, target))
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        Assert.Fail(
            $"Silo '{silo.Node}' did not discover '{target}' within {DiscoveryTimeout}.{Environment.NewLine}{silo.GetLogTail()}");
    }

    private static int[] GetFreeTcpPorts(int count)
    {
        var listeners = new List<TcpListener>(count);
        try
        {
            for (var index = 0; index < count; index++)
            {
                var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                listeners.Add(listener);
            }

            return listeners
                .Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port)
                .ToArray();
        }
        finally
        {
            foreach (var listener in listeners)
            {
                listener.Stop();
            }
        }
    }

    private sealed record ReadyResponse(int ProcessId, string Node);
    private sealed record DiscoveryResponse(bool Discovered);
    private sealed record ExecutionResponse(
        int SourceProcessId,
        string Target,
        string ActivityStatus,
        string InitializationStatus,
        string InitialState,
        string TransitionStatus,
        string CurrentState);
    private sealed record ExecutionEffect(int ProcessId, int Count);
    private sealed record EffectsResponse(int ProcessId, IReadOnlyDictionary<string, ExecutionEffect> Effects);

    private sealed class ChildSilo
    {
        private const int MaximumLogLines = 100;
        private readonly Process process;
        private readonly ConcurrentQueue<string> logLines = new();
        private readonly Task stdoutDrain;
        private readonly Task stderrDrain;

        private ChildSilo(string node, int httpPort, Process process)
        {
            Node = node;
            HttpPort = httpPort;
            this.process = process;
            stdoutDrain = DrainAsync(process.StandardOutput, "stdout");
            stderrDrain = DrainAsync(process.StandardError, "stderr");
        }

        public string Node { get; }
        public int HttpPort { get; }

        public static ChildSilo Start(
            string node,
            int siloPort,
            int gatewayPort,
            int httpPort,
            int? primarySiloPort,
            string clusterId,
            string serviceId)
        {
            var helperPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "SiloHost",
                "Orleans.IntegrationTests.SiloHost.dll"));
            if (!File.Exists(helperPath))
            {
                throw new FileNotFoundException("The cross-process Orleans silo helper was not built.", helperPath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = GetDotNetHostPath(),
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(helperPath);
            AddArgument(startInfo, "node", node);
            AddArgument(startInfo, "silo-port", siloPort.ToString());
            AddArgument(startInfo, "gateway-port", gatewayPort.ToString());
            AddArgument(startInfo, "http-port", httpPort.ToString());
            if (primarySiloPort is int primaryPort)
            {
                AddArgument(startInfo, "primary-silo-port", primaryPort.ToString());
            }

            AddArgument(startInfo, "cluster-id", clusterId);
            AddArgument(startInfo, "service-id", serviceId);

            var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"Could not start child silo '{node}'.");
            return new ChildSilo(node, httpPort, process);
        }

        public HttpClient CreateClient()
            => new()
            {
                BaseAddress = new Uri($"http://127.0.0.1:{HttpPort}"),
                Timeout = TimeSpan.FromSeconds(5)
            };

        public void ThrowIfExited(string phase)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Child silo '{Node}' exited with code {process.ExitCode} while {phase}.{Environment.NewLine}{GetLogTail()}");
            }
        }

        public string GetLogTail()
            => string.Join(Environment.NewLine, logLines.ToArray());

        public async Task StopAsync()
        {
            if (process.HasExited)
            {
                await DrainLogsAsync();
                process.Dispose();
                return;
            }

            using (var client = CreateClient())
            {
                try
                {
                    using var response = await client.PostAsync("/shutdown", content: null);
                }
                catch (HttpRequestException)
                {
                }
                catch (TaskCanceledException)
                {
                }
            }

            try
            {
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }

                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            }
            finally
            {
                await DrainLogsAsync();
                process.Dispose();
            }
        }

        private async Task DrainAsync(StreamReader reader, string stream)
        {
            while (await reader.ReadLineAsync() is { } line)
            {
                var message = $"{stream}: {line}";
                if (message.Length > 2000)
                {
                    message = message[..2000];
                }

                logLines.Enqueue(message);
                while (logLines.Count > MaximumLogLines)
                {
                    logLines.TryDequeue(out _);
                }
            }
        }

        private async Task DrainLogsAsync()
        {
            await Task.WhenAll(stdoutDrain, stderrDrain).WaitAsync(TimeSpan.FromSeconds(5));
        }

        private static string GetDotNetHostPath()
        {
            var configuredHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
            if (!string.IsNullOrWhiteSpace(configuredHost) && File.Exists(configuredHost))
            {
                return configuredHost;
            }

            var runtimeDirectory = new DirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory());
            var dotNetRoot = runtimeDirectory.Parent?.Parent?.Parent
                ?? throw new InvalidOperationException($"Could not locate the .NET host from '{runtimeDirectory.FullName}'.");
            var hostPath = Path.Combine(dotNetRoot.FullName, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
            return File.Exists(hostPath)
                ? hostPath
                : throw new FileNotFoundException("Could not locate the .NET host used by the test process.", hostPath);
        }

        private static void AddArgument(ProcessStartInfo startInfo, string name, string value)
        {
            startInfo.ArgumentList.Add($"--{name}");
            startInfo.ArgumentList.Add(value);
        }
    }
}
