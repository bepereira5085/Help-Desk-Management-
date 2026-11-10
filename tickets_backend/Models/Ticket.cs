using TicketsBackend.Enums;

namespace TicketsBackend.Models;

public class Ticket
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public DateTime CreatedAt { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Aberto;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public string? EquipmentTag { get; set; }

    public List<TicketTag> TicketTags { get; set; } = new();
}
