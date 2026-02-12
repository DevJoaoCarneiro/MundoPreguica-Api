using Application.Interfaces;
using Application.Request;
using Application.Services;
using Domain.entities;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace Tests.Services
{
    public class AuthServiceTest
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
        private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        private readonly AuthService _service;

        public AuthServiceTest()
        {
            _service = new AuthService(_userRepository, _tokenService, _refreshTokenRepository);
        }

        [Fact]
        public async Task AuthenticateLogin_WhenUserNotFound_ReturnsInvalidCredentials()
        {
            _userRepository.GetByEmailAsync(Arg.Any<string>()).ReturnsNull();

            var result = await _service.AuthenticateLogin(new LoginRequestDto
            {
                Mail = "user@test.com",
                Password = "123456"
            });

            Assert.Equal("invalid_credentials", result.Status);
            Assert.Equal("Usuario ou senha invalidos", result.Message);
        }

        [Fact]
        public async Task AuthenticateLogin_WhenPasswordInvalid_ReturnsInvalidCredentials()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "user@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("valid-password")
            };

            _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);

            var result = await _service.AuthenticateLogin(new LoginRequestDto
            {
                Mail = user.Email,
                Password = "invalid-password"
            });

            Assert.Equal("invalid_credentials", result.Status);
            Assert.Equal("Usuario ou senha invalidos", result.Message);
        }

        [Fact]
        public async Task AuthenticateLogin_WhenValidCredentials_ReturnsTokens()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "user@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("valid-password")
            };

            _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
            _tokenService.GenerateToken(user).Returns("access-token");
            _tokenService.GenerateRefreshToken(user.UserId).Returns(new RefreshToken
            {
                Token = "refresh-token",
                UserId = user.UserId,
                Expires = DateTime.UtcNow.AddDays(1)
            });

            var result = await _service.AuthenticateLogin(new LoginRequestDto
            {
                Mail = user.Email,
                Password = "valid-password"
            });

            Assert.Equal("Success", result.Status);
            Assert.Equal("Login Realizado com Sucesso", result.Message);
            Assert.Equal("access-token", result.Token);
            Assert.Equal("refresh-token", result.RefreshToken);

            await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>());
            await _refreshTokenRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AuthenticateLogin_WhenExceptionThrown_ReturnsError()
        {
            _userRepository.GetByEmailAsync(Arg.Any<string>()).ThrowsAsync(new Exception("db error"));

            var result = await _service.AuthenticateLogin(new LoginRequestDto
            {
                Mail = "user@test.com",
                Password = "123"
            });

            Assert.Equal("error", result.Status);
            Assert.Equal("Erro interno", result.Message);
        }

        [Fact]
        public async Task RefreshToken_WhenTokenNotFound_ReturnsInvalidToken()
        {
            _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>()).ReturnsNull();

            var result = await _service.RefreshToken(new RefreshTokenRequestDTO
            {
                RefreshToken = "invalid"
            });

            Assert.Equal("invalid_token", result.Status);
            Assert.Equal("Token de refresh invalido", result.Message);
        }

        [Fact]
        public async Task RefreshToken_WhenTokenExpired_ReturnsExpiredToken()
        {
            _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>()).Returns(new RefreshToken
            {
                Token = "refresh",
                UserId = Guid.NewGuid(),
                Expires = DateTime.UtcNow.AddMinutes(-1)
            });

            var result = await _service.RefreshToken(new RefreshTokenRequestDTO
            {
                RefreshToken = "refresh"
            });

            Assert.Equal("expired_token", result.Status);
            Assert.Equal("Token de refresh expirou", result.Message);
        }

        [Fact]
        public async Task RefreshToken_WhenTokenRevoked_ReturnsSecurityAlert()
        {
            var userId = Guid.NewGuid();
            var revokedToken = new RefreshToken
            {
                Token = "refresh",
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(1),
                Revoked = DateTime.UtcNow
            };

            _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>()).Returns(revokedToken);
            _refreshTokenRepository.GetAllActiveByUserIdAsync(userId)
                .Returns(new List<RefreshToken>
                {
                    new RefreshToken { Token = "t1", UserId = userId },
                    new RefreshToken { Token = "t2", UserId = userId }
                });

            var result = await _service.RefreshToken(new RefreshTokenRequestDTO
            {
                RefreshToken = "refresh"
            });

            Assert.Equal("security_alert", result.Status);
            Assert.Equal("Uso invalido do token detectado", result.Message);

            await _refreshTokenRepository.Received(2).UpdateAsync(Arg.Any<RefreshToken>());
            await _refreshTokenRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RefreshToken_WhenUserMissing_ReturnsNotFound()
        {
            _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>()).Returns(new RefreshToken
            {
                Token = "refresh",
                UserId = Guid.NewGuid(),
                Expires = DateTime.UtcNow.AddDays(1),
                User = null
            });

            var result = await _service.RefreshToken(new RefreshTokenRequestDTO
            {
                RefreshToken = "refresh"
            });

            Assert.Equal("not-found", result.Status);
            Assert.Equal("Erro critico", result.Message);
        }

        [Fact]
        public async Task RefreshToken_WhenValid_ReturnsNewTokens()
        {
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId, Email = "user@test.com" };
            var storedToken = new RefreshToken
            {
                Token = "refresh",
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(1),
                User = user
            };

            _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>()).Returns(storedToken);
            _tokenService.GenerateToken(user).Returns("new-access");
            _tokenService.GenerateRefreshToken(userId).Returns(new RefreshToken
            {
                Token = "new-refresh",
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(1)
            });

            var result = await _service.RefreshToken(new RefreshTokenRequestDTO
            {
                RefreshToken = "refresh"
            });

            Assert.Equal("Success", result.Status);
            Assert.Equal("Token renovado com sucesso", result.Message);
            Assert.Equal("new-access", result.Data.AccessToken);
            Assert.Equal("new-refresh", result.Data.RefreshToken);

            await _refreshTokenRepository.Received(1).UpdateAsync(Arg.Any<RefreshToken>());
            await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>());
            await _refreshTokenRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RefreshToken_WhenExceptionThrown_ReturnsError()
        {
            _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>()).ThrowsAsync(new Exception("db error"));

            var result = await _service.RefreshToken(new RefreshTokenRequestDTO
            {
                RefreshToken = "refresh"
            });

            Assert.Equal("error", result.Status);
            Assert.StartsWith("Erro interno", result.Message);
        }
    }
}
