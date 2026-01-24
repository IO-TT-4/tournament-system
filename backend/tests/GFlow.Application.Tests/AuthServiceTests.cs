using Xunit;
using Moq;
using GFlow.Application.Services;
using GFlow.Application.Ports;
using GFlow.Application.DTOs;
using GFlow.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace GFlow.Application.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenProvider> _tokenProviderMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenProviderMock = new Mock<ITokenProvider>();
            _authService = new AuthService(_userRepoMock.Object, _passwordHasherMock.Object, _tokenProviderMock.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new User { Username = "test", PasswordHash = "hash", Email = "test@example.com" };
            _userRepoMock.Setup(x => x.GetByUsername("test")).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.VerifyPassword("password", "hash")).Returns(true);
            _tokenProviderMock.Setup(x => x.GenerateToken(It.IsAny<string>(), "test")).Returns("access_token");
            _tokenProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

            // Act
            var result = await _authService.Login(new LoginRequest { username = "test", password = "password" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access_token", result.Token);
            Assert.Equal("refresh_token", result.RefreshToken);
            Assert.Equal("refresh_token", user.RefreshToken);
            _userRepoMock.Verify(x => x.Update(user), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnTokens_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var request = new RegisterRequest { username = "newuser", password = "password", email = "new@example.com" };
            _userRepoMock.Setup(x => x.GetByUsername("newuser")).ReturnsAsync((User?)null);
            _passwordHasherMock.Setup(x => x.HashPassword("password")).Returns("hash");
            
            var createdUser = new User { Username = "newuser", PasswordHash = "hash", Email = "new@example.com", Id = "new_id" };
            _userRepoMock.Setup(x => x.Add(It.IsAny<User>())).ReturnsAsync(createdUser);
            
            _tokenProviderMock.Setup(x => x.GenerateToken("new_id", "newuser")).Returns("access_token");
            _tokenProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access_token", result.Token);
            Assert.Equal("refresh_token", result.RefreshToken);
            Assert.Equal("refresh_token", createdUser.RefreshToken);
            _userRepoMock.Verify(x => x.Update(createdUser), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var user = new User 
            { 
                Username = "test", 
                PasswordHash = "hash", 
                RefreshToken = "valid_refresh",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            
            _userRepoMock.Setup(x => x.GetByRefreshToken("valid_refresh")).ReturnsAsync(user);
            _tokenProviderMock.Setup(x => x.GenerateToken(It.IsAny<string>(), "test")).Returns("new_access");
            _tokenProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("new_refresh");

            // Act
            var result = await _authService.RefreshToken("valid_refresh");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new_access", result.Token);
            Assert.Equal("new_refresh", result.RefreshToken);
            Assert.Equal("new_refresh", user.RefreshToken);
            _userRepoMock.Verify(x => x.Update(user), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnNull_WhenRefreshTokenIsExpired()
        {
            // Arrange
             var user = new User 
            { 
                Username = "test", 
                PasswordHash = "hash", 
                RefreshToken = "expired_refresh",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };
            
            _userRepoMock.Setup(x => x.GetByRefreshToken("expired_refresh")).ReturnsAsync(user);

            // Act
            var result = await _authService.RefreshToken("expired_refresh");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnNull_WhenRefreshTokenDoesNotMatch()
        {
             // Arrange
             var user = new User 
            { 
                Username = "test", 
                PasswordHash = "hash", 
                RefreshToken = "actual_refresh",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            
            // Simulating repo returning user but token doesn't match (unlikely if GetByRefreshToken works, but possible if changed concurrently or implementation differs)
            // Actually implementation calls GetByRefreshToken, so checking that it returns null if not found is key.
            // But if it returns a user, the service checks user.RefreshToken != refreshToken.
            _userRepoMock.Setup(x => x.GetByRefreshToken("wrong_refresh")).ReturnsAsync(user);

            // Act
            var result = await _authService.RefreshToken("wrong_refresh");

            // Assert
            Assert.Null(result);
        }
    }
}
