using FluentValidation;
using Stateflows.Common;

namespace Stateflows.Examples.StreamingFractals;

public sealed class ConfigureRenderingValidator : AbstractValidator<ConfigureRendering>
{
    public ConfigureRenderingValidator()
        => RuleFor(settings => settings.MaxIterations).InclusiveBetween(32, 1024);
}

public sealed class TileRequestValidator : AbstractValidator<TileRequest>
{
    public TileRequestValidator()
    {
        RuleFor(tile => tile.Generation).GreaterThan(0);
        RuleFor(tile => tile.TileId).GreaterThanOrEqualTo(0);
        RuleFor(tile => tile.Real).Must(double.IsFinite).InclusiveBetween(-1.5, 1.5);
        RuleFor(tile => tile.Imaginary).Must(double.IsFinite).InclusiveBetween(-1.5, 1.5);
        RuleFor(tile => tile.ImageWidth).InclusiveBetween(32, 960);
        RuleFor(tile => tile.ImageHeight).InclusiveBetween(32, 640);
        RuleFor(tile => tile.X).GreaterThanOrEqualTo(0);
        RuleFor(tile => tile.Y).GreaterThanOrEqualTo(0);
        RuleFor(tile => tile.Width).InclusiveBetween(1, 960);
        RuleFor(tile => tile.Height).InclusiveBetween(1, 640);
        RuleFor(tile => tile.Step).Must(step => step is 1 or 2 or 4 or 8);
        RuleFor(tile => tile).Must(tile =>
                (long)tile.X + tile.Width <= tile.ImageWidth &&
                (long)tile.Y + tile.Height <= tile.ImageHeight)
            .WithMessage("The tile must fit inside the image.");
        RuleFor(tile => tile).Must(tile => tile.Step <= 0 ||
                ((long)tile.Width + tile.Step - 1) / tile.Step *
                (((long)tile.Height + tile.Step - 1) / tile.Step) <= 16384)
            .WithMessage("A tile may contain at most 16384 samples.");
    }
}

public sealed class TileBatchValidator : AbstractValidator<TokensInput<TileRequest>>
{
    public TileBatchValidator()
    {
        RuleFor(input => input.Tokens).NotNull().Must(tokens => tokens is { Count: 1 })
            .WithMessage("Submit exactly one tile per admission.");
        var validator = new TileRequestValidator();
        RuleForEach(input => input.Tokens).Custom((holder, context) =>
        {
            if (holder?.BoxedPayload is not TileRequest tile)
            {
                context.AddFailure("A non-null TileRequest payload is required.");
                return;
            }
            foreach (var error in validator.Validate(tile).Errors)
                context.AddFailure(error);
        });
    }
}
