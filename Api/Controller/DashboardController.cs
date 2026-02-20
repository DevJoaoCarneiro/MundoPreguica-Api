using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Api.Controller
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> getDashboardData([FromQuery] int? year, [FromQuery] int? startMonth, [FromQuery] int? endMonth)
        {
            try
            {
            _logger.LogInformation("Recebendo requisicao para dados do dashboard.");
                var result = await _dashboardService.GetMonthlyDashboardAsync(year, startMonth, endMonth);

                return result.Status switch
                {
                    "success" => Ok(result),
                    "invalid_argument" => BadRequest(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no endpoint de dashboard.");
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado no servidor.",
                    Status = "error"
                });
            }
        }
    }
}
