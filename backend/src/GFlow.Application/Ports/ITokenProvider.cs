using System.Security.Claims;

namespace GFlow.Application.Ports
{
    public interface ITokenProvider
    {
        string GenerateToken(string userId, string username);

        ClaimsPrincipal? ValidateToken(string token);
    }
}