using GFlow.Application.DTOs;

namespace GFlow.Application.Ports
{
    public interface IAuthService
    {
        Task<AuthResponse?> Login(LoginRequest request);
        Task<AuthResponse?> RegisterUser(RegisterRequest request);
        Task<AuthResponse?> RefreshToken(string refreshToken);
    }
}