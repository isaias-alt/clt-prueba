using FluentValidation;

namespace CltPrueba.Api.Application.Currencies.Commands;

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("El codigo es requerido")
            .MaximumLength(10).WithMessage("El codigo admite hasta 10 caracteres");
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre admite hasta 100 caracteres");
        RuleFor(command => command.RateToBase)
            .GreaterThan(0).WithMessage("La tasa debe ser mayor a 0")
            .PrecisionScale(18, 6, true).WithMessage("La tasa admite hasta 18 digitos en total y 6 decimales");
    }
}
