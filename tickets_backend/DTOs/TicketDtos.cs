using System.ComponentModel.DataAnnotations;
using TicketsBackend.Enums;

namespace TicketsBackend.DTOs;

public class CreateTicketRequest
{
    [Required] public int DepartmentId { get; set; }
    [Required, MaxLength(150)] public string ItemName { get; set; } = string.Empty;
    [Required, MaxLength(2000)] public string Description { get; set; } = string.Empty;
    [EnumDataType(typeof(TicketPriority))] public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    [MaxLength(50)] public string? EquipmentTag { get; set; }
    public List<int> TagIds { get; set; } = new();
}

public class UpdateTicketRequest : CreateTicketRequest
{
    [EnumDataType(typeof(TicketStatus))] public TicketStatus? Status { get; set; }
}

public class UpdateStatusRequest
{
    [Required, EnumDataType(typeof(TicketStatus))] public TicketStatus Status { get; set; }
}

public class TagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class TicketResponse
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentColor { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string? EquipmentTag { get; set; }
    public List<TagResponse> Tags { get; set; } = new();
}
