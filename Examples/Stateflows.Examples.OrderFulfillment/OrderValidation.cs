using FluentValidation;

namespace Stateflows.Examples.OrderFulfillment;

public sealed class SubmitOrderValidator : AbstractValidator<SubmitOrder>
{
    public SubmitOrderValidator()
    {
        RuleFor(order => order.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(order => order.Scenario).IsInEnum();
        RuleFor(order => order.CallbackDelayMilliseconds).InclusiveBetween(100, 5000);
        RuleFor(order => order.Lines).NotEmpty().Must(lines => lines is null || lines.Length <= 50)
            .WithMessage("An order may contain at most 50 lines.");
        RuleForEach(order => order.Lines).NotNull().SetValidator(new OrderLineValidator());
        RuleFor(order => order.Lines)
            .Must(lines => lines is null ||
                lines.Where(line => line is not null).Select(line => line.Sku)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() == lines.Length)
            .WithMessage("Each SKU must occur only once.");
    }
}

public sealed class OrderLineValidator : AbstractValidator<OrderLine>
{
    public OrderLineValidator()
    {
        RuleFor(line => line.Sku).NotEmpty().MaximumLength(50);
        RuleFor(line => line.Quantity).InclusiveBetween(1, 1000);
        RuleFor(line => line.UnitPrice).InclusiveBetween(0.01m, 100000m)
            .Must(price => decimal.Round(price, 2) == price)
            .WithMessage("Unit prices may have at most two decimal places.");
    }
}

public sealed class CancelOrderValidator : AbstractValidator<CancelOrder>
{
    public CancelOrderValidator() => RuleFor(command => command.Reason).NotEmpty().MaximumLength(500);
}
