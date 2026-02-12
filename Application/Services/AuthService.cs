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
                        Message = "Usuario ou senha invalidos",
                        Status = "invalid_credentials"
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequestDTO.Password, result.PasswordHash))
                {
                    return new LoginResponseDto
                    {
                        Message = "Usuario ou senha invalidos",
                        Status = "invalid_credentials"
                    };
                }

                var token = _tokenRepository.GenerateToken(result);
                var refreshToken = _tokenRepository.GenerateRefreshToken(result.UserId);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Message = "Login Realizado com Sucesso",
                    Status = "Success",
                    Token = token,
                    RefreshToken = refreshToken.Token
                };
            }
            catch (Exception)
            {
                return new LoginResponseDto
                {
                    Message = "Erro interno",
                    Status = "error",
                    Token = "",
                    RefreshToken = ""
                };
            }
        }
        public async Task<RefreshTokenResponseDTO> RefreshToken(RefreshTokenRequestDTO requestDTO)
        {
            try
            {
                var storedToken = await _refreshTokenRepository.GetByTokenAsync(requestDTO.RefreshToken);

                if (storedToken == null)
                {
                    return new RefreshTokenResponseDTO
                    {
                        Message = "Token de refresh invalido",
                        Status = "invalid_token"
                    };
                }

                if (storedToken.Expires < DateTime.UtcNow)
                {
                    return new RefreshTokenResponseDTO
                    {
                        Message = "Token de refresh expirou",
                        Status = "expired_token"
                    };
                }

                if (storedToken.Revoked != null)
                {
                    var allTokens = await _refreshTokenRepository.GetAllActiveByUserIdAsync(storedToken.UserId);
                    foreach (var token in allTokens)
                    {
                        token.Revoked = DateTime.UtcNow;
                        token.ReasonRevoked = "Compromised token";

                        await _refreshTokenRepository.UpdateAsync(token);
                    }

                    await _refreshTokenRepository.SaveChangesAsync();

                    return new RefreshTokenResponseDTO
                    {
                        Message = "Uso invalido do token detectado",
                        Status = "security_alert"
                    };
                }

                if (storedToken.User == null)
                {
                    return new RefreshTokenResponseDTO
                    {
                        Message = "Erro critico",
                        Status = "not-found"
                    };
                }

                var newAccessToken = _tokenRepository.GenerateToken(storedToken.User);
                var newRefreshToken = _tokenRepository.GenerateRefreshToken(storedToken.UserId);

                storedToken.Revoked = DateTime.UtcNow;
                storedToken.ReplacedByToken = newRefreshToken.Token;
                storedToken.ReasonRevoked = "Replaced by new token";

                await _refreshTokenRepository.UpdateAsync(storedToken);
                await _refreshTokenRepository.AddAsync(newRefreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                return new RefreshTokenResponseDTO
                {
                    Message = "Token renovado com sucesso",
                    Status = "Success",
                    Data = new DataToken
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken.Token
                    }
                };
            }
            catch (Exception ex)
            {
                return new RefreshTokenResponseDTO
                {
                    Message = "Erro interno: " + ex.Message,
                    Status = "error"
                };
            }

        }
    }

}