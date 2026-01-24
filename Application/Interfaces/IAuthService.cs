using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> AuthenticateLogin(LoginRequestDto loginRequestDTO);

        Task<RefreshTokenResponseDTO> RefreshToken(RefreshTokenRequestDTO refreshTokenRequestDTO);
    }
}
