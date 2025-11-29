using Enterprise.Api.Server.DTOs;
using FluentValidation;

namespace Enterprise.Api.Server.Validators;

/// <summary>
/// Server API katmanı - CreateCustomerApiRequest validator
/// API seviyesinde input validation
/// </summary>
public class CreateCustomerApiRequestValidator : AbstractValidator<CreateCustomerApiRequest>
{
    public CreateCustomerApiRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur")
            .MaximumLength(100).WithMessage("Ad alanı en fazla 100 karakter olabilir")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad alanı sadece harf içerebilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur")
            .MaximumLength(100).WithMessage("Soyad alanı en fazla 100 karakter olabilir")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Soyad alanı sadece harf içerebilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı zorunludur")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(256).WithMessage("Email alanı en fazla 256 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir")
            .Matches(@"^\+?[0-9\s\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Geçerli bir telefon numarası giriniz");
    }
}

/// <summary>
/// Update Customer API Request validator
/// </summary>
public class UpdateCustomerApiRequestValidator : AbstractValidator<UpdateCustomerApiRequest>
{
    public UpdateCustomerApiRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur")
            .MaximumLength(100).WithMessage("Ad alanı en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur")
            .MaximumLength(100).WithMessage("Soyad alanı en fazla 100 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

