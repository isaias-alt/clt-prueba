using FluentValidation;

namespace CltPrueba.Api.Application.Users.Commands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200);
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Debe ingresar un email valido")
            .MaximumLength(320);
        RuleFor(command => command.IsActive)
            .NotNull().WithMessage("isActive es requerido");
    }
}