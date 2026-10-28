using FluentValidation;
using InventoryApi.DTOs;

namespace InventoryApi.Validators;

    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequestDto>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("A valid Customer ID is required.");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("An order must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(items =>
            {
                items.RuleFor(i => i.ProductId)
                    .GreaterThan(0)
                    .WithMessage("Product ID must be valid.");

                items.RuleFor(i => i.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be at least 1.");
            });
        }
    }

