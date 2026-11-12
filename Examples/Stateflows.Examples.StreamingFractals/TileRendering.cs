namespace Stateflows.Examples.StreamingFractals;

public interface IJuliaTileRenderer
{
    Task<byte[]> RenderAsync(TileRequest tile, int maxIterations, CancellationToken cancellationToken);
}

public sealed class CpuJuliaTileRenderer : IJuliaTileRenderer
{
    public Task<byte[]> RenderAsync(TileRequest tile, int maxIterations, CancellationToken cancellationToken)
        => Task.Run(() => JuliaRenderer.Render(tile, maxIterations, cancellationToken), cancellationToken);
}

public sealed class RenderingOptions
{
    public string Backend { get; set; } = "Cuda";
    public int CudaDeviceIndex { get; set; }
    public int CudaStreams { get; set; } = 2;

    public void Validate()
    {
        if (!string.Equals(Backend, "Cpu", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Backend, "Cuda", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Rendering:Backend must be Cpu or Cuda (no automatic fallback).");
        if (CudaDeviceIndex < 0)
            throw new InvalidOperationException("Rendering:CudaDeviceIndex must be nonnegative.");
        if (CudaStreams is < 1 or > 4)
            throw new InvalidOperationException("Rendering:CudaStreams must be between 1 and 4.");
    }

    public static IJuliaTileRenderer Create(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var options = configuration.GetSection("Rendering").Get<RenderingOptions>() ?? new();
        options.Validate();
        if (string.Equals(options.Backend, "Cuda", StringComparison.OrdinalIgnoreCase))
            return new CudaJuliaTileRenderer(options, loggerFactory.CreateLogger<CudaJuliaTileRenderer>());
        loggerFactory.CreateLogger<CpuJuliaTileRenderer>().LogInformation("Julia rendering backend: Cpu");
        return new CpuJuliaTileRenderer();
    }
}
