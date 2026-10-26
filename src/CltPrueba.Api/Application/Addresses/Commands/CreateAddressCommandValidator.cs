using FluentValidation;

namespace CltPrueba.Api.Application.Addresses.Commands;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(command => command.Street)
            .NotEmpty().WithMessage("La calle es requerida")
            .MaximumLength(200).WithMessage("La calle admite hasta 200 caracteres");
        RuleFor(command => command.City)
            .NotEmpty().WithMessage("La ciudad es requerida")
            .MaximumLength(100).WithMessage("La ciudad admite hasta 100 caracteres");
        RuleFor(command => command.Country)
            .NotEmpty().WithMessage("El pais es requerido")
            .MaximumLength(100).WithMessage("El pais admite hasta 100 caracteres");
        RuleFor(command => command.ZipCode)
            .MaximumLength(20).WithMessage("El codigo postal admite hasta 20 caracteres");
    }
}
