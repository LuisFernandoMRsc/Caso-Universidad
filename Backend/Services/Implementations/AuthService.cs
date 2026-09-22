using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusConnect.Api.Data;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CampusConnect.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
        {
            throw new InvalidOperationException("El correo electrónico ya se encuentra registrado.");
        }

        if (!string.IsNullOrEmpty(dto.CarnetCodigo) &&
            await _context.Usuarios.AnyAsync(u => u.CarnetCodigo == dto.CarnetCodigo))
        {
            throw new InvalidOperationException("El carnet/código universitario ya está registrado.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new Usuario
        {
            NombreCompleto = dto.NombreCompleto,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            CarnetCodigo = dto.CarnetCodigo,
            Telefono = dto.Telefono,
            Rol = dto.Rol ?? RolUsuario.ESTUDIANTE,
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            User = new UserProfileDto
            {
                Id = user.Id,
                CarnetCodigo = user.CarnetCodigo,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email,
                Rol = user.Rol,
                Telefono = user.Telefono,
                CreatedAt = user.CreatedAt
            },
            Token = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

        if (user == null || !user.Activo)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas o cuenta desactivada.");
        }

        bool isMatch = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isMatch)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            User = new UserProfileDto
            {
                Id = user.Id,
                CarnetCodigo = user.CarnetCodigo,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email,
                Rol = user.Rol,
                Telefono = user.Telefono,
                CreatedAt = user.CreatedAt
            },
            Token = token
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _context.Usuarios
            .Include(u => u.SolicitudesCreadas)
            .Include(u => u.Asignaciones)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            CarnetCodigo = user.CarnetCodigo,
            NombreCompleto = user.NombreCompleto,
            Email = user.Email,
            Rol = user.Rol,
            Telefono = user.Telefono,
            CreatedAt = user.CreatedAt,
            TotalSolicitudes = user.SolicitudesCreadas.Count,
            TotalAsignaciones = user.Asignaciones.Count(a => a.Activo)
        };
    }

    private string GenerateJwtToken(Usuario user)
    {
        var secret = _configuration["Jwt:Secret"] ?? "campus_connect_super_secret_jwt_key_2026_universidad";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.NombreCompleto),
            new Claim(ClaimTypes.Role, user.Rol.ToString()),
            new Claim("carnet", user.CarnetCodigo ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "CampusConnect",
            audience: _configuration["Jwt:Audience"] ?? "CampusConnectApp",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
