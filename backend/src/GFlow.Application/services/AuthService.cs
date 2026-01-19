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

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userRepo.GetByUsername(request.username);
            if (user == null) return null;

            bool isPasswordValid = _passwordHasher.VerifyPassword(request.password, user.PasswordHash);
            if (!isPasswordValid) return null;

            user.Token = _tokenProvider.GenerateToken(user.Id, user.Username);
            
            AuthResponse response = new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = user.Token
            };

            return response;
        }

        public async Task<AuthResponse> RegisterUser(RegisterRequest request)
        {
            // 4. Walidacja podstawowa
            if (string.IsNullOrWhiteSpace(request.email) || 
                string.IsNullOrWhiteSpace(request.username) || 
                string.IsNullOrWhiteSpace(request.password))
            {
                return null;
            }

            if (request.username.Length < 3) return null;

            // 5. Sprawdzenie formatu email
            if (!EmailRegex.IsMatch(request.email)) return null;

            // 6. Sprawdzenie czy użytkownik już istnieje (Unikalność)
            var existingUser = await _userRepo.GetByUsername(request.username);
            if (existingUser != null) return null;

            // 7. Haszowanie hasła
            string passwordHash = _passwordHasher.HashPassword(request.password);

            var user = new User
            {
                Email = request.email,
                Username = request.username,
                PasswordHash = passwordHash
            };

            // 8. Zapis do bazy
            var createdUser = await _userRepo.Add(user);
            if (createdUser == null) return null;

            // 9. Generowanie tokenu dla nowo zarejestrowanego użytkownika
            createdUser.Token = _tokenProvider.GenerateToken(createdUser.Id, createdUser.Username);

            AuthResponse response = new AuthResponse
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email,
                Token = createdUser.Token
            };

            return response;
        }
    }
}