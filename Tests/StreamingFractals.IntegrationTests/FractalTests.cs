using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stateflows.Examples.StreamingFractals;
using Stateflows.Activities;

namespace StreamingFractals.IntegrationTests;

[TestClass]
public sealed class FractalTests
{
    private static TileRequest Tile => new(1, 0, 0, 0, 32, 32, 0, 0, 32, 32, 1);

    private sealed class CompressionInput : List<TileResult>, IInputTokens<TileResult> { }
    private sealed class CompressionOutput : List<TileResult>, IOutputTokens<TileResult> { }

    [TestMethod]
    public async Task CompressionPreservesSamplesMetadataAndUsesRawWhenSmaller()
    {
        var input = new CompressionInput();
        var output = new CompressionOutput();
        var bytes = JuliaRenderer.Render(Tile, 256, CancellationToken.None);
        var compressible = new TileResult(1, 0, 0, 0, 32, 32, 1, 32, 32, 256,
            Convert.ToBase64String(bytes), 1.5, "Streamed");
        var tiny = compressible with { TileId = 1, Width = 1, Height = 1, Columns = 1, Rows = 1, Iterations = "AQA=" };
        input.AddRange([compressible, tiny]);
        var action = new CompressJuliaTileAction(input, output);
        await action.ExecuteAsync(CancellationToken.None);
        Assert.AreEqual("gzip", output[0].Encoding);
        CollectionAssert.AreEqual(bytes, DecodeIterations(output[0]));
        Assert.AreEqual(compressible, output[0] with { Iterations = compressible.Iterations, Encoding = "raw" });
        Assert.IsTrue(output[0].Iterations.Length < compressible.Iterations.Length);
        Assert.AreEqual(tiny, output[1]);
        output.Clear();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => action.ExecuteAsync(new CancellationToken(true)));
        Assert.HasCount(0, output);
    }

    private static byte[] DecodeIterations(TileResult tile)
    {
        var bytes = Convert.FromBase64String(tile.Iterations);
        if (tile.Encoding == "raw") return bytes;
        Assert.AreEqual("gzip", tile.Encoding);
        using var gzip = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
        using var decoded = new MemoryStream();
        gzip.CopyTo(decoded);
        return decoded.ToArray();
    }

    [TestMethod]
    public void JuliaIterationCountsMatchKnownInteriorAndExteriorPoints()
    {
        var bytes = JuliaRenderer.Render(Tile, 256, CancellationToken.None);
        Assert.HasCount(32 * 32 * 2, bytes);
        // For c=0, z=0 remains bounded and the image corner escapes immediately.
        Assert.AreEqual(256, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan((16 * 32 + 16) * 2)));
        Assert.IsTrue(BinaryPrimitives.ReadUInt16LittleEndian(bytes) < 10);
        Assert.ThrowsExactly<OperationCanceledException>(() =>
            JuliaRenderer.Render(Tile, 256, new CancellationToken(true)));
    }

    [TestMethod]
    public void ValidationBoundsGeometryWorkAndNumericInputs()
    {
        var validator = new TileRequestValidator();
        Assert.IsTrue(validator.Validate(Tile).IsValid);
        Assert.IsTrue(validator.Validate(Tile with { ImageWidth = 768, ImageHeight = 512, Width = 768, Height = 512, Step = 8 }).IsValid);
        Assert.IsTrue(validator.Validate(Tile with { Step = 4 }).IsValid);
        Assert.IsTrue(validator.Validate(Tile with { Step = 2 }).IsValid);
        foreach (var invalid in new[]
        {
            Tile with { Real = double.NaN }, Tile with { Imaginary = double.PositiveInfinity },
            Tile with { Real = 2 }, Tile with { Generation = 0 }, Tile with { TileId = -1 },
            Tile with { X = 1 }, Tile with { Y = -1 }, Tile with { Width = 0 },
            Tile with { Step = 0 }, Tile with { Step = 3 }, Tile with { X = int.MaxValue },
            Tile with { ImageWidth = 960, ImageHeight = 640, Width = 960, Height = 640 },
            Tile with { ImageWidth = 961 }, Tile with { ImageHeight = 641 }
        })
            Assert.IsFalse(validator.Validate(invalid).IsValid, invalid.ToString());
        var settings = new ConfigureRenderingValidator();
        Assert.IsFalse(settings.Validate(new ConfigureRendering(1025)).IsValid);
        Assert.IsFalse(settings.Validate(new ConfigureRendering(31)).IsValid);
    }

    [TestMethod]
    [DataRow("Cpu")]
    [DataRow("Cuda")]
    [Timeout(90_000)]
    public async Task HttpAdmissionSseSteeringAndLifecycleWorkTogether(string backend)
    {
        if (backend == "Cuda") RendererTests.RequireCuda();
        await using var factory = CreateFactory(backend);
        using var http = factory.CreateClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var instance = Guid.NewGuid().ToString("N");
        var route = $"/stateflows/activities/Julia/{instance}";
        using var contract = await http.GetFromJsonAsync<JsonDocument>("/api/fractal", timeout.Token);
        Assert.IsNotNull(contract);
        var eventName = contract.RootElement.GetProperty("outputEvent").GetString();
        using var sse = await http.GetAsync($"{route}/output/tileResult?stream=true",
            HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        sse.EnsureSuccessStatusCode();
        Assert.AreEqual("text/event-stream", sse.Content.Headers.ContentType?.MediaType);
        using var reader = new StreamReader(await sse.Content.ReadAsStreamAsync(timeout.Token));

        using var configured = await http.PostAsJsonAsync($"{route}/configureRendering",
            new { @event = new ConfigureRendering(64) }, timeout.Token);
        configured.EnsureSuccessStatusCode();
        using var invalid = await http.PostAsJsonAsync($"{route}/configureRendering",
            new { @event = new ConfigureRendering(0) }, timeout.Token);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);

        foreach (var batch in new[] { Array.Empty<TileRequest>(), new[] { Tile, Tile }, new[] { Tile with { X = 999 } } })
        {
            using var rejected = await http.PostAsJsonAsync($"{route}/input/tileRequest?sendAsStream=true",
                new { tokens = batch }, timeout.Token);
            Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        using var admitted = await http.PostAsJsonAsync($"{route}/input/tileRequest?sendAsStream=true",
            new { tokens = new[] { Tile } }, timeout.Token);
        Assert.AreEqual(HttpStatusCode.Accepted, admitted.StatusCode);
        var output = await ReadTileAsync(reader, eventName!, timeout.Token);
        Assert.AreEqual(1, output.Generation);
        Assert.AreEqual("Streamed", output.ExecutionMode);
        Assert.AreEqual(64, output.MaxIterations);
        Assert.AreEqual("gzip", output.Encoding);
        CollectionAssert.AreEqual(JuliaRenderer.Render(Tile, 64, timeout.Token), DecodeIterations(output));

        var concurrentTiles = Enumerable.Range(10, 8)
            .Select(id => Tile with { TileId = id, Generation = 10, Real = -0.745, Imaginary = 0.186,
                Step = id % 2 == 0 ? 2 : 4 })
            .ToArray();
        await Task.WhenAll(concurrentTiles.Select(async tile =>
        {
            using var response = await http.PostAsJsonAsync($"{route}/input/tileRequest?sendAsStream=true",
                new { tokens = new[] { tile } }, timeout.Token);
            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        }));
        var received = new HashSet<int>();
        foreach (var _ in concurrentTiles)
        {
            var result = await ReadTileAsync(reader, eventName!, timeout.Token);
            Assert.AreEqual(10, result.Generation);
            Assert.AreEqual("Streamed", result.ExecutionMode);
            Assert.IsTrue(received.Add(result.TileId), "Each partition must publish exactly one result.");
            var tile = concurrentTiles.Single(candidate => candidate.TileId == result.TileId);
            CollectionAssert.AreEqual(JuliaRenderer.Render(tile, 64, timeout.Token),
                DecodeIterations(result));
        }

        using var ordinary = await http.PostAsJsonAsync($"{route}/input/tileRequest",
            new { tokens = new[] { Tile with { Generation = 2 } } }, timeout.Token);
        ordinary.EnsureSuccessStatusCode();
        var transactional = await ReadTileAsync(reader, eventName!, timeout.Token);
        Assert.AreEqual(2, transactional.Generation);
        Assert.AreEqual("Transactional", transactional.ExecutionMode);

        var diagram = await http.GetStringAsync($"/api/sessions/{instance}/diagram", timeout.Token);
        StringAssert.Contains(diagram, "CalculateJuliaTileAction");
        StringAssert.Contains(diagram, "CompressJuliaTileAction");
        StringAssert.Contains(diagram, FractalActivity.RenderTiles);
        StringAssert.Contains(diagram, "interleave");
        using var stopped = await http.PostAsync($"/api/sessions/{instance}/stop", null, timeout.Token);
        stopped.EnsureSuccessStatusCode();
        using var afterStop = await http.PostAsJsonAsync($"{route}/input/tileRequest?sendAsStream=true",
            new { tokens = new[] { Tile with { Generation = 3 } } }, timeout.Token);
        Assert.AreEqual(HttpStatusCode.Accepted, afterStop.StatusCode);
        var restarted = await ReadTileAsync(reader, eventName!, timeout.Token);
        Assert.AreEqual(3, restarted.Generation);
        Assert.AreEqual(ConfigureRendering.DefaultIterations, restarted.MaxIterations);
        using var cleaned = await http.PostAsync($"/api/sessions/{instance}/stop", null, timeout.Token);
        cleaned.EnsureSuccessStatusCode();
    }

    private static async Task<TileResult> ReadTileAsync(StreamReader reader, string eventName, CancellationToken cancellationToken)
    {
        var actualEvent = "";
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line.StartsWith("event: ", StringComparison.Ordinal)) actualEvent = line[7..];
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;
            Assert.AreEqual(eventName, actualEvent);
            using var document = JsonDocument.Parse(line[6..]);
            var payload = document.RootElement.GetProperty("payload");
            return payload.Deserialize<TileResult>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? throw new InvalidOperationException("Missing tile payload.");
        }
        throw new EndOfStreamException("Output stream ended before returning a tile.");
    }

    private static WebApplicationFactory<Program> CreateFactory(string backend)
    {
        using var silo = new TcpListener(IPAddress.Loopback, 0);
        using var gateway = new TcpListener(IPAddress.Loopback, 0);
        silo.Start();
        gateway.Start();
        var siloPort = ((IPEndPoint)silo.LocalEndpoint).Port;
        var gatewayPort = ((IPEndPoint)gateway.LocalEndpoint).Port;
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Development")
            .ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Rendering:Backend"] = backend,
                    ["Orleans:SiloPort"] = siloPort.ToString(),
                    ["Orleans:GatewayPort"] = gatewayPort.ToString()
                })));
    }
}
