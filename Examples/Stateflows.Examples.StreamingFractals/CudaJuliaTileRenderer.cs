using System.Buffers.Binary;
using System.Collections.Concurrent;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;

namespace Stateflows.Examples.StreamingFractals;

public sealed class CudaJuliaTileRenderer : IJuliaTileRenderer, IAsyncDisposable
{
    public const int MaximumSamples = 16384;
    private static readonly TileRequestValidator Validator = new();
    private readonly Context context;
    private readonly CudaAccelerator accelerator;
    private readonly Action<AcceleratorStream, Index1D, ArrayView1D<int, Stride1D.Dense>, KernelTile> kernel;
    private readonly List<Slot> resources = [];
    private readonly ConcurrentQueue<Slot> available = new();
    private readonly SemaphoreSlim capacity;
    private readonly CancellationTokenSource shutdown = new();
    private readonly object lifecycle = new();
    private readonly TaskCompletionSource drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int operations;
    private bool stopping;
    private Task? disposal;

    public string DeviceName => accelerator.Name;

    public CudaJuliaTileRenderer(RenderingOptions options, ILogger<CudaJuliaTileRenderer> logger)
    {
        options.Validate();
        capacity = new SemaphoreSlim(options.CudaStreams, options.CudaStreams);
        Context? createdContext = null;
        CudaAccelerator? createdAccelerator = null;
        try
        {
            // Default math retains double precision; Multiply below also prevents CUDA JIT fusion.
            createdContext = Context.Create(builder => builder.Cuda().Math(MathMode.Default));
            if (options.CudaDeviceIndex >= createdContext.GetCudaDevices().Count)
                throw new InvalidOperationException($"CUDA device index {options.CudaDeviceIndex} is unavailable.");
            createdAccelerator = createdContext.CreateCudaAccelerator(
                options.CudaDeviceIndex, CudaAcceleratorFlags.ScheduleBlockingSync);
            context = createdContext;
            accelerator = createdAccelerator;
            kernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView1D<int, Stride1D.Dense>, KernelTile>(RenderKernel);
            for (var index = 0; index < options.CudaStreams; index++)
            {
                var slot = new Slot(accelerator);
                resources.Add(slot);
                available.Enqueue(slot);
            }
            logger.LogInformation("Julia rendering backend: Cuda; device {Index}: {Device}; {Streams} reusable streams",
                options.CudaDeviceIndex, DeviceName, options.CudaStreams);
        }
        catch (Exception exception)
        {
            try
            {
                DisposeAll([.. resources, createdAccelerator, createdContext, capacity, shutdown]);
            }
            catch (Exception cleanupException)
            {
                exception = new AggregateException(exception, cleanupException);
            }
            throw new InvalidOperationException(
                "CUDA rendering initialization failed. Check Rendering:CudaDeviceIndex, the NVIDIA driver, " +
                "and available GPU memory. CPU fallback is disabled; select Rendering:Backend=Cpu explicitly.", exception);
        }
    }

    public async Task<byte[]> RenderAsync(TileRequest tile, int maxIterations, CancellationToken cancellationToken)
    {
        if (!Validator.Validate(tile).IsValid || maxIterations is < 32 or > 1024)
            throw new ArgumentOutOfRangeException(nameof(tile), "Invalid tile geometry, parameters, or iteration limit.");
        lock (lifecycle)
        {
            ObjectDisposedException.ThrowIf(stopping, this);
            operations++;
        }
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, shutdown.Token);
            await capacity.WaitAsync(linked.Token).ConfigureAwait(false);
            try
            {
                linked.Token.ThrowIfCancellationRequested();
                if (!available.TryDequeue(out var slot))
                    throw new InvalidOperationException("CUDA buffer pool is inconsistent.");
                try
                {
                    // Only admitted work occupies a thread-pool thread; queued callers await asynchronously.
                    return await Task.Run(() => Render(slot, tile, maxIterations, linked.Token),
                        linked.Token).ConfigureAwait(false);
                }
                finally { available.Enqueue(slot); }
            }
            finally { capacity.Release(); }
        }
        finally
        {
            lock (lifecycle)
            {
                if (--operations == 0 && stopping) drained.TrySetResult();
            }
        }
    }

    private byte[] Render(Slot slot, TileRequest tile, int maxIterations, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var parameters = new KernelTile(tile, maxIterations);
        var count = parameters.Columns * parameters.Rows;
        try
        {
            kernel(slot.Stream, count, slot.Buffer.View, parameters);
            slot.Buffer.View.SubView(0, count).CopyToCPU(slot.Stream, ref slot.Readback[0], count);
        }
        finally
        {
            // A launched kernel cannot be cancelled. Drain before reusing or disposing its resources.
            slot.Stream.Synchronize();
        }
        cancellationToken.ThrowIfCancellationRequested();
        var bytes = new byte[count * sizeof(ushort)];
        for (var index = 0; index < count; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), (ushort)slot.Readback[index]);
        cancellationToken.ThrowIfCancellationRequested();
        return bytes;
    }

    public ValueTask DisposeAsync()
    {
        lock (lifecycle)
        {
            if (disposal is not null) return new ValueTask(disposal);
            stopping = true;
            if (operations == 0) drained.TrySetResult();
            // Native teardown may block; do not run it on the caller's synchronization context.
            disposal = Task.Run(DisposeCoreAsync);
            return new ValueTask(disposal);
        }
    }

    private async Task DisposeCoreAsync()
    {
        shutdown.Cancel();
        await drained.Task.ConfigureAwait(false);
        DisposeAll([.. resources, accelerator, context, capacity, shutdown]);
    }

    private static void DisposeAll(IEnumerable<IDisposable?> items)
    {
        List<Exception>? errors = null;
        foreach (var item in items)
        {
            try { item?.Dispose(); }
            catch (Exception exception)
            {
                (errors ??= []).Add(exception);
            }
        }
        if (errors is not null) throw new AggregateException("CUDA resource cleanup failed.", errors);
    }

    private sealed class Slot : IDisposable
    {
        public AcceleratorStream Stream { get; }
        public MemoryBuffer1D<int, Stride1D.Dense> Buffer { get; }
        public int[] Readback { get; } = new int[MaximumSamples];

        public Slot(CudaAccelerator accelerator)
        {
            Stream = accelerator.CreateStream();
            try { Buffer = accelerator.Allocate1D<int>(MaximumSamples); }
            catch { Stream.Dispose(); throw; }
        }

        public void Dispose()
        {
            try { Stream.Dispose(); }
            finally { Buffer.Dispose(); }
        }
    }

    // ILGPU's generated launcher assembly must be able to access the argument type.
    public readonly struct KernelTile(TileRequest tile, int maxIterations)
    {
        public readonly int Columns = (tile.Width + tile.Step - 1) / tile.Step;
        public readonly int Rows = (tile.Height + tile.Step - 1) / tile.Step;
        public readonly int X = tile.X, Y = tile.Y, Step = tile.Step;
        public readonly int ImageWidth = tile.ImageWidth, ImageHeight = tile.ImageHeight;
        public readonly double Real = tile.Real, Imaginary = tile.Imaginary;
        public readonly int MaxIterations = maxIterations;
    }

    private static void RenderKernel(Index1D index, ArrayView1D<int, Stride1D.Dense> output, KernelTile tile)
    {
        var column = index.X % tile.Columns;
        var row = index.X / tile.Columns;
        var aspect = (double)tile.ImageWidth / tile.ImageHeight;
        var real = Multiply(Multiply(Multiply(
            (tile.X + column * tile.Step + 0.5) / tile.ImageWidth, 2) - 1, 1.3), aspect);
        var imaginary = Multiply(1 - Multiply(
            (tile.Y + row * tile.Step + 0.5) / tile.ImageHeight, 2), 1.3);
        var iteration = 0;
        while (Multiply(real, real) + Multiply(imaginary, imaginary) <= 4 && iteration < tile.MaxIterations)
        {
            var nextReal = Multiply(real, real) - Multiply(imaginary, imaginary) + tile.Real;
            imaginary = Multiply(Multiply(2, real), imaginary) + tile.Imaginary;
            real = nextReal;
            iteration++;
        }
        output[index] = iteration;
    }

    private static double Multiply(double left, double right)
    {
        // ILGPU 1.5.3 emits unqualified mul.f64, which the driver may fuse with an add.
        // Explicit rounding forbids that contraction and preserves the CPU reference's operation order.
        CudaAsm.Emit("mul.rn.f64 %0, %1, %2;", out double result, left, right);
        return result;
    }
}
