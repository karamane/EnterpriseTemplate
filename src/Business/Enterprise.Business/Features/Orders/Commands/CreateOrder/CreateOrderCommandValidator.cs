using FluentValidation;

namespace Enterprise.Business.Features.Orders.Commands.CreateOrder;

/// <summary>
/// CreateOrderCommand validator
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("Müşteri ID geçerli olmalıdır");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Sipariş en az bir kalem içermelidir");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemCommandValidator());
    }
}

/// <summary>
/// CreateOrderItemCommand validator
/// </summary>
public class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
{
    public CreateOrderItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Ürün ID geçerli olmalıdır");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Ürün adı boş olamaz")
            .MaximumLength(200)
            .WithMessage("Ürün adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Miktar 0'dan büyük olmalıdır");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Birim fiyat negatif olamaz");
    }
}


