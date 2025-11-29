using Enterprise.Api.Client.DTOs;
using FluentValidation;

namespace Enterprise.Api.Client.Validators;

/// <summary>
/// Client API katmanı - CreateCustomerClientRequest validator
/// Mobil input validation
/// </summary>
public class CreateCustomerClientRequestValidator : AbstractValidator<CreateCustomerClientRequest>
{
    public CreateCustomerClientRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur")
            .MaximumLength(100).WithMessage("Ad çok uzun");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur")
            .MaximumLength(100).WithMessage("Soyad çok uzun");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı zorunludur")
            .EmailAddress().WithMessage("Geçersiz email formatı");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Geçersiz telefon formatı");
    }
}

