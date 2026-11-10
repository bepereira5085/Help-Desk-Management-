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
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DepartmentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Department>>> GetAll() =>
        await _db.Departments.OrderBy(d => d.Name).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Department>> GetById(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        return department == null ? NotFound() : Ok(department);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Department>> Create(DepartmentDto dto)
    {
        var department = new Department { Name = dto.Name, Color = dto.Color };
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Department>> Update(int id, DepartmentDto dto)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null) return NotFound();

        department.Name = dto.Name;
        department.Color = dto.Color;
        await _db.SaveChangesAsync();
        return Ok(department);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null) return NotFound();

        var inUse = await _db.Users.AnyAsync(u => u.DepartmentId == id)
                    || await _db.Tickets.AnyAsync(t => t.DepartmentId == id);
        if (inUse)
            return BadRequest(new { message = "Setor em uso por usuários ou chamados; não pode ser excluído." });

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
