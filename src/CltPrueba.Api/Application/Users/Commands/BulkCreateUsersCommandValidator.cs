using FluentValidation;

namespace CltPrueba.Api.Application.Users.Commands;

public class BulkCreateUsersCommandValidator : AbstractValidator<BulkCreateUsersCommand>
{
    private const int MaxUsers = 1000;

    public BulkCreateUsersCommandValidator()
    {
        RuleFor(command => command.Users)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Debe enviar al menos un usuario")
            .Must(users => users.Count <= MaxUsers).WithMessage($"Se admiten hasta {MaxUsers} usuarios por solicitud");
    }
}
