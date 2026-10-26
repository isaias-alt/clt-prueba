using CltPrueba.Api.Application.Users.Dto;
using FluentValidation;

namespace CltPrueba.Api.Application.Users.Commands;

public class BulkCreateUsersHandler
{
    private readonly CreateUserHandler _createUserHandler;
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly ILogger<BulkCreateUsersHandler> _logger;
    private readonly int _maxDegreeOfParallelism;

    public BulkCreateUsersHandler(
        CreateUserHandler createUserHandler,
        IValidator<CreateUserCommand> validator,
        ILogger<BulkCreateUsersHandler> logger,
        IConfiguration configuration)
    {
        _createUserHandler = createUserHandler;
        _validator = validator;
        _logger = logger;
        _maxDegreeOfParallelism = Math.Max(1, configuration.GetValue<int?>("Bulk:MaxDegreeOfParallelism") ?? 4);
    }

    public async Task<BulkCreateUsersResultDto> HandleAsync(BulkCreateUsersCommand command, CancellationToken cancellationToken)
    {
        var failures = new List<BulkUserFailureDto>();
        var candidates = new List<(int Index, CreateUserCommand User)>();
        var firstIndexByEmail = new Dictionary<string, int>();

        for (var index = 0; index < command.Users.Count; index++)
        {
            var user = command.Users[index];
            if (user is null)
            {
                failures.Add(new BulkUserFailureDto(index, null, "El elemento esta vacio"));
                continue;
            }

            var validation = await _validator.ValidateAsync(user, cancellationToken);
            if (!validation.IsValid)
            {
                var reason = string.Join("; ", validation.Errors.Select(error => error.ErrorMessage));
                failures.Add(new BulkUserFailureDto(index, user.Email, reason));
                continue;
            }

            var email = user.Email.Trim().ToLowerInvariant();
            if (firstIndexByEmail.TryGetValue(email, out var firstIndex))
            {
                failures.Add(new BulkUserFailureDto(index, user.Email, $"Email repetido en el arreglo (primera aparicion en la posicion {firstIndex})"));
                continue;
            }

            firstIndexByEmail[email] = index;
            candidates.Add((index, user));
        }

        using var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);

        var tasks = candidates.Select(candidate => Task.Run(() => CreateUserAsync(candidate.Index, candidate.User, semaphore, cancellationToken), cancellationToken));
        var outcomes = await Task.WhenAll(tasks);

        var created = outcomes
            .Where(outcome => outcome.Created is not null)
            .OrderBy(outcome => outcome.Index)
            .Select(outcome => outcome.Created!)
            .ToList();

        failures.AddRange(outcomes
            .Where(outcome => outcome.Failure is not null)
            .Select(outcome => new BulkUserFailureDto(outcome.Index, outcome.Email, outcome.Failure!)));

        failures = failures.OrderBy(failure => failure.Index).ToList();

        return new BulkCreateUsersResultDto(command.Users.Count, created.Count, failures.Count, created, failures);
    }

    private async Task<Outcome> CreateUserAsync(int index, CreateUserCommand user, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var result = await _createUserHandler.HandleAsync(user, cancellationToken);
            if (result.IsSuccess)
            {
                return new Outcome(index, user.Email, result.Value, null);
            }
            else
            {
                return new Outcome(index, user.Email, null, result.Error);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error al crear el usuario de la posicion {Index} del bulk", index);
            return new Outcome(index, user.Email, null, "Error inesperado al guardar el usuario");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private record Outcome(int Index, string Email, UserDto? Created, string? Failure);
}
