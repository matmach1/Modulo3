namespace Riesgos.Api.Domain;

public enum Role
{
    Comun,
    Admin,
}

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Role Role { get; set; } = Role.Comun;
}
