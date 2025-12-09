using FluentValidation;

namespace Enterprise.Business.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// UpdateCustomerCommand validator
/// </summary>
public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir müşteri ID'si giriniz");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş olamaz")
            .MaximumLength(100).WithMessage("Ad alanı en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş olamaz")
            .MaximumLength(100).WithMessage("Soyad alanı en fazla 100 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir")
            .Matches(@"^\+?[0-9\s\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Geçerli bir telefon numarası giriniz");
    }
}


