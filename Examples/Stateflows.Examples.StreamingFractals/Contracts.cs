namespace Stateflows.Examples.StreamingFractals;

public sealed record TileRequest(
    long Generation, int TileId, double Real, double Imaginary,
    int ImageWidth, int ImageHeight, int X, int Y, int Width, int Height, int Step);

public sealed record ConfigureRendering(int MaxIterations)
{
    public const int DefaultIterations = 256;
}

public sealed record TileResult(
    long Generation, int TileId, int X, int Y, int Width, int Height, int Step,
    int Columns, int Rows, int MaxIterations, string Iterations,
    double ComputeMilliseconds, string ExecutionMode, string Encoding = "raw");
