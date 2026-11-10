using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBackend.Data;
using TicketsBackend.DTOs;
using TicketsBackend.Models;
using TicketsBackend.Services;

namespace TicketsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Login == request.Login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Login ou senha inválidos." });

        return Ok(new LoginResponse
        {
            Token = _tokenService.GenerateToken(user),
            User = ToUserInfo(user)
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfoResponse>> Me()
    {
        var userId = User.GetUserId();
        var user = await _db.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return Unauthorized();
        return Ok(ToUserInfo(user));
    }

    private static UserInfoResponse ToUserInfo(User user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        Name = user.Name,
        IsAdmin = user.IsAdmin,
        DepartmentId = user.DepartmentId,
        DepartmentName = user.Department?.Name ?? string.Empty
    };
}
