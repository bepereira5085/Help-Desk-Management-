using System.ComponentModel.DataAnnotations;

namespace TicketsBackend.DTOs;

public class DepartmentDto
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Color { get; set; } = string.Empty;
}

public class TagDto
{
    [Required, MaxLength(50)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Color { get; set; } = string.Empty;
}
