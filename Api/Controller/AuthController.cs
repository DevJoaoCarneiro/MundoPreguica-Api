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
                    detail = ex.Message
                });
            }
        }

       
    }
}