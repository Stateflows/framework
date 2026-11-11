using System.Buffers.Binary;

namespace Stateflows.Examples.StreamingFractals;

public static class JuliaRenderer
{
    public static byte[] Render(TileRequest tile, int maxIterations, CancellationToken cancellationToken)
    {
        var columns = (tile.Width + tile.Step - 1) / tile.Step;
        var rows = (tile.Height + tile.Step - 1) / tile.Step;
        var bytes = new byte[columns * rows * sizeof(ushort)];
        var aspect = (double)tile.ImageWidth / tile.ImageHeight;
        for (var row = 0; row < rows; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var column = 0; column < columns; column++)
            {
                var real = ((tile.X + column * tile.Step + 0.5) / tile.ImageWidth * 2 - 1) * 1.3 * aspect;
                var imaginary = (1 - (tile.Y + row * tile.Step + 0.5) / tile.ImageHeight * 2) * 1.3;
                ushort iteration = 0;
                while (real * real + imaginary * imaginary <= 4 && iteration < maxIterations)
                {
                    var nextReal = real * real - imaginary * imaginary + tile.Real;
                    imaginary = 2 * real * imaginary + tile.Imaginary;
                    real = nextReal;
                    iteration++;
                }
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan((row * columns + column) * 2), iteration);
            }
        }
        return bytes;
    }
}
