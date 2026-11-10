namespace TicketsBackend.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    // false (0) = usuário comum, true (1) = administrador
    public bool IsAdmin { get; set; }
}
