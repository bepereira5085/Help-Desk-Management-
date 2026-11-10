using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBackend.Data;
using TicketsBackend.DTOs;
using TicketsBackend.Models;

namespace TicketsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TagsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> GetAll() =>
        await _db.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse { Id = t.Id, Name = t.Name, Color = t.Color })
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TagResponse>> GetById(int id)
    {
        var tag = await _db.Tags.FindAsync(id);
        return tag == null ? NotFound() : Ok(ToResponse(tag));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create(TagDto dto)
    {
        var tag = new Tag { Name = dto.Name, Color = dto.Color };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, ToResponse(tag));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<TagResponse>> Update(int id, TagDto dto)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return NotFound();

        tag.Name = dto.Name;
        tag.Color = dto.Color;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(tag));
    }

    // Os vínculos em TicketTags são removidos em cascata.
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return NotFound();

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static TagResponse ToResponse(Tag tag) =>
        new() { Id = tag.Id, Name = tag.Name, Color = tag.Color };
}
