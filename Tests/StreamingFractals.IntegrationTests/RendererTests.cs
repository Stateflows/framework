using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stateflows.Examples.StreamingFractals;

namespace StreamingFractals.IntegrationTests;

[TestClass]
public sealed class RendererTests
{
    public TestContext TestContext { get; set; } = null!;
    private static TileRequest Tile => new(1, 0, -0.745, 0.186, 768, 512, 320, 192, 128, 128, 1);

    internal static void RequireCuda()
    {
        if (Environment.GetEnvironmentVariable("STREAMING_FRACTALS_CUDA_TESTS") != "1")
            Assert.Inconclusive("Set STREAMING_FRACTALS_CUDA_TESTS=1 to require actual CUDA hardware (no fallback).");
    }

    private static CudaJuliaTileRenderer CreateCuda(int streams = 2)
        => new(new RenderingOptions { Backend = "Cuda", CudaStreams = streams },
            NullLogger<CudaJuliaTileRenderer>.Instance);

    [TestMethod]
    public async Task ConfigurationDefaultsToCudaSupportsExplicitCpuAndRejectsUnsupportedSelections()
    {
        Assert.AreEqual("Cuda", new RenderingOptions().Backend);
        var renderer = RenderingOptions.Create(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Rendering:Backend"] = "Cpu" }).Build(), NullLoggerFactory.Instance);
        Assert.IsInstanceOfType<CpuJuliaTileRenderer>(renderer);
        CollectionAssert.AreEqual(JuliaRenderer.Render(Tile, 64, CancellationToken.None),
            await renderer.RenderAsync(Tile, 64, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() =>
            renderer.RenderAsync(Tile, 64, new CancellationToken(true)));
        foreach (var backend in new[] { "", "Gpu", "OpenCL", "Automatic", "1" })
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["Rendering:Backend"] = backend }).Build();
            var error = Assert.ThrowsExactly<InvalidOperationException>(() =>
                RenderingOptions.Create(configuration, NullLoggerFactory.Instance));
            StringAssert.Contains(error.Message, "Rendering:Backend");
        }
        Assert.ThrowsExactly<InvalidOperationException>(() => new RenderingOptions { CudaStreams = 0 }.Validate());
        Assert.ThrowsExactly<InvalidOperationException>(() => new RenderingOptions { CudaStreams = 5 }.Validate());
        Assert.ThrowsExactly<InvalidOperationException>(() => new RenderingOptions { CudaDeviceIndex = -1 }.Validate());
    }

    [TestMethod]
    public void MissingCudaDeviceFailsExplicitlyWithoutCpuFallback()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Rendering:Backend"] = "cUdA",
                ["Rendering:CudaDeviceIndex"] = int.MaxValue.ToString()
            }).Build();
        var error = Assert.ThrowsExactly<InvalidOperationException>(() =>
            RenderingOptions.Create(configuration, NullLoggerFactory.Instance));
        StringAssert.Contains(error.Message, "CPU fallback is disabled");
        Assert.IsNotNull(error.InnerException);
    }

    [TestMethod]
    [Timeout(90_000)]
    public async Task CudaMatchesCpuForDoublePrecisionGeometryAndConcurrentPartitions()
    {
        RequireCuda();
        await using var renderer = CreateCuda();
        TestContext.WriteLine($"CUDA device: {renderer.DeviceName}");
        var tiles = new[]
        {
            Tile with { Real = 0, Imaginary = 0, ImageWidth = 32, ImageHeight = 32, X = 0, Y = 0, Width = 32, Height = 32 },
            Tile,
            Tile with { Real = -0.8, Imaginary = 0.156 },
            Tile with { Real = 0.285, Imaginary = 0.01, X = 1, Y = 3, Width = 127, Height = 119 },
            Tile with { ImageWidth = 719, ImageHeight = 473, X = 351, Y = 213, Width = 127, Height = 119 },
            Tile with { Real = -1.5, Imaginary = 1.5, X = 767, Y = 511, Width = 1, Height = 1 },
            Tile with { ImageWidth = 960, ImageHeight = 640, X = 0, Y = 0, Width = 960, Height = 640, Step = 8 }
        };
        foreach (var iterations in new[] { 32, 64, 256, 1024 })
        foreach (var step in new[] { 1, 2, 4, 8 })
        {
            var requests = tiles.Select(tile => tile with { Step = Math.Max(tile.Step, step) }).ToArray();
            var results = await Task.WhenAll(requests.Select(tile =>
                renderer.RenderAsync(tile, iterations, CancellationToken.None)));
            for (var index = 0; index < requests.Length; index++)
                CollectionAssert.AreEqual(JuliaRenderer.Render(requests[index], iterations, CancellationToken.None),
                    results[index], $"Iterations={iterations}, tile={requests[index]}");
        }
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
            renderer.RenderAsync(Tile with { Width = 768, X = 0, Height = 512, Y = 0 }, 256, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
            renderer.RenderAsync(Tile, 1025, CancellationToken.None));
    }

    [TestMethod]
    [Timeout(90_000)]
    public async Task CudaCancellationAndShutdownDrainBeforeReusingOrDisposingBuffers()
    {
        RequireCuda();
        var renderer = CreateCuda(1);
        try
        {
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                renderer.RenderAsync(Tile, 1024, new CancellationToken(true)));
            using var cancel = new CancellationTokenSource();
            var requests = Enumerable.Range(0, 64).Select(_ =>
                renderer.RenderAsync(Tile with { Real = 0, Imaginary = 0 }, 1024, cancel.Token)).ToArray();
            cancel.Cancel();
            var cancelled = 0;
            foreach (var request in requests)
            {
                try { await request; }
                catch (OperationCanceledException) { cancelled++; }
            }
            Assert.IsGreaterThan(0, cancelled, "Queued work must observe cancellation.");
            CollectionAssert.AreEqual(JuliaRenderer.Render(Tile, 256, CancellationToken.None),
                await renderer.RenderAsync(Tile, 256, CancellationToken.None));

            var pending = Enumerable.Range(0, 64).Select(_ =>
                renderer.RenderAsync(Tile, 1024, CancellationToken.None)).ToArray();
            var disposing = renderer.DisposeAsync().AsTask();
            foreach (var request in pending)
            {
                try { await request; }
                catch (OperationCanceledException) { }
            }
            await disposing;
            await renderer.DisposeAsync();
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() =>
                renderer.RenderAsync(Tile, 256, CancellationToken.None));
        }
        finally { await renderer.DisposeAsync(); }
    }

    [TestMethod]
    [TestCategory("Benchmark")]
    [Timeout(90_000)]
    public async Task WarmTileBenchmarkIncludesSchedulingReadbackAndLittleEndianConversion()
    {
        RequireCuda();
        if (Environment.GetEnvironmentVariable("STREAMING_FRACTALS_BENCHMARK") != "1")
            Assert.Inconclusive("Set STREAMING_FRACTALS_BENCHMARK=1 for the bounded warm-tile experiment.");
        await using var cuda = CreateCuda();
        var cpu = new CpuJuliaTileRenderer();
        TestContext.WriteLine($"Warm sequential tiles; {cuda.DeviceName}; Release={ !IsDebugBuild() }; 8 warmups, 25 samples/backend.");
        foreach (var (name, tile, iterations) in new[]
        {
            ("boundary", Tile, 256),
            ("interior", Tile with { Real = 0, Imaginary = 0 }, 1024),
            ("exterior", Tile with { Real = -1.5, Imaginary = 1.5, X = 0, Y = 0 }, 256),
            ("step8", Tile with { X = 0, Y = 0, Width = 768, Height = 512, Step = 8 }, 256)
        })
        {
            var cpuTimes = await Measure(cpu, tile, iterations);
            var cudaTimes = await Measure(cuda, tile, iterations);
            CollectionAssert.AreEqual(await cpu.RenderAsync(tile, iterations, CancellationToken.None),
                await cuda.RenderAsync(tile, iterations, CancellationToken.None));
            TestContext.WriteLine($"{name} iterations={iterations} geometry={tile.Width}x{tile.Height}/step{tile.Step}: " +
                $"CPU median={cpuTimes[12]:F3}ms p95={cpuTimes[23]:F3}ms; " +
                $"CUDA median={cudaTimes[12]:F3}ms p95={cudaTimes[23]:F3}ms; median ratio CPU/CUDA={cpuTimes[12] / cudaTimes[12]:F2}x");
        }
    }

    private static async Task<double[]> Measure(IJuliaTileRenderer renderer, TileRequest tile, int iterations)
    {
        for (var index = 0; index < 8; index++)
            await renderer.RenderAsync(tile, iterations, CancellationToken.None);
        var samples = new double[25];
        for (var index = 0; index < samples.Length; index++)
        {
            var started = Stopwatch.GetTimestamp();
            await renderer.RenderAsync(tile, iterations, CancellationToken.None);
            samples[index] = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        }
        Array.Sort(samples);
        return samples;
    }

    private static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
