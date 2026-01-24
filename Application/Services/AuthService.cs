using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Interfaces;
using Domain.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(IUserRepository userRepository, ITokenService tokenRepository, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }


        public async Task<LoginResponseDto> AuthenticateLogin(LoginRequestDto loginRequestDTO)
        {
            try
            {
                var result = await _userRepository.GetByEmailAsync(loginRequestDTO.Mail);

                if (result == null)
                {
                    return new LoginResponseDto
                    {
                        Message = "User not found",
                        Status = "invalid_credentials"
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequestDTO.Password, result.PasswordHash))
                {
                    return new LoginResponseDto
                    {
                        Message = "Invalid password",
                        Status = "invalid_credentials"
                    };
                }

                var token = _tokenRepository.GenerateToken(result);
                var refreshToken = _tokenRepository.GenerateRefreshToken(result.UserId);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Message = "Login successful",
                    Status = "Success",
                    Token = token,
                    RefreshToken = refreshToken.Token
                };
            }
            catch (Exception)
            {
                return new LoginResponseDto
                {
                    Message = "Internal Error",
                    Status = "error",
                    Token = "",
                    RefreshToken = ""
                };
            }

        }
    }
}