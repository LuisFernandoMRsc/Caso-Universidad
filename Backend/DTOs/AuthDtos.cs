using System.ComponentModel.DataAnnotations;
using CampusConnect.Api.Models.Enums;

namespace CampusConnect.Api.DTOs;

public class RegisterRequestDto
{
    [Required]
    [MaxLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? CarnetCodigo { get; set; }
    public string? Telefono { get; set; }
    public RolUsuario? Rol { get; set; } = RolUsuario.ESTUDIANTE;
}

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public UserProfileDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string? CarnetCodigo { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string? Telefono { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalSolicitudes { get; set; }
    public int TotalAsignaciones { get; set; }
}
