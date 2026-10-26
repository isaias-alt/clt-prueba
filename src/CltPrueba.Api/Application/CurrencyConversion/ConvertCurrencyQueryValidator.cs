using FluentValidation;

namespace CltPrueba.Api.Application.CurrencyConversion;

public class ConvertCurrencyQueryValidator : AbstractValidator<ConvertCurrencyQuery>
{
    public ConvertCurrencyQueryValidator()
    {
        RuleFor(query => query.FromCurrencyCode)
            .NotEmpty().WithMessage("La moneda de origen es requerida");
        RuleFor(query => query.ToCurrencyCode)
            .NotEmpty().WithMessage("La moneda de destino es requerida");
        RuleFor(query => query.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0");
    }
}
