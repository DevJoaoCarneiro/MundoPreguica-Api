using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.controller
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AuthenticateUser([FromBody] LoginRequestDto loginRequestDTO)
        {
            _logger.LogInformation("Tentativa de login para o usuário: {UserEmail}", loginRequestDTO?.Mail);
            try
            {
                var result = await _authService.AuthenticateLogin(loginRequestDTO);
                return result.Status switch
                {
                    "invalid_credentials" => Unauthorized(result),
                    "error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Falha crítica no endpoint de autenticação para o usuário {UserEmail}", loginRequestDTO?.Mail);
                return StatusCode(500, new
                {
                    Message = "Internal server error",
                    Status = "error"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                var result = await _authService.RefreshToken(refreshTokenRequestDTO);
                return result.Status switch
                {
                    "invalid_token" => Unauthorized(result),
                    "expired_token" => Unauthorized(result),
                    "security_alert" => Unauthorized(result),
                    "not-found" => StatusCode(404, result),
                    "error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no endpoint de refresh token.");
                return StatusCode(500, new
                {
                    Message = "Internal server error",
                    Status = "error"
                });
            }
        }


    }
}