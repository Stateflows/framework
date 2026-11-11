using System.Diagnostics;
using System.IO.Compression;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Entities.Enums;

namespace Stateflows.Examples.StreamingFractals;

public sealed class FractalActivity : IActivity
{
    public const string Name = "Julia";
    public const string SettingsKey = "render-settings";
    public const string RenderTiles = "RenderTiles";

    public static void Build(IActivityBuilder activity) => activity
        .AddDefaultInitializer<FractalInitializer>()
        .AddAcceptEventAction<ConfigureRendering, ConfigureRenderingAction>()
        .AddInputPin<TileRequest>(pin => pin
            .AddFlow(RenderTiles)   
        )
        .AddParallelActivity<TileRequest>(RenderTiles, parallel => parallel
            .Interleave()
            .AddInputPin<TileRequest>(pin => pin.AddFlow<CalculateJuliaTileAction>())
            .AddAction<CalculateJuliaTileAction>(node => node
                .AddFlow<TileResult, CompressJuliaTileAction>()
            )
            .AddAction<CompressJuliaTileAction>(node => node
                .AddFlow<TileResult, OutputPin<TileResult>>()
            ),
            chunkSize: 1,
            maxConcurrency: 4
        )
        .AddOutputPin<TileResult>();
}

public sealed class CompressJuliaTileAction(
    IInputTokens<TileResult> input, IOutputTokens<TileResult> output) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var tile in input)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tile.Encoding != "raw")
                throw new InvalidOperationException($"Cannot compress tile encoding '{tile.Encoding}'.");
            var bytes = Convert.FromBase64String(tile.Iterations);
            using var compressed = new MemoryStream();
            await using (var gzip = new GZipStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
                await gzip.WriteAsync(bytes, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            output.Add(compressed.Length < bytes.Length
                ? tile with { Iterations = Convert.ToBase64String(compressed.ToArray()), Encoding = "gzip" }
                : tile);
        }
    }
}

public sealed class FractalInitializer(IActivityContext activity) : IDefaultInitializer
{
    public async Task<bool> OnInitializeAsync()
    {
        await activity.Values.SetAsync(FractalActivity.SettingsKey,
            new ConfigureRendering(ConfigureRendering.DefaultIterations));
        return true;
    }
}

public sealed class ConfigureRenderingAction(IActivityContext activity) : IAcceptEventActionNode<ConfigureRendering>
{
    public Task ExecuteAsync(ConfigureRendering @event, CancellationToken cancellationToken)
        => activity.Values.SetAsync(FractalActivity.SettingsKey, @event);
}

public sealed class CalculateJuliaTileAction(
    IActivityContext activity, IExecutionContext execution, IInputTokens<TileRequest> input,
    IOutputTokens<TileResult> output, IJuliaTileRenderer renderer) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var tile in input)
        {
            var settings = await activity.Values.GetOrDefaultAsync<ConfigureRendering>(FractalActivity.SettingsKey)
                ?? throw new InvalidOperationException("Rendering settings have not been initialized.");
            var started = Stopwatch.GetTimestamp();
            var bytes = await renderer.RenderAsync(tile, settings.MaxIterations, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            output.Add(new TileResult(tile.Generation, tile.TileId, tile.X, tile.Y,
                tile.Width, tile.Height, tile.Step,
                (tile.Width + tile.Step - 1) / tile.Step, (tile.Height + tile.Step - 1) / tile.Step,
                settings.MaxIterations, Convert.ToBase64String(bytes),
                Stopwatch.GetElapsedTime(started).TotalMilliseconds, execution.ExecutionMode.ToString()));
        }
    }
}
