namespace CltPrueba.Api.Domain;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Address> Addresses { get; set; } = [];
}