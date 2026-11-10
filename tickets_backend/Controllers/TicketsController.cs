using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBackend.Data;
using TicketsBackend.DTOs;
using TicketsBackend.Enums;
using TicketsBackend.Models;

namespace TicketsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TicketsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista chamados ordenados por prioridade (Urgente → Baixa) e, depois, data (mais recente primeiro). Usuário comum vê apenas os seus;
    /// admin vê todos e pode filtrar também por data e setor.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TicketResponse>>> GetAll(
        [FromQuery] DateTime? date,
        [FromQuery] int? departmentId,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] string? itemName)
    {
        var query = BaseQuery();

        if (User.IsAdmin())
        {
            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(t => t.CreatedAt >= start && t.CreatedAt < end);
            }

            if (departmentId.HasValue)
                query = query.Where(t => t.DepartmentId == departmentId.Value);
        }
        else
        {
            var userId = User.GetUserId();
            query = query.Where(t => t.UserId == userId);
        }

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            var term = itemName.Trim();
            query = query.Where(t => t.ItemName.Contains(term));
        }

        var tickets = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
        return Ok(tickets.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketResponse>> GetById(int id)
    {
        var ticket = await BaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null || !CanAccess(ticket)) return NotFound();
        return Ok(ToResponse(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(CreateTicketRequest request)
    {
        if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
            return BadRequest(new { message = "Setor inválido." });

        var tagIds = request.TagIds.Distinct().ToList();
        var existingTags = await _db.Tags.CountAsync(t => tagIds.Contains(t.Id));
        if (existingTags != tagIds.Count)
            return BadRequest(new { message = "Uma ou mais tags são inválidas." });

        var ticket = new Ticket
        {
            UserId = User.GetUserId(),
            DepartmentId = request.DepartmentId,
            CreatedAt = DateTime.Now,
            ItemName = request.ItemName,
            Description = request.Description,
            Priority = request.Priority,
            EquipmentTag = string.IsNullOrWhiteSpace(request.EquipmentTag) ? null : request.EquipmentTag,
            Status = TicketStatus.Aberto,
            TicketTags = tagIds.Select(tagId => new TicketTag { TagId = tagId }).ToList()
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        var created = await BaseQuery().FirstAsync(t => t.Id == ticket.Id);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ToResponse(created));
    }

    /// <summary>
    /// Edita o chamado. O dono só edita enquanto estiver "Aberto"; o admin edita sempre
    /// e é o único que pode alterar o status por aqui.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TicketResponse>> Update(int id, UpdateTicketRequest request)
    {
        var ticket = await _db.Tickets
            .Include(t => t.TicketTags)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null || !CanAccess(ticket)) return NotFound();

        var isAdmin = User.IsAdmin();
        if (!isAdmin && ticket.Status != TicketStatus.Aberto)
            return BadRequest(new { message = "Só é possível editar chamados com status Aberto." });

        if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
            return BadRequest(new { message = "Setor inválido." });

        var tagIds = request.TagIds.Distinct().ToList();
        var existingTags = await _db.Tags.CountAsync(t => tagIds.Contains(t.Id));
        if (existingTags != tagIds.Count)
            return BadRequest(new { message = "Uma ou mais tags são inválidas." });

        ticket.DepartmentId = request.DepartmentId;
        ticket.ItemName = request.ItemName;
        ticket.Description = request.Description;
        ticket.Priority = request.Priority;
        ticket.EquipmentTag = string.IsNullOrWhiteSpace(request.EquipmentTag) ? null : request.EquipmentTag;

        if (isAdmin && request.Status.HasValue)
            ticket.Status = request.Status.Value;

        ticket.TicketTags.Clear();
        foreach (var tagId in tagIds)
            ticket.TicketTags.Add(new TicketTag { TicketId = ticket.Id, TagId = tagId });

        await _db.SaveChangesAsync();

        var updated = await BaseQuery().FirstAsync(t => t.Id == id);
        return Ok(ToResponse(updated));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TicketResponse>> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.Status = request.Status;
        await _db.SaveChangesAsync();

        var updated = await BaseQuery().FirstAsync(t => t.Id == id);
        return Ok(ToResponse(updated));
    }

    /// <summary>Cancela (exclui) o chamado, apenas se ainda estiver "Aberto".</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null || !CanAccess(ticket)) return NotFound();

        if (ticket.Status != TicketStatus.Aberto)
            return BadRequest(new { message = "Só é possível cancelar chamados com status Aberto." });

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Ticket> BaseQuery() =>
        _db.Tickets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Department)
            .Include(t => t.TicketTags).ThenInclude(tt => tt.Tag);

    private bool CanAccess(Ticket ticket) =>
        User.IsAdmin() || ticket.UserId == User.GetUserId();

    private static TicketResponse ToResponse(Ticket t) => new()
    {
        Id = t.Id,
        CreatedAt = t.CreatedAt,
        UserId = t.UserId,
        UserName = t.User?.Name ?? string.Empty,
        DepartmentId = t.DepartmentId,
        DepartmentName = t.Department?.Name ?? string.Empty,
        DepartmentColor = t.Department?.Color ?? string.Empty,
        ItemName = t.ItemName,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        EquipmentTag = t.EquipmentTag,
        Tags = t.TicketTags
            .Where(tt => tt.Tag != null)
            .Select(tt => new TagResponse { Id = tt.Tag!.Id, Name = tt.Tag.Name, Color = tt.Tag.Color })
            .ToList()
    };
}
