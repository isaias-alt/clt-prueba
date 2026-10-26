using FluentValidation;

namespace CltPrueba.Api.Application.Users.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200);
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Debe ingresar un email valido")
            .MaximumLength(320);
        RuleFor(command => command.Password)
            .MinimumLength(8).WithMessage("El password debe tener al menos 8 caracteres")
            .When(command => command.Password is not null );
    }
}