using FluentValidation;

namespace Enterprise.Business.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// CreateCustomerCommand validator
/// FluentValidation ile input validation örneği
/// </summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş olamaz")
            .MaximumLength(100).WithMessage("Ad alanı en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş olamaz")
            .MaximumLength(100).WithMessage("Soyad alanı en fazla 100 karakter olabilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş olamaz")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(256).WithMessage("Email alanı en fazla 256 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir")
            .Matches(@"^\+?[0-9\s\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Geçerli bir telefon numarası giriniz");
    }
}

