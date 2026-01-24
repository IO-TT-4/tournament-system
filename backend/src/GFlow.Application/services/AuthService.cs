using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GFlow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenProvider _tokenProvider;

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public AuthService(IUserRepository userRepo, IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
        }

        public async Task<AuthResponse?> Login(LoginRequest request)
        {
            var user = await _userRepo.GetByUsername(request.username);
            if (user == null) return null;

            bool isPasswordValid = _passwordHasher.VerifyPassword(request.password, user.PasswordHash);
            if (!isPasswordValid) return null;

            string accessToken = _tokenProvider.GenerateToken(user.Id, user.Username);
            string refreshToken = _tokenProvider.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _userRepo.Update(user);

            AuthResponse response = new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? string.Empty,
                Token = accessToken,
                RefreshToken = refreshToken
            };

            return response;
        }

        public async Task<AuthResponse?> RegisterUser(RegisterRequest request)
        {
            // 4. Basic validation
            if (string.IsNullOrWhiteSpace(request.email) || 
                string.IsNullOrWhiteSpace(request.username) || 
                string.IsNullOrWhiteSpace(request.password))
            {
                return null;
            }

            if (request.username.Length < 3) return null;

            // 5. Check email format
            if (!EmailRegex.IsMatch(request.email)) return null;

            // 6. Check if user already exists (Uniqueness)
            var existingUser = await _userRepo.GetByUsername(request.username);
            if (existingUser != null) return null;

            // 7. Password hashing
            string passwordHash = _passwordHasher.HashPassword(request.password);

            var user = new User
            {
                Email = request.email,
                Username = request.username,
                PasswordHash = passwordHash
            };

            // 8. Save to database
            var createdUser = await _userRepo.Add(user);
            if (createdUser == null) return null;

            // 9. Generate token for newly registered user
            string accessToken = _tokenProvider.GenerateToken(createdUser.Id, createdUser.Username);
            string refreshToken = _tokenProvider.GenerateRefreshToken();

            createdUser.RefreshToken = refreshToken;
            createdUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _userRepo.Update(createdUser);

            AuthResponse response = new AuthResponse
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email ?? string.Empty,
                Token = accessToken,
                RefreshToken = refreshToken
            };

            return response;
        }

        public async Task<AuthResponse?> RefreshToken(string refreshToken)
        {
            var user = await _userRepo.GetByRefreshToken(refreshToken);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            string newAccessToken = _tokenProvider.GenerateToken(user.Id, user.Username);
            string newRefreshToken = _tokenProvider.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepo.Update(user);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? string.Empty,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}