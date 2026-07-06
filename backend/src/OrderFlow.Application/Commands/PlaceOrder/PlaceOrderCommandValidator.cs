using FluentValidation;

namespace OrderFlow.Application.Commands.PlaceOrder;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("A valid user ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

            item.RuleFor(i => i.Quantity)
                .InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
        });
    }
}
