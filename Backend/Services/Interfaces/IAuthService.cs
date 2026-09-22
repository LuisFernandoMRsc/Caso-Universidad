using CampusConnect.Api.DTOs;

namespace CampusConnect.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<UserProfileDto> GetProfileAsync(int userId);
}
